using UnityEngine;

public class CookingProcess
{
    public float Timer { get; private set; }
    public float CookingTime { get; private set; }

    public bool IsRunning { get; private set; }
    public bool IsReady => Timer >= CookingTime;
    public float Progress => CookingTime <= 0f ? 1f : Mathf.Clamp01(Timer / CookingTime);
    public float OvercookTime => IsReady ? Timer - CookingTime : 0f;

    public CookingProcess(float cookingTime)
    {
        CookingTime = Mathf.Max(0f, cookingTime);
        Reset();
    }

    public void Start()
    {
        Reset();
        IsRunning = true;
    }

    public void Resume()
    {
        IsRunning = true;
    }

    public void Update(float deltaTime)
    {
        if (!IsRunning)
            return;

        Timer += Mathf.Max(0f, deltaTime);
    }

    public void Stop()
    {
        IsRunning = false;
    }

    public void Pause()
    {
        IsRunning = false;
    }

    public void Reset()
    {
        Timer = 0f;
        IsRunning = false;
    }
}

