using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewRecipe",
    menuName = "Abigobaldos/Recipe Data"
)]
public class RecipeData : ScriptableObject
{
    [Header("Station")]
    [SerializeField] private ContainerType requiredContainer;

    [Header("Ingredients")]
    [SerializeField] private List<ObjectData> ingredients = new List<ObjectData>();

    [Header("Result")]
    [SerializeField] private ObjectData resultObject;
    [SerializeField] private Material readyMaterial;
    [SerializeField] private Material overcookedMaterial;
    [SerializeField] private Material burnedMaterial;
    [SerializeField] private Material carbonizedMaterial;
    [SerializeField] private List<ObjectData> byproducts = new List<ObjectData>();
    [SerializeField] private bool spawnByproductsOnStart;

    [Header("Timing")]
    [SerializeField] private float cookingTime = 3f;
    [SerializeField] private bool canOvercook;
    [SerializeField] private float slightlyBurnedDelay = 5f;
    [SerializeField] private float burnedDelay = 10f;
    [SerializeField] private float carbonizedDelay = 15f;

    public ContainerType RequiredContainer => requiredContainer;
    public IReadOnlyList<ObjectData> Ingredients => ingredients;
    public ObjectData ResultObject => resultObject;
    public Material ReadyMaterial => readyMaterial;
    public Material OvercookedMaterial => overcookedMaterial;
    public Material BurnedMaterial => burnedMaterial;
    public Material CarbonizedMaterial => carbonizedMaterial;
    public IReadOnlyList<ObjectData> Byproducts => byproducts;
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

    public bool Matches(ContainerType containerType, IReadOnlyList<ObjectData> contents)
    {
        if (!CanRunIn(containerType))
            return false;

        if (contents == null)
            return false;

        if (contents.Count != ingredients.Count)
            return false;

        List<ObjectData> remaining = new List<ObjectData>(contents);

        foreach (ObjectData ingredient in ingredients)
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


