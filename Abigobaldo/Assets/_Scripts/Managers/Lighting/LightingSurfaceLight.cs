using UnityEngine;

namespace Abigobaldo.Game
{
    [AddComponentMenu("Abigobaldo/Lighting/Surface Light")]
    [DisallowMultipleComponent]
    public sealed class LightingSurfaceLight : LightingLocalLight
    {
        [Tooltip("The local forward direction is the emitting face.")]
        [SerializeField] private Vector2 size = new Vector2(1f, 1f);

        protected override LightType ExpectedLightType => LightType.Area;
        protected override bool SupportsMixedLighting => false;

        protected override void ApplySpecificSettings(Light lightComponent)
        {
            lightComponent.areaSize = size;
        }

        protected override void OnValidate()
        {
            size.x = Mathf.Max(0.01f, size.x);
            size.y = Mathf.Max(0.01f, size.y);
            base.OnValidate();
        }
    }
}
