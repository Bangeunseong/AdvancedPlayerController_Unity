using AdvancedPlayer.Project.Scripts.Controllers.AiEnemy;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Scripts.FSM.State.EnemyState
{
    public class EnemyAttackState : EnemyBaseState
    {
        private readonly NavMeshAgent agent;
        private readonly Transform player;
        
        public EnemyAttackState(Enemy enemy, Animator animator, NavMeshAgent agent, Transform player) : base(enemy, animator)
        {
            this.agent = agent;
            this.player = player;
        }

        public override void OnEnter()
        {
            animator.CrossFade(AttackHash, crossFadeDuration);
        }

        public override void Update()
        {
            agent.SetDestination(player.position);
            enemy.Attack();
        }
    }
}