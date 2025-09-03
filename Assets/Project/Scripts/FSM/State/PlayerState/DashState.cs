using Project.Scripts.Controllers.Player;
using UnityEngine;

namespace Project.Scripts.FSM.State.PlayerState
{
    public class DashState : BaseState
    {
        public DashState(PlayerController player, Animator animator) : base(player, animator) { }

        public override void OnEnter()
        {
            // Debug.Log("DashState Enter");
            animator.CrossFade(DashHash, crossFadeDuration);
        }

        public override void FixedUpdate()
        {
            player.HandleMovement();
        }
    }
}