using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abigobaldo.Game
{
    [AddComponentMenu("Abigobaldo/Lighting/Depth Of Field Effect")]
    [DisallowMultipleComponent]
    public sealed class LightingDepthOfField : LightingPostEffect
    {
        [Tooltip("Distance where the image begins to leave the sharp focus range.")]
        [Min(0f)]
        [SerializeField] private float focusDistance = 12f;
        [Tooltip("Distance after Focus Distance where far blur reaches full strength.")]
        [Min(0.01f)]
        [SerializeField] private float focusRange = 18f;
        [Range(0.5f, 1.5f)]
        [SerializeField] private float farIntensity = 0.75f;
        [Tooltip("More stable but more expensive. Leave disabled for gameplay.")]
        [SerializeField] private bool highQuality;

        internal override void Apply(VolumeProfile profile)
        {
            DepthOfField depthOfField = profile.Add<DepthOfField>(true);
            depthOfField.active = true;
            depthOfField.mode.Override(DepthOfFieldMode.Gaussian);
            depthOfField.gaussianStart.Override(focusDistance);
            depthOfField.gaussianEnd.Override(focusDistance + focusRange);
            depthOfField.gaussianMaxRadius.Override(farIntensity);
            depthOfField.highQualitySampling.Override(highQuality);
        }

        protected override void OnValidate()
        {
            focusDistance = Mathf.Max(0f, focusDistance);
            focusRange = Mathf.Max(0.01f, focusRange);
            farIntensity = Mathf.Clamp(farIntensity, 0.5f, 1.5f);
            base.OnValidate();
        }
    }
}
