using System.Collections.Generic;
using KBCore.Refs;
using Project.Scripts.FSM;
using Project.Scripts.FSM.Predicate;
using Project.Scripts.FSM.State;
using Project.Scripts.FSM.State.PlayerState;
using Project.Scripts.Input;
using Project.Scripts.Utils;
using Unity.Cinemachine;
using UnityEngine;
using static Project.Scripts.Utils.Timer;

namespace Project.Scripts.Controllers.Player
{
    public class PlayerController : ValidatedMonoBehaviour
    {
        [Header("References")] 
        [SerializeField, Self] private Rigidbody rb;
        [SerializeField, Self] private Animator animator;
        [SerializeField, Self] private GroundChecker groundChecker;
        [SerializeField, Anywhere] private CinemachineCamera freeLookVCam;
        [SerializeField, Anywhere] private InputReader input;

        [Header("Movement Settings")] 
        [SerializeField] private float moveSpeed = 0f;
        [SerializeField] private float rotationSpeed = 15f;
        [SerializeField] private float smoothTime = 0.2f;

        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float jumpDuration = 0.5f;
        [SerializeField] private float jumpCooldown = 0f;
        [SerializeField] private float gravityMultiplier = 3f;

        [Header("Dash Settings")] 
        [SerializeField] private float dashDuration = 0.5f;
        [SerializeField] private float dashCooldown = 5f;
        [SerializeField] private float dashForce = 10f;

        [Header("Attack Settings")] 
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private float attackDistance = 1f;
        [SerializeField] private float attackDamage = 10f;
        
        // Components
        private Transform mainCam;
        
        // Movement Parameters
        private float currentSpeed;
        private float currentSpeedVelocity;
        private float jumpVelocity;
        private float dashVelocity = 1f;
        
        private Vector3 movement;
        
        // Timers
        List<Timer> timers;
        private CountdownTimer jumpTimer;
        private CountdownTimer jumpCooldownTimer;
        private CountdownTimer dashTimer;
        private CountdownTimer dashCooldownTimer;
        private CountdownTimer attackTimer;
        
        // State Machine
        private StateMachine stateMachine;

        // Constants & Statics
        private const float ZeroF = 0f;
        static readonly int Speed = Animator.StringToHash("Speed");
        
        private void Awake()
        {
            // Set Components
            mainCam = Camera.main.transform; 
            freeLookVCam.Follow = transform; 
            freeLookVCam.LookAt = transform;
            freeLookVCam.OnTargetObjectWarped(transform, transform.position - freeLookVCam.transform.position - Vector3.forward);

            rb.freezeRotation = true;
            
            SetupTimers();
            SetupStateMachine();
        }
        
        private void OnEnable()
        {
            input.Jump += OnJump;
            input.Dash += OnDash;
            input.Attack += OnAttack;
        }

        private void Start()
        {
            input.EnablePlayerActions();
        }

        private void Update()
        {
            movement = new Vector3(input.Direction.x, 0f, input.Direction.y);
            stateMachine.Update();
            
            HandleTimers();
            UpdateAnimator();
        }

        private void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }
        
        private void OnDisable()
        {
            input.Jump -= OnJump;
            input.Dash -= OnDash;
            input.Attack -= OnAttack;
        }
        
        private void SetupStateMachine()
        {
            // State Machine
            stateMachine = new StateMachine();

            // Declare States
            var locomotionState = new LocomotionState(this, animator);
            var jumpState = new JumpState(this, animator);
            var dashState = new DashState(this, animator);
            var attackState = new AttackState(this, animator);
            
            // Define Transitions
            At(locomotionState, jumpState, new FuncPredicate(() => jumpTimer.IsRunning));
            At(locomotionState, dashState, new FuncPredicate(() => dashTimer.IsRunning));
            At(locomotionState, attackState, new FuncPredicate(() => attackTimer.IsRunning));
            At(attackState, locomotionState, new FuncPredicate(() => !attackTimer.IsRunning));
            Any(locomotionState, new FuncPredicate(ReturnToLocomotionState));
            
            // Set Initial State
            stateMachine.SetState(locomotionState);
        }

        private bool ReturnToLocomotionState()
        {
            return groundChecker.IsGrounded 
                   && !jumpTimer.IsRunning 
                   && !dashTimer.IsRunning 
                   && !attackTimer.IsRunning;
        }

        private void SetupTimers()
        {
            // Setup Timers
            jumpTimer = new CountdownTimer(jumpDuration);
            jumpCooldownTimer = new CountdownTimer(jumpCooldown);
            jumpTimer.OnTimerStart += () => jumpVelocity = jumpForce;
            jumpTimer.OnTimerStop += () => jumpCooldownTimer.Start();

            dashTimer = new CountdownTimer(dashDuration);
            dashCooldownTimer = new CountdownTimer(dashCooldown);
            dashTimer.OnTimerStart += () => dashVelocity = dashForce;
            dashTimer.OnTimerStop += () =>
            {
                dashVelocity = 1f;
                dashCooldownTimer.Start();
            };

            attackTimer = new CountdownTimer(attackCooldown);
            
            timers = new List<Timer>(5){jumpTimer, jumpCooldownTimer, dashTimer, dashCooldownTimer, attackTimer};
        }

        private void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        private void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);
        
        public void HandleJump()
        {
            // if not jumping and grounded, keep jump velocity at 0
            if (!jumpTimer.IsRunning && groundChecker.IsGrounded)
            {
                jumpVelocity = ZeroF;
                jumpTimer.Stop();
                return;
            }
            
            // if jumping or falling calculate velocity
            if (!jumpTimer.IsRunning) {
                // Gravity takes over
                jumpVelocity += Physics.gravity.y * gravityMultiplier * Time.fixedDeltaTime;
            }
            
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpVelocity, rb.linearVelocity.z);
        }

        public void HandleMovement()
        {
            var adjustedDirection = Quaternion.AngleAxis(mainCam.eulerAngles.y, Vector3.up) * movement;
            
            if (adjustedDirection.magnitude > ZeroF)
            {
                HandleRotation(adjustedDirection);
                HandleHorizontalMovement(adjustedDirection);
                SmoothSpeed(adjustedDirection.magnitude);
            }
            else
            {
                SmoothSpeed(ZeroF);
            }
        }

        public void Attack()
        {
            Vector3 attackPos = transform.position + transform.forward;
            Collider[] hitEnemies = Physics.OverlapSphere(attackPos, attackDistance);

            foreach (var enemy in hitEnemies)
            {
                if (enemy.CompareTag("Enemy"))
                    enemy.GetComponent<Health>().TakeDamage((int)attackDamage);
            }
        }
        
        private void UpdateAnimator()
        {
            animator.SetFloat(Speed, currentSpeed);
        }
        
        private void HandleTimers()
        {
            foreach(var timer in timers) timer.Tick(Time.deltaTime);
        }
        
        private void OnJump(bool performed)
        {
            if (performed && !jumpTimer.IsRunning && !jumpCooldownTimer.IsRunning && groundChecker.IsGrounded)
            {
                jumpTimer.Start();
            } else if (!performed && jumpTimer.IsRunning)
            {
                jumpTimer.Stop();
            }
        }

        private void OnDash(bool performed)
        {
            if (performed && !dashTimer.IsRunning && !dashCooldownTimer.IsRunning)
            {
                dashTimer.Start();
            } else if (!performed && dashTimer.IsRunning)
            {
                dashTimer.Stop();
            }
        }
        
        private void OnAttack()
        {
            if (!attackTimer.IsRunning)
            {
                attackTimer.Start();
            }
        }

        private void HandleHorizontalMovement(Vector3 adjustedDirection)
        {
            // Move the player
            Vector3 velocity = adjustedDirection * (moveSpeed * dashVelocity * Time.fixedDeltaTime);
            rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
        }

        private void HandleRotation(Vector3 adjustedDirection)
        {
            var targetRotation = Quaternion.LookRotation(adjustedDirection);
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            transform.LookAt(transform.position + adjustedDirection);
        }

        private void SmoothSpeed(float value)
        {
            currentSpeed = Mathf.SmoothDamp(currentSpeed, value, ref currentSpeedVelocity, smoothTime);
        }
    }
}