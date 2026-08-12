using UnityEngine;

namespace Abigobaldo.Game
{
    [CreateAssetMenu(menuName = "Abigobaldo/Demo Recipe Book")]
    public class DemoRecipeBook : ScriptableObject
    {
        [SerializeField] private RecipeData[] fryingPanRecipes;
        [SerializeField] private RecipeData[] blenderRecipes;
        [SerializeField] private RecipeData[] cuscuzeiraRecipes;

        public RecipeData FindFryingPanRecipe(ObjectKind inputKind)
        {
            return FindRecipe(fryingPanRecipes, inputKind);
        }

        public RecipeData FindBlenderRecipe(ObjectKind inputKind)
        {
            return FindRecipe(blenderRecipes, inputKind);
        }

        public RecipeData FindCuscuzeiraRecipe(ObjectKind inputKind)
        {
            return FindRecipe(cuscuzeiraRecipes, inputKind);
        }

        private static RecipeData FindRecipe(RecipeData[] recipes, ObjectKind inputKind)
        {
            if (recipes == null)
                return null;

            foreach (RecipeData recipe in recipes)
            {
                if (recipe != null && recipe.Matches(inputKind))
                    return recipe;
            }

            return null;
        }
    }
}
