using System;

namespace Project.Scripts.Utils
{
    public abstract class Timer
    {
        protected float initialTime;
        protected float Time { get; set; }
        public bool IsRunning { get; protected set; }
        public abstract bool IsFinished { get; }
        public float Progress => Time / initialTime;
    
        public Action OnTimerStart = delegate { };
        public Action OnTimerStop = delegate { };

        protected Timer(float value)
        {
            initialTime = value;
            IsRunning = false;
        }

        public void Start()
        {
            Time = initialTime;
            if (!IsRunning)
            {
                IsRunning = true;
                OnTimerStart.Invoke();
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                OnTimerStop.Invoke();
            }
        }

        public void Resume() => IsRunning = true;
        public void Pause() => IsRunning = false;

        public abstract void Tick(float deltaTime);

        // Countdown Timer
        public class CountdownTimer : Timer
        {
            public CountdownTimer(float value) : base(value) { }

            public override void Tick(float deltaTime)
            {
                if (IsRunning && Time > 0) Time -= deltaTime;
                if (IsRunning && Time <= 0) Stop();
            }
            
            public override bool IsFinished => Time <= 0;
        
            public void Reset()
            {
                Time = initialTime;
            }

            public void Reset(float newTime)
            {
                initialTime = newTime; 
                Reset();
            }
        }

        // Interval Timer
        public class IntervalTimer : Timer
        {
            readonly float interval;
            float nextInterval;
            
            public Action OnInterval = delegate { };

            public IntervalTimer(float totalTime, float intervalSecond) : base(totalTime) {
                interval = intervalSecond;
                nextInterval = totalTime - interval;
            }

            public override void Tick(float deltaTime) {
                if (IsRunning && Time > 0) {
                    Time -= deltaTime;

                    while (Time <= nextInterval && nextInterval >= 0) {
                        OnInterval.Invoke();
                        nextInterval -= interval;
                    }
                }

                if (IsRunning && Time <= 0) {
                    Time = 0;
                    Stop();
                }
            }
            
            public override bool IsFinished => Time <= 0;
        }
        
        // Stopwatch Timer
        public class StopwatchTimer : Timer
        {
            public StopwatchTimer() : base(0) { }

            public override void Tick(float deltaTime)
            {
                if (IsRunning) Time += deltaTime;
            }
            
            public override bool IsFinished => false;

            public void Reset() => Time = 0;
            public float GetTime() => Time;
        }
    }
}
