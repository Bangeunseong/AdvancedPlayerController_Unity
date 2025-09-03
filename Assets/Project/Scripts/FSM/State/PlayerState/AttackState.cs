using Project.Scripts.Controllers.Player;
using UnityEngine;

namespace Project.Scripts.FSM.State.PlayerState
{
    public class AttackState : BaseState
    {
        public AttackState(PlayerController player, Animator animator) : base(player, animator) { }

        public override void OnEnter()
        {
            animator.CrossFade(AttackHash, crossFadeDuration);
            player.Attack();
        }

        public override void FixedUpdate()
        {
            player.HandleMovement();
        }
    }
}