using UnityEngine;

namespace Project.Scripts.Animation
{
    public class AnimationEventStateBehavior : StateMachineBehaviour
    {
        public string eventName;
        [Range(0f, 1f)] public float triggerTime;

        private bool hasTriggered;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            hasTriggered = false;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo,
            int layerIndex)
        {
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo,
            int layerIndex)
        {
            float currentTime = stateInfo.normalizedTime % 1f;

            if (!hasTriggered && currentTime >= triggerTime)
            {
                NotifyReceiver(animator);
                hasTriggered = true;
            }
        }

        public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo,
            int layerIndex)
        {
        }

        public override void OnStateIK(Animator animator, AnimatorStateInfo stateInfo,
            int layerIndex)
        {
        }
        
        private void NotifyReceiver(Animator animator)
        {
            AnimationEventReceiver receiver = animator.GetComponent<AnimationEventReceiver>();
            if (receiver != null) {
                receiver.OnAnimationEventTriggered(eventName);
            }
        }
    }
}