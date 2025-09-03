using System;
using UnityEngine.Events;

namespace Project.Scripts.Animation
{
    [Serializable] public class AnimationEvent
    {
        public string eventName;
        public UnityEvent OnAnimationEvent;
    }
}