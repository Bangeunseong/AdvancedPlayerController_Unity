using System;
using UnityEngine;

namespace Project.Scripts.Controllers.Obstacles
{
    [RequireComponent(typeof(TriggerArea))]
    public class GravityWell : MonoBehaviour
    {
        TriggerArea triggerArea;

        void Start() {
            triggerArea = GetComponent<TriggerArea>();
        }

        void FixedUpdate() {
            for (var i = 0; i < triggerArea.RigidBodies.Count; i++) {
                Rigidbody rb = triggerArea.RigidBodies[i];

                Vector3 directionToRb = rb.transform.position - transform.position;
                Vector3 projection = Vector3.Project(directionToRb, transform.forward);

                Vector3 center = projection + transform.position;
                Vector3 directionToCenter = center - rb.transform.position;

                RotateRigidbody(rb, directionToCenter);
            }
        }

        void OnTriggerExit(Collider other) {
            if (other.TryGetComponent(out Rigidbody rb)) {
                RotateRigidbody(rb, Vector3.up);
                Vector3 eulerAngles = new Vector3(0, rb.rotation.eulerAngles.y, 0);
                rb.MoveRotation(Quaternion.Euler(eulerAngles));
            }
        }

        void RotateRigidbody(Rigidbody rb, Vector3 directionToCenter) {
            directionToCenter.Normalize();

            Quaternion rotationDifference = Quaternion.FromToRotation(rb.transform.up, directionToCenter);
            Quaternion finalRotation = rotationDifference * rb.transform.rotation;
            
            rb.MoveRotation(finalRotation);
        }
    }
}