using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Animation
{
    public class AnimationEventReceiver : MonoBehaviour
    {
        [SerializeField] private List<AnimationEvent> animationEvents = new();

        public void OnAnimationEventTriggered(string eventName)
        {
            AnimationEvent matchingEvent = animationEvents.Find(se => se.eventName == eventName);
            matchingEvent?.OnAnimationEvent?.Invoke();
        }
    }
}