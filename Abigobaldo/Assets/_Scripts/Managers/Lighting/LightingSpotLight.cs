using UnityEngine;

namespace Abigobaldo.Game
{
    [AddComponentMenu("Abigobaldo/Lighting/Spot Light")]
    [DisallowMultipleComponent]
    public sealed class LightingSpotLight : LightingLocalLight
    {
        [Range(1f, 179f)]
        [SerializeField] private float angle = 45f;
        [Range(0f, 179f)]
        [SerializeField] private float innerAngle = 30f;

        protected override LightType ExpectedLightType => LightType.Spot;

        protected override void ApplySpecificSettings(Light lightComponent)
        {
            lightComponent.spotAngle = angle;
            lightComponent.innerSpotAngle = Mathf.Min(innerAngle, angle);
        }

        protected override void OnValidate()
        {
            angle = Mathf.Clamp(angle, 1f, 179f);
            innerAngle = Mathf.Clamp(innerAngle, 0f, angle);
            base.OnValidate();
        }
    }
}
