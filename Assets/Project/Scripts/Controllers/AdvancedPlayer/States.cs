using Project.Scripts.FSM.State;

namespace Project.Scripts.Controllers.AdvancedPlayer
{
    public class GroundedState : IState {
        readonly PlayerController controller;
        readonly AnimationController animationController;

        public GroundedState(PlayerController controller, AnimationController animationController) {
            this.controller = controller;
            this.animationController = animationController;
        }

        public void OnEnter() {
            animationController.CrossFade(AnimationController.AnimationState.Locomotion);
            controller.OnGroundContactRegained();
        }
    }

    public class FallingState : IState {
        readonly PlayerController controller;
        readonly AnimationController animationController;
        
        public FallingState(PlayerController controller, AnimationController animationController) {
            this.controller = controller;
            this.animationController = animationController;
        }

        public void OnEnter() {
            animationController.CrossFade(AnimationController.AnimationState.Jump);
            controller.OnFallStart();
        }
    }

    public class SlidingState : IState {
        readonly PlayerController controller;
        readonly AnimationController animationController;

        public SlidingState(PlayerController controller, AnimationController animationController) {
            this.controller = controller;
            this.animationController = animationController;
        }

        public void OnEnter() {
            controller.OnGroundContactLost();
        }
    }

    public class RisingState : IState {
        readonly PlayerController controller;
        readonly AnimationController animationController;

        public RisingState(PlayerController controller, AnimationController animationController) {
            this.controller = controller;
            this.animationController = animationController;
        }

        public void OnEnter() {
            animationController.CrossFade(AnimationController.AnimationState.Jump);
            controller.OnGroundContactLost();
        }
    }

    public class JumpingState : IState {
        readonly PlayerController controller;
        readonly AnimationController animationController;

        public JumpingState(PlayerController controller, AnimationController animationController) {
            this.controller = controller;
            this.animationController = animationController;
        }

        public void OnEnter() {
            animationController.CrossFade(AnimationController.AnimationState.Jump);
            controller.OnGroundContactLost();
            controller.OnJumpStart();
        }
    }
}