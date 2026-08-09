using UnityEngine;

public class ParticleEmitterController : MonoBehaviour
{
    [SerializeField] private ParticleSystem target;

    private ParticleSystem Target
    {
        get
        {
            if (target == null)
                target = GetComponentInChildren<ParticleSystem>();

            return target;
        }
    }

    public void Play()
    {
        if (Target == null)
            return;

        ParticleSystem.EmissionModule emission = Target.emission;
        emission.enabled = true;
        Target.Play();
    }

    public void Stop()
    {
        if (Target == null)
            return;

        ParticleSystem.EmissionModule emission = Target.emission;
        emission.enabled = false;
        Target.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }

    public void SetColor(Color color)
    {
        if (Target == null)
            return;

        ParticleSystem.MainModule main = Target.main;
        main.startColor = color;
    }

    public void SetRate(float rate)
    {
        if (Target == null)
            return;

        ParticleSystem.EmissionModule emission = Target.emission;
        emission.rateOverTime = Mathf.Max(0f, rate);
    }
}
