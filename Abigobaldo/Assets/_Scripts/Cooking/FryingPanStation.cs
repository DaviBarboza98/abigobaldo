using UnityEngine;
using UnityEngine.Serialization;

namespace Abigobaldo.Game
{
    public class FryingPanStation : HeatedContainerStation
    {
        [FormerlySerializedAs("itemAnchor")]
        [SerializeField] private Transform contentAnchor;

        protected override RecipeStationType StationType => RecipeStationType.FryingPan;
        protected override ObjectVisualTarget VisualTarget => ObjectVisualTarget.FryingPan;

        protected override Transform GetContentAnchor()
        {
            return contentAnchor != null ? contentAnchor : transform;
        }
    }
}
