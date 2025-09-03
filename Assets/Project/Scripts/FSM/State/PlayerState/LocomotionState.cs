using Project.Scripts.Controllers.Player;
using UnityEngine;

namespace Project.Scripts.FSM.State.PlayerState
{
    public class LocomotionState : BaseState
    {
        public LocomotionState(PlayerController player, Animator animator) : base(player, animator) { }

        public override void OnEnter()
        {
            // Debug.Log("LocomotionState Enter");
            animator.CrossFade(LocomotionHash, crossFadeDuration);
        }

        public override void FixedUpdate()
        {
            player.HandleMovement();
        }
    }
}