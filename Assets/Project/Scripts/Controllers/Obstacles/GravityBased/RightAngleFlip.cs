using System;
using UnityEngine;

namespace Project.Scripts.Controllers.Obstacles
{
    public class RightAngleFlip : MonoBehaviour
    {
        void OnTriggerEnter(Collider other) {
            if (other.attachedRigidbody != null) {
                FlipDirection(transform.forward, other.attachedRigidbody.transform);
            }
        }

        void FlipDirection(Vector3 newUpDirection, Transform tr) {
            float angleBetweenUpDirections = Vector3.Angle(newUpDirection, tr.up);
            float angleThreshold = 0.001f;

            if (angleThreshold > angleBetweenUpDirections) return;
            
            Quaternion rotationDifference = Quaternion.FromToRotation(tr.up, newUpDirection);
            tr.rotation = rotationDifference * tr.rotation;
        }
    }
}
