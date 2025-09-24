using AdvancedPlayer.Project.Scripts.Controllers.AiEnemy;
using UnityEngine;
using UnityEngine.AI;

namespace Project.Scripts.FSM.State.EnemyState
{
    public class EnemyWanderState : EnemyBaseState
    {
        private readonly NavMeshAgent agent;
        private readonly Vector3 startPoint;
        private float wanderRadius;
        
        public EnemyWanderState(Enemy enemy, Animator animator, NavMeshAgent agent, float wanderRadius) : base(enemy, animator)
        {
            this.agent = agent;
            this.startPoint = enemy.transform.position;
            this.wanderRadius = wanderRadius;
        }

        public override void OnEnter()
        {
            animator.CrossFade(WalkHash, crossFadeDuration);
        }

        public override void Update()
        {
            if (!HasReachedDestination()) return;
            
            // find new destination
            var randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += startPoint;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, 1);
            var finalPosition = hit.position;

            agent.SetDestination(finalPosition);
        }

        private bool HasReachedDestination()
        {
            return !agent.pathPending 
                   && agent.remainingDistance <= agent.stoppingDistance 
                   && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
        }
    }
}