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
    [SerializeField] private List<ItemData> ingredients = new List<ItemData>();

    [Header("Resultado")]
    [SerializeField] private ItemData resultItem;
    [SerializeField] private ItemData slightlyBurnedResultItem;
    [SerializeField] private ItemData burnedResultItem;
    [SerializeField] private ItemData carbonizedResultItem;
    [SerializeField] private List<ItemData> byproducts = new List<ItemData>();
    [SerializeField] private bool spawnByproductsOnStart;

    [Header("Tempo")]
    [SerializeField] private float cookingTime = 3f;
    [SerializeField] private bool canOvercook;
    [SerializeField] private float slightlyBurnedDelay = 5f;
    [SerializeField] private float burnedDelay = 10f;
    [SerializeField] private float carbonizedDelay = 15f;

    public ContainerType RequiredContainer => requiredContainer;
    public IReadOnlyList<ItemData> Ingredients => ingredients;
    public ItemData ResultItem => resultItem;
    public ItemData SlightlyBurnedResultItem => slightlyBurnedResultItem;
    public ItemData BurnedResultItem => burnedResultItem;
    public ItemData CarbonizedResultItem => carbonizedResultItem;
    public IReadOnlyList<ItemData> Byproducts => byproducts;
    public bool SpawnByproductsOnStart => spawnByproductsOnStart;
    public float CookingTime => cookingTime;
    public bool CanOvercook => canOvercook;
    public float SlightlyBurnedDelay => slightlyBurnedDelay;
    public float BurnedDelay => burnedDelay;
    public float CarbonizedDelay => carbonizedDelay;

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
        slightlyBurnedDelay = Mathf.Max(0f, slightlyBurnedDelay);
        burnedDelay = Mathf.Max(slightlyBurnedDelay, burnedDelay);
        carbonizedDelay = Mathf.Max(burnedDelay, carbonizedDelay);
    }
}
