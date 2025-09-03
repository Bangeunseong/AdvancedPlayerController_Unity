using System;
using Project.Scripts.Controllers.Player;
using Project.Scripts.FSM;
using Project.Scripts.FSM.Predicate;
using Project.Scripts.FSM.State;
using Project.Scripts.Input;
using Project.Scripts.Utils;
using UnityEngine;
using static Project.Scripts.Utils.Timer;

namespace Project.Scripts.Controllers.AdvancedPlayer
{
    [RequireComponent(typeof(PlayerMover), typeof(CeilingDetector))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private InputReader input;
        [SerializeField] private Transform cameraTransform;
        
        public float movementSpeed = 7f;
        public float airControlRate = 2f;
        public float jumpSpeed = 10f;
        public float jumpDuration = 0.2f;
        public float airFriction = 0.5f;
        public float groundFriction = 100f;
        public float gravity = 30f;
        public float slideGravity = 5f;
        public float slopeLimit = 30f;
        public bool useLocalMomentum;

        private Transform tr;
        private PlayerMover mover;
        private AnimationController animationController;
        private CeilingDetector ceilingDetector;

        private bool jumpInputIsLocked, jumpKeyWasPressed, jumpKeyWasLetGo, jumpKeyIsPressed;
        
        private StateMachine stateMachine;
        private CountdownTimer jumpTimer;

        private Vector3 momentum, savedVelocity, savedMovementVelocity;
        
        public event Action<Vector3> OnJump = delegate { };
        public event Action<Vector3> OnLand = delegate { };
        
        public Vector3 GetVelocity() => savedVelocity;
        public Vector3 GetMomentum() => useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;
        public Vector3 GetMovementVelocity() => savedMovementVelocity;
        
        private void Awake()
        {
            tr = transform;
            mover = GetComponent<PlayerMover>();
            ceilingDetector = GetComponent<CeilingDetector>();
            animationController = GetComponent<AnimationController>();
            
            jumpTimer = new CountdownTimer(jumpDuration);
            SetupStateMachine();
        }

        private void Start()
        {
            input.EnablePlayerActions();
            input.Jump += HandleJumpKeyInput;
        }

        private void HandleJumpKeyInput(bool isButtonPressed)
        {
            if (!jumpKeyIsPressed && isButtonPressed)
                jumpKeyWasPressed = true;

            if (jumpKeyIsPressed && !isButtonPressed)
            {
                jumpKeyWasLetGo = true;
                jumpInputIsLocked = false;
            }

            jumpKeyIsPressed = isButtonPressed;
        }

        private void Update()
        {
            jumpTimer.Tick(Time.deltaTime);
            stateMachine.Update();
        }

        private void FixedUpdate()
        {
            stateMachine.FixedUpdate();
            mover.CheckForGround();
            HandleMomentum();
            Vector3 velocity = stateMachine.CurrentState is GroundedState ? CalculateMovementVelocity() : Vector3.zero;
            velocity += useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;
            
            mover.SetExtendSensorRange(IsGrounded());
            mover.SetVelocity(velocity);

            savedVelocity = velocity;
            savedMovementVelocity = CalculateMovementVelocity();
            
            ResetJumpKeys();
            
            if (ceilingDetector != null) ceilingDetector.Reset();
        }

        private void SetupStateMachine()
        {
            stateMachine = new StateMachine();

            var grounded = new GroundedState(this, animationController);
            var falling = new FallingState(this, animationController);
            var sliding = new SlidingState(this, animationController);
            var rising = new RisingState(this, animationController);
            var jumping = new JumpingState(this, animationController);
            
            At(grounded, rising, new FuncPredicate(IsRising));
            At(grounded, sliding, new FuncPredicate(() => mover.IsGrounded() && IsGroundTooSteep()));
            At(grounded, falling, new FuncPredicate(() => !mover.IsGrounded()));
            At(grounded, jumping, new FuncPredicate(() => (jumpKeyIsPressed || jumpKeyWasPressed) && !jumpInputIsLocked));
            
            At(falling, rising, new FuncPredicate(IsRising));
            At(falling, grounded, new FuncPredicate(() => mover.IsGrounded() && !IsGroundTooSteep()));
            At(falling, sliding, new FuncPredicate(() => mover.IsGrounded() && IsGroundTooSteep()));
            
            At(sliding, rising, new FuncPredicate(IsRising));
            At(sliding, falling, new FuncPredicate(() => !mover.IsGrounded()));
            At(sliding, grounded, new FuncPredicate(() => mover.IsGrounded() && !IsGroundTooSteep()));
            
            At(rising, grounded, new FuncPredicate(() => mover.IsGrounded() && !IsGroundTooSteep()));
            At(rising, sliding, new FuncPredicate(() => mover.IsGrounded() && IsGroundTooSteep()));
            At(rising, falling, new FuncPredicate(IsFalling));
            At(rising, falling, new FuncPredicate(() => ceilingDetector != null && ceilingDetector.HitCeiling()));
            
            At(jumping, rising, new FuncPredicate(() => jumpTimer.IsFinished || jumpKeyWasLetGo));
            At(jumping, falling, new FuncPredicate(() => ceilingDetector != null && ceilingDetector.HitCeiling()));
            
            stateMachine.SetState(falling);
        }
        
        private void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        private void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

        private bool IsRising() => VectorMath.GetDotProduct(GetMomentum(), tr.up) > 0f;
        private bool IsFalling() => VectorMath.GetDotProduct(GetMomentum(), tr.up) < 0f;
        private bool IsGroundTooSteep() => Vector3.Angle(mover.GetGroundNormal(), tr.up) > slopeLimit;
        
        /// <summary>
        /// Handle Momentum Value
        /// </summary>
        private void HandleMomentum()
        {
            if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;

            Vector3 verticalMomentum = VectorMath.ExtractDotVector(momentum, tr.up);
            Vector3 horizontalMomentum = momentum - verticalMomentum;

            verticalMomentum -= tr.up * (gravity * Time.deltaTime);
            if (stateMachine.CurrentState is GroundedState && VectorMath.GetDotProduct(verticalMomentum, tr.up) < 0f) {
                verticalMomentum = Vector3.zero;
            }

            if (!IsGrounded()) {
                AdjustHorizontalMomentum(ref horizontalMomentum, CalculateMovementVelocity());
            }

            if (stateMachine.CurrentState is SlidingState)
            {
                HandleSliding(ref horizontalMomentum);
            }

            float friction = stateMachine.CurrentState is GroundedState ? groundFriction : airFriction;
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * Time.deltaTime);

            momentum = horizontalMomentum + verticalMomentum;

            if (stateMachine.CurrentState is JumpingState)
            {
                HandleJumping();
            }

            if (stateMachine.CurrentState is SlidingState)
            {
                momentum = Vector3.ProjectOnPlane(momentum, mover.GetGroundNormal());
                if (VectorMath.GetDotProduct(momentum, tr.up) > 0f) 
                    momentum = VectorMath.RemoveDotVector(momentum, tr.up);

                Vector3 slideDirection = Vector3.ProjectOnPlane(-tr.up, mover.GetGroundNormal()).normalized;
                momentum += slideDirection * (slideGravity * Time.deltaTime);
            }

            if (useLocalMomentum) momentum = tr.worldToLocalMatrix * momentum;
        }

        private void HandleJumping() {
            momentum = VectorMath.RemoveDotVector(momentum, tr.up);
            momentum += tr.up * jumpSpeed;
        }

        private void ResetJumpKeys()
        {
            jumpKeyWasLetGo = false;
            jumpKeyWasPressed = false;
        }

        public void OnJumpStart()
        {
            if (useLocalMomentum) momentum = tr.worldToLocalMatrix * momentum;

            momentum += tr.up * jumpSpeed;
            jumpTimer.Start();
            jumpInputIsLocked = true;
            OnJump.Invoke(momentum);
            
            if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;
        }
        
        public void OnGroundContactRegained() {
            Vector3 collisionVelocity = useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;
            OnLand.Invoke(collisionVelocity);
        }
        
        public void OnGroundContactLost() {
            if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;
            
            Vector3 velocity = GetMovementVelocity();
            if (velocity.sqrMagnitude >= 0f && momentum.sqrMagnitude > 0f) {
                Vector3 projectedMomentum = Vector3.Project(momentum, velocity.normalized);
                float dot = VectorMath.GetDotProduct(projectedMomentum.normalized, velocity.normalized);
                
                if (projectedMomentum.sqrMagnitude >= velocity.sqrMagnitude && dot > 0f) velocity = Vector3.zero;
                else if (dot > 0f) velocity -= projectedMomentum;
            }
            momentum += velocity;
            
            if (useLocalMomentum) momentum = tr.worldToLocalMatrix * momentum;
        }

        public void OnFallStart() {
            var currentUpMomentum = VectorMath.ExtractDotVector(momentum, tr.up);
            momentum = VectorMath.RemoveDotVector(momentum, tr.up);
            momentum -= tr.up * currentUpMomentum.magnitude;
        }

        /// <summary>
        /// Handle Slide Physics
        /// </summary>
        /// <param name="horizontalMomentum"></param>
        private void HandleSliding(ref Vector3 horizontalMomentum)
        {
            Vector3 pointDownVector = Vector3.ProjectOnPlane(mover.GetGroundNormal(), tr.up).normalized;
            Vector3 movementVelocity = CalculateMovementDirection();
            movementVelocity = VectorMath.RemoveDotVector(movementVelocity, pointDownVector);
            horizontalMomentum += movementVelocity * Time.fixedDeltaTime;
        }

        /// <summary>
        /// Calculate Horizontal Momentum
        /// </summary>
        /// <param name="horizontalMomentum">Reference of HorizontalMomentum Parameter</param>
        /// <param name="movementVelocity">Current Character Movement Velocity</param>
        private void AdjustHorizontalMomentum(ref Vector3 horizontalMomentum, Vector3 movementVelocity)
        {
            if (horizontalMomentum.magnitude > movementSpeed) {
                if (VectorMath.GetDotProduct(movementVelocity, horizontalMomentum.normalized) > 0f) {
                    movementVelocity = VectorMath.RemoveDotVector(movementVelocity, horizontalMomentum.normalized);
                }
                horizontalMomentum += movementVelocity * (Time.deltaTime * airControlRate * 0.25f);
            }
            else {
                horizontalMomentum += movementVelocity * (Time.deltaTime * airControlRate);
                horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, movementSpeed);
            }
        }

        private bool IsGrounded() => stateMachine.CurrentState is GroundedState or SlidingState;
        private Vector3 CalculateMovementVelocity() => CalculateMovementDirection() * movementSpeed;
        private Vector3 CalculateMovementDirection()
        {
            Vector3 direction = cameraTransform == null ? tr.right * input.Direction.x + tr.forward * input.Direction.y 
                : Vector3.ProjectOnPlane(cameraTransform.right, tr.up).normalized * input.Direction.x + 
                  Vector3.ProjectOnPlane(cameraTransform.forward, tr.up).normalized * input.Direction.y;

            return direction.magnitude > 1f ? direction.normalized : direction;
        }
    }
}