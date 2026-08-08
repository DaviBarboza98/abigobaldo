using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NovaReceita",
    menuName = "Abigobaldos/Recipe Data"
)]
public class RecipeData : ScriptableObject
{
    [Header("Estacao")]
    [SerializeField] private ContainerType requiredContainer;

    [Header("Ingredientes")]
    [SerializeField] private List<ItemData> ingredients = new();

    [Header("Resultado")]
    [SerializeField] private ItemData resultItem;
    [SerializeField] private List<ItemData> byproducts = new();
    [SerializeField] private bool spawnByproductsOnStart;

    [Header("Tempo")]
    [SerializeField] private float cookingTime = 3f;
    [SerializeField] private float burningTime;

    public ContainerType RequiredContainer => requiredContainer;
    public IReadOnlyList<ItemData> Ingredients => ingredients;
    public ItemData ResultItem => resultItem;
    public IReadOnlyList<ItemData> Byproducts => byproducts;
    public bool SpawnByproductsOnStart => spawnByproductsOnStart;
    public float CookingTime => cookingTime;
    public float BurningTime => burningTime;

    public bool CanRunIn(ContainerType containerType)
    {
        return requiredContainer == containerType;
    }

    public bool Matches(ContainerType containerType, IReadOnlyList<ItemData> contents)
    {
        if (!CanRunIn(containerType))
            return false;

        if (contents == null)
            return false;

        if (contents.Count != ingredients.Count)
            return false;

        List<ItemData> remaining = new List<ItemData>(contents);

        foreach (ItemData ingredient in ingredients)
        {
            if (ingredient == null)
                return false;

            if (!remaining.Remove(ingredient))
                return false;
        }

        return true;
    }

    private void OnValidate()
    {
        cookingTime = Mathf.Max(0f, cookingTime);
        burningTime = Mathf.Max(0f, burningTime);
    }
}
