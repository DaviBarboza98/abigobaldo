using System.Collections.Generic;
using UnityEngine;

namespace Abigobaldo.Game
{
    [CreateAssetMenu(fileName = "RecipeBook", menuName = "Abigobaldo/Recipe Book")]
    public sealed class RecipeBook : ScriptableObject
    {
        [Header("Global Outputs")]
        [Tooltip("Mandatory output used when any heated food reaches Carbonized.")]
        [SerializeField] private GameObject charcoalPrefab;

        [Header("Recipes")]
        [SerializeField] private RecipeData[] recipes;

        public GameObject CharcoalPrefab => charcoalPrefab;

        public RecipeData FindExact(RecipeStationType station, IReadOnlyList<ObjectDefinition> contents)
        {
            if (recipes == null)
                return null;

            foreach (RecipeData recipe in recipes)
            {
                if (recipe != null && recipe.Matches(station, contents))
                    return recipe;
            }

            return null;
        }

        public bool CanAccept(RecipeStationType station, IReadOnlyList<ObjectDefinition> contents)
        {
            if (recipes == null)
                return false;

            foreach (RecipeData recipe in recipes)
            {
                if (recipe != null && recipe.CanAccept(station, contents))
                    return true;
            }

            return false;
        }
    }
}
