using UnityEngine;

namespace Abigobaldo.Game
{
    [AddComponentMenu("Abigobaldo/Lighting/Surface Light")]
    [DisallowMultipleComponent]
    public sealed class LightingSurfaceLight : LightingLocalLight
    {
        [Tooltip("The local forward direction is the emitting face.")]
        [SerializeField] private Vector2 size = new Vector2(1f, 1f);

        // WebGL does not support realtime Area Lights. A point-light fallback
        // keeps the scene illuminated instead of preventing the player build.
#if UNITY_WEBGL
        protected override LightType ExpectedLightType => LightType.Point;
#else
        protected override LightType ExpectedLightType => LightType.Area;
#endif
        protected override bool SupportsMixedLighting => false;

        protected override void ApplySpecificSettings(Light lightComponent)
        {
#if !UNITY_WEBGL
            lightComponent.areaSize = size;
#endif
        }

        protected override void OnValidate()
        {
            size.x = Mathf.Max(0.01f, size.x);
            size.y = Mathf.Max(0.01f, size.y);
            base.OnValidate();
        }
    }
}
