using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abigobaldo.Game
{
    [AddComponentMenu("Abigobaldo/Lighting/Bloom Effect")]
    [DisallowMultipleComponent]
    public sealed class LightingBloom : LightingPostEffect
    {
        [Min(0f)]
        [SerializeField] private float intensity = 0.25f;
        [Min(0f)]
        [SerializeField] private float threshold = 1f;
        [Range(0f, 1f)]
        [SerializeField] private float size = 0.6f;
        [SerializeField] private Color tint = Color.white;
        [Tooltip("Quarter resolution is considerably cheaper and fits the low-poly look.")]
        [SerializeField] private bool quarterResolution = true;

        internal override bool RequiresHdr => true;

        internal override void Apply(VolumeProfile profile)
        {
            Bloom bloom = profile.Add<Bloom>(true);
            bloom.active = true;
            bloom.intensity.Override(intensity);
            bloom.threshold.Override(threshold);
            bloom.scatter.Override(size);
            bloom.tint.Override(tint);
            bloom.highQualityFiltering.Override(false);
            bloom.downscale.Override(quarterResolution ? BloomDownscaleMode.Quarter : BloomDownscaleMode.Half);
            bloom.maxIterations.Override(quarterResolution ? 4 : 6);
        }

        protected override void OnValidate()
        {
            intensity = Mathf.Max(0f, intensity);
            threshold = Mathf.Max(0f, threshold);
            size = Mathf.Clamp01(size);
            base.OnValidate();
        }
    }
}
