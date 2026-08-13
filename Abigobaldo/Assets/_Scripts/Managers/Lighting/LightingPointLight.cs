using UnityEngine;

namespace Abigobaldo.Game
{
    [AddComponentMenu("Abigobaldo/Lighting/Point Light")]
    [DisallowMultipleComponent]
    public sealed class LightingPointLight : LightingLocalLight
    {
        protected override LightType ExpectedLightType => LightType.Point;
    }
}
