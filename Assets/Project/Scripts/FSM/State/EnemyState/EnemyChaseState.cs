using UnityEngine;
using UnityEngine.AI;

namespace Project.Scripts.FSM.State.EnemyState
{
    public class EnemyChaseState : EnemyBaseState
    {
        private readonly NavMeshAgent agent;
        private readonly Transform player;

        public EnemyChaseState(Controllers.Enemy.Enemy enemy, Animator animator, NavMeshAgent agent, Transform player) : base(enemy, animator)
        {
            this.agent = agent;
            this.player = player;
        }

        public override void OnEnter()
        {
            animator.CrossFade(RunHash, crossFadeDuration);
        }

        public override void Update()
        {
            agent.SetDestination(player.position);
        }
    }
}