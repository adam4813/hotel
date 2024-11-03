using System;

public class CountdownTimer
{
    public event Action OnTimerStop = delegate { };
    public event Action OnTimerStart = delegate { };

    private float initialTime;
    public float Time { get; set; }
    public bool IsRunning { get; protected set; }
    public bool IsFinished => Time <= 0;

    public void Resume() => IsRunning = true;
    public void Pause() => IsRunning = false;

    public CountdownTimer(float timerInterval)
    {
        initialTime = timerInterval;
        IsRunning = false;
    }

    public void Start()
    {
        Time = initialTime;
        if (IsRunning) return;
        
        IsRunning = true;
        OnTimerStart.Invoke();
    }

    public void Stop()
    {
        if (!IsRunning) return;
        
        IsRunning = false;
        OnTimerStop.Invoke();
    }

    public void Tick(float deltaTime)
    {
        if (IsRunning && Time > 0)
        {
            Time -= deltaTime;
        }

        if (IsRunning && Time <= 0)
        {
            Stop();
        }
    }

    public void Reset() => Time = initialTime;

    public void Reset(float newTime) {
        initialTime = newTime;
        Reset();
    }
}