using UnityEngine;

namespace Abigobaldo.Demo
{
    [CreateAssetMenu(menuName = "Abigobaldo Demo/Recipe")]
    public class DemoRecipeData : ScriptableObject
    {
        [System.Serializable]
        public struct StateVisual
        {
            public DemoFoodState state;
            public Material material;
            public GameObject modelPrefab;
        }

        [Header("Match")]
        [SerializeField] private DemoContainerKind containerKind;
        [SerializeField] private DemoObjectKind inputKind;
        [SerializeField] private DemoObjectKind[] resumeInputKinds;

        [Header("Output")]
        [SerializeField] private DemoHoldableObject outputOnInsertPrefab;
        [SerializeField] private DemoHoldableObject outputWhenReadyPrefab;
        [SerializeField] private DemoHoldableObject charcoalPrefab;
        [SerializeField] private bool carbonizedTurnsIntoCharcoal = true;

        [Header("Visual")]
        [SerializeField] private GameObject containedVisualPrefab;
        [SerializeField] private StateVisual[] stateVisuals;

        [Header("Timing")]
        [SerializeField] private bool usesHeat = true;
        [SerializeField] private bool canBurn = true;
        [SerializeField] private bool spinsInContainer;
        [SerializeField] private float spinSpeed = 720f;
        [SerializeField] private float almostReadyTime = 5f;
        [SerializeField] private float readyTime = 10f;
        [SerializeField] private float overdoneTime = 15f;
        [SerializeField] private float burnedTime = 20f;
        [SerializeField] private float carbonizedTime = 25f;

        [Header("Side Effects")]
        [SerializeField] private DemoHoldableObject[] spawnedOnInsertPrefabs;

        [Header("Hand Mixing")]
        [SerializeField] private DemoHoldableObject handMixOutputPrefab;
        [SerializeField] private DemoFoodState handMixRequiredState = DemoFoodState.AlmostReady;
        [SerializeField] private float handMixRequiredIntensity = 80f;

        public DemoContainerKind ContainerKind => containerKind;
        public DemoObjectKind InputKind => inputKind;
        public DemoHoldableObject OutputOnInsertPrefab => outputOnInsertPrefab;
        public DemoHoldableObject OutputWhenReadyPrefab => outputWhenReadyPrefab;
        public DemoHoldableObject CharcoalPrefab => charcoalPrefab;
        public bool CarbonizedTurnsIntoCharcoal => carbonizedTurnsIntoCharcoal;
        public GameObject ContainedVisualPrefab => containedVisualPrefab;
        public bool UsesHeat => usesHeat;
        public bool CanBurn => canBurn;
        public bool SpinsInContainer => spinsInContainer;
        public float SpinSpeed => spinSpeed;
        public float AlmostReadyTime => almostReadyTime;
        public float ReadyTime => readyTime;
        public float OverdoneTime => overdoneTime;
        public float BurnedTime => burnedTime;
        public float CarbonizedTime => carbonizedTime;
        public DemoHoldableObject[] SpawnedOnInsertPrefabs => spawnedOnInsertPrefabs;
        public DemoHoldableObject HandMixOutputPrefab => handMixOutputPrefab;
        public DemoFoodState HandMixRequiredState => handMixRequiredState;
        public float HandMixRequiredIntensity => handMixRequiredIntensity;

        public bool Matches(DemoContainerKind targetContainer, DemoObjectKind targetInput)
        {
            if (containerKind != targetContainer)
                return false;

            if (inputKind == targetInput)
                return true;

            if (resumeInputKinds != null)
            {
                foreach (DemoObjectKind resumeInputKind in resumeInputKinds)
                {
                    if (resumeInputKind == targetInput)
                        return true;
                }
            }

            return false;
        }

        public bool TryGetStateVisual(DemoFoodState state, out StateVisual visual)
        {
            if (stateVisuals != null)
            {
                foreach (StateVisual entry in stateVisuals)
                {
                    if (entry.state == state)
                    {
                        visual = entry;
                        return true;
                    }
                }
            }

            visual = default;
            return false;
        }

        private void OnValidate()
        {
            spinSpeed = Mathf.Max(0f, spinSpeed);
            almostReadyTime = Mathf.Max(0f, almostReadyTime);
            readyTime = Mathf.Max(almostReadyTime, readyTime);
            overdoneTime = Mathf.Max(readyTime, overdoneTime);
            burnedTime = Mathf.Max(overdoneTime, burnedTime);
            carbonizedTime = Mathf.Max(burnedTime, carbonizedTime);
            handMixRequiredIntensity = Mathf.Max(0f, handMixRequiredIntensity);
        }
    }
}
