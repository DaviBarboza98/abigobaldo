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

        private void LateUpdate()
        {
            FoodState state = CurrentRecipeProgress != null ? CurrentRecipeProgress.State : FoodState.Raw;
            float volume = state >= FoodState.Burned ? 1f : state >= FoodState.Overdone ? 0.78f : 0.48f;
            GameSoundManager.SetFrying(HasContent && IsDocked, volume);
        }
    }
}
