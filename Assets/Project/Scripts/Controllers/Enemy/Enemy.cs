using KBCore.Refs;
using Project.Scripts.Entities;
using Project.Scripts.FSM;
using Project.Scripts.FSM.Predicate;
using Project.Scripts.FSM.State;
using Project.Scripts.FSM.State.EnemyState;
using UnityEngine;
using UnityEngine.AI;
using static Project.Scripts.Utils.Timer;

namespace Project.Scripts.Controllers.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(PlayerDetector))]
    public class Enemy : Entity
    {
        [Header("References")]
        [SerializeField, Self] private NavMeshAgent agent;
        [SerializeField, Self] private PlayerDetector playerDetector;
        [SerializeField, Child] private Animator animator;
        
        [Header("Enemy Settings")]
        [SerializeField] private float wanderRadius = 10f;
        [SerializeField] private float timeBetweenAttacks = 1f;

        private StateMachine stateMachine;
        private CountdownTimer attackTimer;

        private void OnValidate() => this.ValidateRefs();

        private void Start()
        {
            attackTimer = new CountdownTimer(timeBetweenAttacks);
            
            stateMachine = new StateMachine();

            var wanderState = new EnemyWanderState(this, animator, agent, wanderRadius);
            var chaseState = new EnemyChaseState(this, animator, agent, playerDetector.Player);
            var attackState = new EnemyAttackState(this, animator, agent, playerDetector.Player);
            
            At(wanderState, chaseState, new FuncPredicate(() => playerDetector.CanDetectPlayer()));
            At(chaseState, wanderState, new FuncPredicate(() => !playerDetector.CanDetectPlayer()));
            At(chaseState, attackState, new FuncPredicate(() => playerDetector.CanAttackPlayer()));
            At(attackState, chaseState, new FuncPredicate(() => !playerDetector.CanAttackPlayer()));
            
            stateMachine.SetState(wanderState);
        }

        private void At(IState from, IState to, IPredicate condition) => stateMachine.AddTransition(from, to, condition);
        private void Any(IState to, IPredicate condition) => stateMachine.AddAnyTransition(to, condition);

        private void Update()
        {
            stateMachine.Update();
            attackTimer.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            stateMachine.FixedUpdate();
        }

        public void Attack()
        {
            if (attackTimer.IsRunning) return;
            
            attackTimer.Start();
            playerDetector.PlayerHealth.TakeDamage(10);
        }
    }
}