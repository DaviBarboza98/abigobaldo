using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "RecipeDatabase",
    menuName = "Abigobaldos/Recipe Database"
)]
public class RecipeDatabase : ScriptableObject
{
    [SerializeField] private List<RecipeData> recipes = new List<RecipeData>();

    public bool TryFindRecipe(
        ContainerType containerType,
        IReadOnlyList<ObjectData> contents,
        out RecipeData recipe
    )
    {
        foreach (RecipeData candidate in recipes)
        {
            if (candidate == null)
                continue;

            if (!candidate.Matches(containerType, contents))
                continue;

            recipe = candidate;
            return true;
        }

        recipe = null;
        return false;
    }
}

