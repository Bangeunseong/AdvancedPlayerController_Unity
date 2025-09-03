using System;
using UnityEngine;

namespace Project.Scripts.Controllers
{
    public class GroundChecker : MonoBehaviour
    {
        [SerializeField] private float groundDistance = 0.08f;
        [SerializeField] private LayerMask groundLayers;
        
        public bool IsGrounded { get; private set; }

        private void Update()
        {
            IsGrounded = Physics.CheckSphere(transform.position, groundDistance, groundLayers, QueryTriggerInteraction.Ignore);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, groundDistance);
        }
    }
}