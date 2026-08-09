using UnityEngine;

public class ParticleEmitterController : MonoBehaviour
{
    public enum ParticlePreset
    {
        Custom,
        SoftSteam,
        HeavySteam,
        DarkBurnSmoke,
        BlueCooktopFlame
    }

    [SerializeField] private ParticleSystem target;
    [SerializeField] private ParticlePreset preset = ParticlePreset.SoftSteam;
    [SerializeField] private bool applyPresetOnAwake;

    private ParticleSystem Target
    {
        get
        {
            if (target == null)
                target = GetComponentInChildren<ParticleSystem>();

            return target;
        }
    }

    private void Awake()
    {
        if (applyPresetOnAwake)
            ApplyPreset();
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

    [ContextMenu("Apply Preset")]
    public void ApplyPreset()
    {
        if (Target == null || preset == ParticlePreset.Custom)
            return;

        switch (preset)
        {
            case ParticlePreset.SoftSteam:
                ConfigureSteam(new Color(0.86f, 0.86f, 0.82f, 0.38f), 8f, 1.8f, 0.28f, 0.75f);
                break;
            case ParticlePreset.HeavySteam:
                ConfigureSteam(new Color(0.9f, 0.9f, 0.86f, 0.52f), 18f, 2.3f, 0.38f, 1.05f);
                break;
            case ParticlePreset.DarkBurnSmoke:
                ConfigureSteam(new Color(0.17f, 0.15f, 0.13f, 0.62f), 14f, 2.8f, 0.42f, 0.65f);
                break;
            case ParticlePreset.BlueCooktopFlame:
                ConfigureBlueFlame();
                break;
        }
    }

    [ContextMenu("Apply Best Preset From Hierarchy")]
    public void ApplyBestPresetFromHierarchy()
    {
        string hierarchyName = GetHierarchyName().ToLowerInvariant();

        if (hierarchyName.Contains("cooktop") || hierarchyName.Contains("stove") || hierarchyName.Contains("fire") || hierarchyName.Contains("flame"))
            preset = ParticlePreset.BlueCooktopFlame;
        else if (hierarchyName.Contains("burn") || hierarchyName.Contains("smoke"))
            preset = ParticlePreset.DarkBurnSmoke;
        else if (hierarchyName.Contains("cuscuz") || hierarchyName.Contains("steam"))
            preset = ParticlePreset.HeavySteam;
        else
            preset = ParticlePreset.SoftSteam;

        ApplyPreset();
    }

    private void ConfigureSteam(Color color, float rate, float lifetime, float startSize, float speed)
    {
        ParticleSystem.MainModule main = Target.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = lifetime;
        main.startSpeed = speed;
        main.startSize = startSize;
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = Target.emission;
        emission.enabled = false;
        emission.rateOverTime = rate;

        ParticleSystem.ShapeModule shape = Target.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 18f;
        shape.radius = 0.08f;
        shape.position = Vector3.zero;

        ParticleSystem.VelocityOverLifetimeModule velocity = Target.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.y = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);

        ParticleSystem.SizeOverLifetimeModule size = Target.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.25f, 1f, 1.4f));

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = Target.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = BuildFadeGradient(color);

        ParticleSystem.NoiseModule noise = Target.noise;
        noise.enabled = true;
        noise.strength = 0.22f;
        noise.frequency = 0.55f;
        noise.scrollSpeed = 0.25f;

        ParticleSystemRenderer particleRenderer = Target.GetComponent<ParticleSystemRenderer>();
        if (particleRenderer != null)
        {
            particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            particleRenderer.sortingFudge = 0.2f;
        }
    }

    private void ConfigureBlueFlame()
    {
        ParticleSystem.MainModule main = Target.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = 0.28f;
        main.startSpeed = 0.45f;
        main.startSize = 0.16f;
        main.startColor = new Color(0.25f, 0.65f, 1f, 0.75f);
        main.simulationSpace = ParticleSystemSimulationSpace.Local;

        ParticleSystem.EmissionModule emission = Target.emission;
        emission.enabled = false;
        emission.rateOverTime = 36f;

        ParticleSystem.ShapeModule shape = Target.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 10f;
        shape.radius = 0.16f;

        ParticleSystem.SizeOverLifetimeModule size = Target.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.45f, 1f, 0f));

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = Target.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = BuildFadeGradient(new Color(0.2f, 0.7f, 1f, 0.8f));

        ParticleSystem.NoiseModule noise = Target.noise;
        noise.enabled = true;
        noise.strength = 0.06f;
        noise.frequency = 1.8f;
    }

    private static ParticleSystem.MinMaxGradient BuildFadeGradient(Color color)
    {
        Gradient gradient = new Gradient();
        Color transparent = new Color(color.r, color.g, color.b, 0f);

        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 0.65f),
                new GradientColorKey(transparent, 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(color.a, 0.2f),
                new GradientAlphaKey(0f, 1f)
            }
        );

        return new ParticleSystem.MinMaxGradient(gradient);
    }

    private string GetHierarchyName()
    {
        Transform current = transform;
        string fullName = string.Empty;

        while (current != null)
        {
            fullName = string.IsNullOrEmpty(fullName)
                ? current.name
                : $"{current.name}/{fullName}";
            current = current.parent;
        }

        return fullName;
    }
}


