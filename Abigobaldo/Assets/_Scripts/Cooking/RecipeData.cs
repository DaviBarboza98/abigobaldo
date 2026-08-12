using UnityEngine;

namespace Abigobaldo.Game
{
    [CreateAssetMenu(menuName = "Abigobaldo/Recipe")]
    public class RecipeData : ScriptableObject
    {
        [System.Serializable]
        public struct StateVisual
        {
            public FoodState state;
            public Material material;
            public GameObject modelPrefab;
        }

        [Header("Match")]
        [SerializeField] private ContainerKind containerKind;
        [SerializeField] private ObjectKind inputKind;
        [SerializeField] private ObjectKind[] resumeInputKinds;

        [Header("Output")]
        [SerializeField] private HoldableObject outputOnInsertPrefab;
        [SerializeField] private HoldableObject outputWhenReadyPrefab;
        [SerializeField] private HoldableObject charcoalPrefab;
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
        [SerializeField] private HoldableObject[] spawnedOnInsertPrefabs;

        [Header("Hand Mixing")]
        [SerializeField] private HoldableObject handMixOutputPrefab;
        [SerializeField] private FoodState handMixRequiredState = FoodState.AlmostReady;
        [SerializeField] private float handMixRequiredIntensity = 80f;

        public ContainerKind ContainerKind => containerKind;
        public ObjectKind InputKind => inputKind;
        public HoldableObject OutputOnInsertPrefab => outputOnInsertPrefab;
        public HoldableObject OutputWhenReadyPrefab => outputWhenReadyPrefab;
        public HoldableObject CharcoalPrefab => charcoalPrefab;
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
        public HoldableObject[] SpawnedOnInsertPrefabs => spawnedOnInsertPrefabs;
        public HoldableObject HandMixOutputPrefab => handMixOutputPrefab;
        public FoodState HandMixRequiredState => handMixRequiredState;
        public float HandMixRequiredIntensity => handMixRequiredIntensity;

        public bool Matches(ContainerKind targetContainer, ObjectKind targetInput)
        {
            if (containerKind != targetContainer)
                return false;

            if (inputKind == targetInput)
                return true;

            if (resumeInputKinds != null)
            {
                foreach (ObjectKind resumeInputKind in resumeInputKinds)
                {
                    if (resumeInputKind == targetInput)
                        return true;
                }
            }

            return false;
        }

        public bool TryGetStateVisual(FoodState state, out StateVisual visual)
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
