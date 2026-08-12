using UnityEngine;

namespace Abigobaldo.Game
{
    public class StationParticles : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] particleSystems;
        [SerializeField] private Color rawColor = new Color(1f, 1f, 1f, 0.35f);
        [SerializeField] private Color almostReadyColor = new Color(1f, 1f, 1f, 0.45f);
        [SerializeField] private Color readyColor = new Color(1f, 0.95f, 0.75f, 0.55f);
        [SerializeField] private Color overdoneColor = new Color(0.55f, 0.48f, 0.42f, 0.65f);
        [SerializeField] private Color burnedColor = new Color(0.25f, 0.22f, 0.2f, 0.8f);
        [SerializeField] private Color carbonizedColor = new Color(0.05f, 0.05f, 0.05f, 0.9f);

        private bool playing;

        public static StationParticles CreateDefault(Transform parent, string objectName)
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

            StationParticles controller = particleObject.AddComponent<StationParticles>();
            controller.particleSystems = new[] { particleSystem };
            controller.SetState(false, FoodState.Raw);
            return controller;
        }

        private void Awake()
        {
            CacheParticles();
            SetState(false, FoodState.Raw);
        }

        public void SetState(bool shouldPlay, FoodState state)
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

        private Color GetColor(FoodState state)
        {
            return state switch
            {
                FoodState.AlmostReady => almostReadyColor,
                FoodState.Ready => readyColor,
                FoodState.Overdone => overdoneColor,
                FoodState.Burned => burnedColor,
                FoodState.Carbonized => carbonizedColor,
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
