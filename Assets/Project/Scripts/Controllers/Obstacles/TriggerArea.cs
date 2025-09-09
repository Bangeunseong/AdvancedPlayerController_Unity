using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Controllers.Obstacles
{
    public class TriggerArea : MonoBehaviour
    {
        readonly List<Rigidbody> rigidBodies = new();
        public IReadOnlyList<Rigidbody> RigidBodies => rigidBodies;

        void OnTriggerEnter(Collider other) {
            if (other.attachedRigidbody != null) {
                rigidBodies.Add(other.attachedRigidbody);
            }
        }

        void OnTriggerExit(Collider other) {
            if (other.attachedRigidbody != null) {
                rigidBodies.Remove(other.attachedRigidbody);
            }
        }
    }
}
