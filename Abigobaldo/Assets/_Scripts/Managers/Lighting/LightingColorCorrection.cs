using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Abigobaldo.Game
{
    [AddComponentMenu("Abigobaldo/Lighting/Color Correction Effect")]
    [DisallowMultipleComponent]
    public sealed class LightingColorCorrection : LightingPostEffect
    {
        [Range(-5f, 5f)]
        [SerializeField] private float brightness;
        [Range(-100f, 100f)]
        [SerializeField] private float contrast = 8f;
        [Range(-100f, 100f)]
        [SerializeField] private float saturation = 8f;
        [Range(-180f, 180f)]
        [SerializeField] private float hueShift;
        [SerializeField] private Color tintColor = Color.white;

        internal override void Apply(VolumeProfile profile)
        {
            ColorAdjustments colorAdjustments = profile.Add<ColorAdjustments>(true);
            colorAdjustments.active = true;
            colorAdjustments.postExposure.Override(brightness);
            colorAdjustments.contrast.Override(contrast);
            colorAdjustments.saturation.Override(saturation);
            colorAdjustments.hueShift.Override(hueShift);
            colorAdjustments.colorFilter.Override(tintColor);
        }

        protected override void OnValidate()
        {
            brightness = Mathf.Clamp(brightness, -5f, 5f);
            contrast = Mathf.Clamp(contrast, -100f, 100f);
            saturation = Mathf.Clamp(saturation, -100f, 100f);
            hueShift = Mathf.Clamp(hueShift, -180f, 180f);
            base.OnValidate();
        }
    }
}
