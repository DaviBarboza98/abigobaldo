using UnityEngine;

namespace Abigobaldo.Demo
{
    public class DemoStationParticles : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] particleSystems;
        [SerializeField] private Color rawColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private Color almostReadyColor = new Color(1f, 1f, 1f, 0.45f);
        [SerializeField] private Color readyColor = new Color(1f, 0.95f, 0.75f, 0.55f);
        [SerializeField] private Color overdoneColor = new Color(0.55f, 0.48f, 0.42f, 0.65f);
        [SerializeField] private Color burnedColor = new Color(0.25f, 0.22f, 0.2f, 0.8f);
        [SerializeField] private Color carbonizedColor = new Color(0.05f, 0.05f, 0.05f, 0.9f);

        private bool playing;

        public static DemoStationParticles CreateDefault(Transform parent, string objectName)
        {
            GameObject particleObject = new GameObject(objectName);
            particleObject.transform.SetParent(parent);
            particleObject.transform.SetLocalPositionAndRotation(Vector3.up * 0.12f, Quaternion.identity);

            ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = particleSystem.main;
            main.loop = true;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.25f, 0.75f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.075f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            ParticleSystem.EmissionModule emission = particleSystem.emission;
            emission.rateOverTime = 18f;

            ParticleSystem.ShapeModule shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 18f;
            shape.radius = 0.08f;

            ParticleSystem.VelocityOverLifetimeModule velocity = particleSystem.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.y = new ParticleSystem.MinMaxCurve(0.25f, 0.65f);

            ParticleSystem.NoiseModule noise = particleSystem.noise;
            noise.enabled = true;
            noise.strength = 0.25f;
            noise.frequency = 0.8f;

            DemoStationParticles controller = particleObject.AddComponent<DemoStationParticles>();
            controller.particleSystems = new[] { particleSystem };
            controller.SetState(false, DemoFoodState.Raw);
            return controller;
        }

        private void Awake()
        {
            CacheParticles();
            SetState(false, DemoFoodState.Raw);
        }

        public void SetState(bool shouldPlay, DemoFoodState state)
        {
            CacheParticles();
            ApplyColor(GetColor(state));

            if (playing == shouldPlay)
                return;

            playing = shouldPlay;

            foreach (ParticleSystem particleSystem in particleSystems)
            {
                if (particleSystem == null)
                    continue;

                if (playing)
                    particleSystem.Play(true);
                else
                    particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }

        private void ApplyColor(Color color)
        {
            foreach (ParticleSystem particleSystem in particleSystems)
            {
                if (particleSystem == null)
                    continue;

                ParticleSystem.MainModule main = particleSystem.main;
                main.startColor = color;
            }
        }

        private Color GetColor(DemoFoodState state)
        {
            return state switch
            {
                DemoFoodState.AlmostReady => almostReadyColor,
                DemoFoodState.Ready => readyColor,
                DemoFoodState.Overdone => overdoneColor,
                DemoFoodState.Burned => burnedColor,
                DemoFoodState.Carbonized => carbonizedColor,
                _ => rawColor
            };
        }

        private void CacheParticles()
        {
            if (particleSystems != null && particleSystems.Length > 0)
                return;

            particleSystems = GetComponentsInChildren<ParticleSystem>(true);
        }
    }
}
