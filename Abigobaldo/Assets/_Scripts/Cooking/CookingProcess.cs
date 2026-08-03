using UnityEngine;

public class CookingProcess
{
    public float Timer { get; private set; }

    public float CookingTime { get; private set; }

    public float BurningTime { get; private set; }

    public bool IsCooking { get; private set; }
    public bool IsReady { get; private set; }
    public bool IsBurned { get; private set; }

    public CookingProcess(
        float cookingTime,
        float burningTime
    )
    {
        CookingTime = cookingTime;
        BurningTime = burningTime;

        Reset();
    }

    public void Start()
    {
        Reset();

        IsCooking = true;
    }

    public void Update(float deltaTime)
    {
        if (!IsCooking)
            return;

        Timer += deltaTime;

        if (!IsReady && Timer >= CookingTime)
        {
            IsReady = true;
        }

        if (
            IsReady &&
            BurningTime > 0f &&
            Timer >= CookingTime + BurningTime
        )
        {
            IsBurned = true;
            IsCooking = false;
        }
    }

    public void Reset()
    {
        Timer = 0f;

        IsCooking = false;
        IsReady = false;
        IsBurned = false;
    }
}