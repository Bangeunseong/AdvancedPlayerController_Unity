using System;
using Project.Scripts.Controllers.Player;
using UnityEngine;
using static Project.Scripts.Utils.Timer;

namespace Project.Scripts.Controllers.Enemy
{
    public class PlayerDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float detectionAngle = 60f;
        [SerializeField] private float detectionRadius = 10f;
        [SerializeField] private float innerDetectionRadius = 5f;
        [SerializeField] private float detectionCooldown = 1f;
        [SerializeField] private float attackRange = 2f;
        
        public Transform Player { get; private set; }
        public Health PlayerHealth { get; private set; }

        private CountdownTimer detectionTimer;
        private IDetectionStrategy detectionStrategy;

        private void Awake()
        {
            Player = GameObject.FindGameObjectWithTag("Player").transform;
            PlayerHealth = Player.GetComponent<Health>();
        }
        
        private void Start()
        {
            detectionTimer = new CountdownTimer(detectionCooldown);
            detectionStrategy = new ConeDetectionStrategy(detectionAngle, detectionRadius, innerDetectionRadius);
        }

        private void Update() => detectionTimer.Tick(Time.deltaTime);

        public bool CanDetectPlayer()
        {
            return detectionTimer.IsRunning || detectionStrategy.Execute(Player, transform, detectionTimer);
        }

        public bool CanAttackPlayer()
        {
            var directionToPlayer = Player.position - transform.position;
            return directionToPlayer.magnitude <= attackRange;
        }

        public void SetDetectionStrategy(IDetectionStrategy strategy) => detectionStrategy = strategy;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            Gizmos.DrawWireSphere(transform.position, innerDetectionRadius);

            Vector3 forwardCone = Quaternion.Euler(0, detectionAngle / 2f, 0) * transform.forward * detectionRadius;
            Vector3 backwardCone = Quaternion.Euler(0,  -detectionAngle / 2f, 0) * transform.forward * detectionRadius;
            
            Gizmos.DrawLine(transform.position, transform.position + forwardCone);
            Gizmos.DrawLine(transform.position, transform.position + backwardCone);
        }
    }
}