using Project.Scripts.Controllers.Player;
using UnityEngine;

namespace Project.Scripts.FSM.State.PlayerState
{
    public class JumpState : BaseState
    {
        public JumpState(PlayerController player, Animator animator) : base(player, animator) { }

        public override void OnEnter()
        {
            // Debug.Log("JumpState Enter");
            animator.CrossFade(JumpHash, crossFadeDuration);
        }

        public override void FixedUpdate()
        {
            player.HandleJump();
            player.HandleMovement();
        }
    }
}