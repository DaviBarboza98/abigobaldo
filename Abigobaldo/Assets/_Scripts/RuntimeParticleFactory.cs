using UnityEngine;

public static class RuntimeParticleFactory
{
    public static ParticleSystem CreateSteam(
        Transform parent,
        string objectName,
        Color color,
        float rate
    )
    {
        return CreateSteam(
            parent,
            objectName,
            color,
            rate,
            1.1f,
            0.45f,
            0.12f,
            0.12f,
            18f,
            null
        );
    }

    public static ParticleSystem CreateSteam(
        Transform parent,
        string objectName,
        Color color,
        float rate,
        float lifetime,
        float speed,
        float size,
        float radius,
        float angle,
        Sprite sprite
    )
    {
        GameObject particleObject = new GameObject(objectName);
        particleObject.transform.SetParent(parent, false);
        particleObject.transform.localPosition = Vector3.zero;

        ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = Mathf.Max(0.01f, lifetime);
        main.startSpeed = Mathf.Max(0f, speed);
        main.startSize = Mathf.Max(0.01f, size);
        main.startColor = color;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = Mathf.Max(0f, rate);
        emission.enabled = false;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = Mathf.Max(0f, angle);
        shape.radius = Mathf.Max(0f, radius);

        ConfigureParticleRenderer(particles, sprite);

        return particles;
    }

    public static ParticleSystem CreateBlueFlame(Transform parent, string objectName)
    {
        GameObject particleObject = new GameObject(objectName);
        particleObject.transform.SetParent(parent, false);
        particleObject.transform.localPosition = Vector3.zero;

        ParticleSystem particles = particleObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particles.main;
        main.startLifetime = 0.35f;
        main.startSpeed = 0.25f;
        main.startSize = 0.16f;
        main.startColor = new Color(0.15f, 0.55f, 1f, 0.85f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.rateOverTime = 35f;
        emission.enabled = false;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = 0.18f;

        ConfigureParticleRenderer(particles, null);

        return particles;
    }

    public static Transform GetOrCreateParticlesRoot(Transform parent, Vector3 localPosition)
    {
        Transform existing = parent.Find("Particles");

        if (existing != null)
        {
            existing.localPosition = localPosition;
            return existing;
        }

        GameObject rootObject = new GameObject("Particles");
        Transform root = rootObject.transform;
        root.SetParent(parent, false);
        root.localPosition = localPosition;
        root.localRotation = Quaternion.identity;
        root.localScale = Vector3.one;

        return root;
    }

    private static void ConfigureParticleRenderer(ParticleSystem particles, Sprite sprite)
    {
        ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        Material particleMaterial = CreateParticleMaterial();

        if (particleMaterial != null)
            renderer.material = particleMaterial;

        if (sprite == null)
            return;

        ParticleSystem.TextureSheetAnimationModule textureSheet = particles.textureSheetAnimation;
        textureSheet.enabled = true;
        textureSheet.mode = ParticleSystemAnimationMode.Sprites;
        textureSheet.AddSprite(sprite);
    }

    private static Material CreateParticleMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Particles/Standard Unlit");

        if (shader == null)
            shader = Shader.Find("Legacy Shaders/Particles/Alpha Blended");

        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        if (shader == null)
            shader = Shader.Find("Unlit/Transparent");

        if (shader == null)
            shader = Shader.Find("Standard");

        return shader != null ? new Material(shader) : null;
    }
}
