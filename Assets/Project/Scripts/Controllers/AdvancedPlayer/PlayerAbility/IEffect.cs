using System;
using System.Collections.Generic;
using Project.Scripts.Controllers.AdvancedPlayer.PlayerAbility;
using UnityEngine;
using static Project.Scripts.Utils.Timer;

namespace AdvancedPlayer.Project.Scripts.Controllers.AdvancedPlayer.PlayerAbility
{
    public interface IEffect<TTarget> {
        void Apply(TTarget target);
        void Cancel();
        event Action<IEffect<TTarget>> OnCompleted;
    }

    [Serializable]
    public class DamageEffect : IEffect<IDamagable>
    {
        public int damageAmount = 10;
        
        public event Action<IEffect<IDamagable>> OnCompleted;
        
        public void Apply(IDamagable target) {
            target.TakeDamage(damageAmount);
            OnCompleted?.Invoke(this);
        }

        public void Cancel() {
            OnCompleted?.Invoke(this);
        }
    }

    [Serializable]
    public class DamageOverTimeEffect : IEffect<IDamagable>
    {
        public float duration = 5f;
        public float tickInterval = 1f;
        public int damagePerTick;

        public event Action<IEffect<IDamagable>> OnCompleted;
        
        IntervalTimer timer;
        IDamagable currentTarget;

        public void Apply(IDamagable target) {
            currentTarget = target;
            timer = new IntervalTimer(duration, tickInterval) {
                OnInterval = OnInterval,
                OnTimerStop = OnStop
            };
            timer.Start();
        }

        public void Cancel() {
            timer?.Stop();
            Cleanup();
        }
        
        void OnInterval() => currentTarget?.TakeDamage(damagePerTick);
        void OnStop() => Cleanup();
        void Cleanup() {
            timer = null;
            currentTarget = null;
            OnCompleted?.Invoke(this);
        }
    }

    [Serializable]
    public class Ability
    {
        public AudioClip castSfx;
        public GameObject castVfx;
        public GameObject runningVfx;
        
        [SerializeReference] public List<IEffect<IDamagable>> effects = new();

        public void Execute(IDamagable target) {
            foreach(var effect in effects)
                if (target is AiEnemy.Enemy enemy) {
                    enemy.ApplyEffect(effect);
                } else effect.Apply(target);
        }
    }
}