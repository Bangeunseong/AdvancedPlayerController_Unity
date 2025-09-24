using System.Collections.Generic;
using AdvancedPlayer.Project.Scripts.Controllers.AdvancedPlayer.PlayerAbility;
using KBCore.Refs;
using Project.Scripts.Controllers.AdvancedPlayer.PlayerAbility;
using Project.Scripts.Controllers.Enemy;
using Project.Scripts.Entities;
using Project.Scripts.FSM;
using Project.Scripts.FSM.Predicate;
using Project.Scripts.FSM.State;
using Project.Scripts.FSM.State.EnemyState;
using UnityEngine;
using UnityEngine.AI;
using static Project.Scripts.Utils.Timer;

namespace AdvancedPlayer.Project.Scripts.Controllers.AiEnemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(PlayerDetector))]
    public class Enemy : Entity, IDamagable
    {
        [Header("References")]
        [SerializeField, Self] private NavMeshAgent agent;
        [SerializeField, Self] private PlayerDetector playerDetector;
        [SerializeField, Child] private Animator animator;
        
        [Header("Enemy Settings")]
        [SerializeField] private float wanderRadius = 10f;
        [SerializeField] private float timeBetweenAttacks = 1f;
        [SerializeField] int health = 50;

        private StateMachine stateMachine;
        private CountdownTimer attackTimer;

        readonly List<IEffect<IDamagable>> activeEffects = new();

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
        
        public void TakeDamage(int amount) {
            health -= amount;
            Debug.Log($"Enemy took {amount} damage. Health now {health}");

            if (health <= 0) Die();
        }

        public void ApplyEffect(IEffect<IDamagable> effect) {
            effect.OnCompleted += RemoveEffect;
            activeEffects.Add(effect);
            effect.Apply(this);
        }

        void RemoveEffect(IEffect<IDamagable> effect) {
            effect.OnCompleted -= RemoveEffect;
            activeEffects.Remove(effect);
        }

        void Die() {
            Debug.Log("Enemy died");

            foreach (var effect in activeEffects) {
                effect.OnCompleted -= RemoveEffect;
                effect.Cancel();
            }
            activeEffects.Clear();
            
            Destroy(gameObject);
        }
    }
}