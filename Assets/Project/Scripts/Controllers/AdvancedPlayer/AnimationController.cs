using System;
using KBCore.Refs;
using UnityEngine;

namespace Project.Scripts.Controllers.AdvancedPlayer
{
    public class AnimationController : MonoBehaviour
    {
        public enum AnimationState{Locomotion, Jump, Land}
        
        private static readonly int LocomotionHash = Animator.StringToHash("Locomotion");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int LandHash = Animator.StringToHash("Land");
        private static readonly int Speed =  Animator.StringToHash("Speed");

        [SerializeField] private PlayerController controller;
        [SerializeField] private Animator animator;
        [SerializeField] private float crossFadeDuration = 0.1f;

        private void Update()
        {
            animator.SetFloat(Speed, controller.GetMovementVelocity().magnitude);
        }

        public void CrossFade(AnimationState to)
        {
            animator.CrossFade(to switch
            {
                AnimationState.Locomotion => LocomotionHash,
                AnimationState.Jump => JumpHash,
                AnimationState.Land => LandHash,
                _ => throw new System.NotImplementedException(),
            }, crossFadeDuration);
        }
    }
}