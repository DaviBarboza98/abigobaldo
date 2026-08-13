using UnityEngine;

namespace Abigobaldo.Game
{
    public class VisibilityCullable : MonoBehaviour
    {
        [SerializeField] private bool neverCull;
        [SerializeField] private float extraBoundsPadding;
        [SerializeField] private float alwaysVisibleDistanceOverride = -1f;

        public bool NeverCull => neverCull;
        public float ExtraBoundsPadding => extraBoundsPadding;
        public float AlwaysVisibleDistanceOverride => alwaysVisibleDistanceOverride;

        private void OnValidate()
        {
            extraBoundsPadding = Mathf.Max(0f, extraBoundsPadding);
        }
    }
}
