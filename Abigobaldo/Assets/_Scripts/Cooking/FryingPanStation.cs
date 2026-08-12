using UnityEngine;

namespace Abigobaldo.Game
{
    public class FryingPanStation : ContainerStation
    {
        [SerializeField] private Transform itemAnchor;

        protected override ObjectVisualTarget VisualTarget => ObjectVisualTarget.FryingPan;

        protected override RecipeData FindRecipe(DemoRecipeBook book, ObjectKind inputKind)
        {
            return book.FindFryingPanRecipe(inputKind);
        }

        protected override Transform GetAnchor()
        {
            return itemAnchor != null ? itemAnchor : transform;
        }
    }
}
