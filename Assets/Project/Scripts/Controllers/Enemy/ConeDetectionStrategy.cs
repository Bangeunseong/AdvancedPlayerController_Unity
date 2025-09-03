using Project.Scripts.Utils;
using UnityEngine;

namespace Project.Scripts.Controllers.Enemy
{
    public class ConeDetectionStrategy : IDetectionStrategy
    {
        private readonly float detectionAngle;
        private readonly float detectionRadius;
        private readonly float innerDetectionRadius;

        public ConeDetectionStrategy(float detectionAngle, float detectionRadius, float innerDetectionRadius)
        {
            this.detectionAngle = detectionAngle;
            this.detectionRadius = detectionRadius;
            this.innerDetectionRadius = innerDetectionRadius;
        }
        
        public bool Execute(Transform player, Transform detector, Timer.CountdownTimer timer)
        {
            if (timer.IsRunning) return false;
            
            var direction = player.position - detector.position;
            var angleToPlayer = Vector3.Angle(direction, detector.forward);
            
            if ((!(angleToPlayer < detectionAngle / 2f) || !(direction.magnitude < detectionRadius)) 
                && !(direction.magnitude < innerDetectionRadius)) 
                return false;
            
            timer.Start();
            return true;
        }
    }
}