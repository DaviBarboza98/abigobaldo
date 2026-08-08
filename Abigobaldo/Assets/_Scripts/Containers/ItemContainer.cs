using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ItemContainer : MonoBehaviour, IInteractable
{
    private enum CookingResultState
    {
        Raw,
        Ready,
        SlightlyBurned,
        Burned,
        Carbonized
    }

    [Header("Container")]
    [SerializeField] private ContainerType containerType;

    [Header("Capacidade")]
    [SerializeField] private int maxItems = 5;

    [Header("Receitas")]
    [SerializeField] private RecipeDatabase recipeDatabase;
    [SerializeField] private List<RecipeData> localRecipes = new();

    [Header("Saida")]
    [SerializeField] private Transform outputSpawnPoint;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private readonly List<ItemData> storedItems = new();
    private CookingProcess cookingProcess;
    private RecipeData activeRecipe;
    private CookingResultState resultState;
    private int lastLoggedSecond = -1;

    public ContainerType Type => containerType;
    public IReadOnlyList<ItemData> StoredItems => storedItems;
    public int ItemCount => storedItems.Count;
    public bool IsEmpty => storedItems.Count == 0;
    public bool IsFull => storedItems.Count >= maxItems;
    public bool IsCooking => cookingProcess != null && cookingProcess.IsRunning && !cookingProcess.IsReady;
    public bool HasReadyOutput => cookingProcess != null && cookingProcess.IsReady && storedItems.Count == 1;
    public float CookingProgress => cookingProcess != null ? cookingProcess.Progress : 0f;

    private void Update()
    {
        UpdateCookingProcess();
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null)
            return;

        ItemHolder holder = player.ItemHolder;

        if (holder == null)
            return;

        if (holder.IsEmpty())
        {
            if (TryTakeOutput(holder))
                return;

            LogContents();
            return;
        }

        TryStoreHeldItem(holder);
    }

    public bool TryStoreHeldItem(ItemHolder holder)
    {
        if (holder == null || holder.IsEmpty())
            return false;

        if (cookingProcess != null)
        {
            Log(cookingProcess.IsReady
                ? $"{name}: retire ou emprate o item pronto antes de colocar outro ingrediente."
                : $"{name}: espere a receita terminar.");
            return false;
        }

        if (IsFull)
        {
            Log($"{name}: o container esta cheio.");
            return false;
        }

        Item heldItem = holder.CurrentItem;

        if (heldItem == null)
            return false;

        if (heldItem.Data == null)
        {
            Debug.LogWarning($"{heldItem.name} nao possui ItemData.");
            return false;
        }

        Item removedItem = holder.RemoveItem();

        if (removedItem == null)
            return false;

        ItemData storedData = removedItem.Data;
        storedItems.Add(storedData);
        Destroy(removedItem.gameObject);

        Log($"{storedData.DisplayName} foi colocado em {containerType}. Total: {storedItems.Count}");
        TryStartRecipe();

        return true;
    }

    public bool TryTakeOutput(ItemHolder holder)
    {
        if (holder == null || !holder.IsEmpty())
            return false;

        if (cookingProcess != null && !cookingProcess.IsReady)
            return false;

        if (storedItems.Count != 1)
            return false;

        ItemData outputData = GetCurrentOutputData();

        if (outputData == null || outputData.Prefab == null)
            return false;

        Vector3 spawnPosition = outputSpawnPoint != null
            ? outputSpawnPoint.position
            : transform.position + Vector3.up * 0.35f;

        Quaternion spawnRotation = outputSpawnPoint != null
            ? outputSpawnPoint.rotation
            : transform.rotation;

        GameObject outputObject = Instantiate(outputData.Prefab, spawnPosition, spawnRotation);
        Item outputItem = outputObject.GetComponent<Item>();

        if (outputItem == null)
        {
            Destroy(outputObject);
            return false;
        }

        if (!holder.TryPickUp(outputItem))
        {
            Destroy(outputObject);
            return false;
        }

        FinishAndClearCooking();
        Log($"{outputData.DisplayName} foi retirado de {containerType}.");

        return true;
    }

    public bool TryMoveOutputToPlate(PlateContainer plate)
    {
        if (plate == null)
            return false;

        if (cookingProcess != null && !cookingProcess.IsReady)
            return false;

        if (storedItems.Count != 1)
            return false;

        ItemData outputData = GetCurrentOutputData();

        if (!plate.TryAddItem(outputData))
            return false;

        FinishAndClearCooking();
        Log($"{outputData.DisplayName} foi colocado no prato.");

        return true;
    }

    public bool ContainsItem(ItemData itemData)
    {
        return itemData != null && storedItems.Contains(itemData);
    }

    public int CountItem(ItemData itemData)
    {
        if (itemData == null)
            return 0;

        int amount = 0;

        foreach (ItemData storedItem in storedItems)
        {
            if (storedItem == itemData)
                amount++;
        }

        return amount;
    }

    public bool RemoveItem(ItemData itemData)
    {
        if (itemData == null)
            return false;

        bool removed = storedItems.Remove(itemData);

        if (removed)
            TryStartRecipe();

        return removed;
    }

    public void ClearContainer()
    {
        storedItems.Clear();
        cookingProcess = null;
        activeRecipe = null;
        resultState = CookingResultState.Raw;
        lastLoggedSecond = -1;

        Log($"{name}: todos os itens foram removidos.");
    }

    public List<ItemData> GetContentsCopy()
    {
        return new List<ItemData>(storedItems);
    }

    private void TryStartRecipe()
    {
        if (IsCooking)
            return;

        if (!TryFindRecipe(out RecipeData recipe))
            return;

        activeRecipe = recipe;
        cookingProcess = new CookingProcess(recipe.CookingTime);
        cookingProcess.Start();
        resultState = CookingResultState.Raw;
        lastLoggedSecond = -1;

        if (recipe.SpawnByproductsOnStart)
            SpawnByproducts(recipe.Byproducts);

        if (recipe.CookingTime <= 0f)
            PrepareReadyOutput();
        else
        {
            Log($"{name}: receita iniciada em {containerType}. Tempo: {recipe.CookingTime:0}s.");
            Log(recipe.ResultItem != null
                ? $"{name}: preparando {recipe.ResultItem.DisplayName}."
                : $"{name}: preparando receita sem resultado definido.");
        }
    }

    private bool TryFindRecipe(out RecipeData recipe)
    {
        foreach (RecipeData localRecipe in localRecipes)
        {
            if (localRecipe == null)
                continue;

            if (!localRecipe.Matches(containerType, storedItems))
                continue;

            recipe = localRecipe;
            return true;
        }

        if (recipeDatabase != null)
            return recipeDatabase.TryFindRecipe(containerType, storedItems, out recipe);

        recipe = null;
        return false;
    }

    private void UpdateCookingProcess()
    {
        if (cookingProcess == null || !cookingProcess.IsRunning)
            return;

        cookingProcess.Update(Time.deltaTime);
        LogCookingTimer();

        if (cookingProcess.IsReady && resultState == CookingResultState.Raw)
            PrepareReadyOutput();

        UpdateOvercookState();
    }

    private void PrepareReadyOutput()
    {
        if (activeRecipe == null)
            return;

        ItemData result = activeRecipe.ResultItem;
        IReadOnlyList<ItemData> byproducts = activeRecipe.Byproducts;

        storedItems.Clear();

        if (result != null)
            storedItems.Add(result);

        if (!activeRecipe.SpawnByproductsOnStart)
            SpawnByproducts(byproducts);

        resultState = CookingResultState.Ready;

        Log(result != null
            ? $"{name}: pronto: {result.DisplayName}. Retire agora ou ele vai passar do ponto."
            : $"{name}: receita terminou sem resultado.");
    }

    private void FinishAndClearCooking()
    {
        if (cookingProcess != null)
            cookingProcess.Stop();

        storedItems.Clear();
        cookingProcess = null;
        activeRecipe = null;
        resultState = CookingResultState.Raw;
        lastLoggedSecond = -1;
    }

    private void UpdateOvercookState()
    {
        if (activeRecipe == null || cookingProcess == null || !cookingProcess.IsReady)
            return;

        if (activeRecipe == null || !activeRecipe.CanOvercook)
            return;

        float overcookTime = cookingProcess.OvercookTime;

        if (overcookTime >= activeRecipe.CarbonizedDelay)
        {
            SetResultState(CookingResultState.Carbonized, activeRecipe.CarbonizedResultItem, "carbonizado");
            return;
        }

        if (overcookTime >= activeRecipe.BurnedDelay)
        {
            SetResultState(CookingResultState.Burned, activeRecipe.BurnedResultItem, "queimado");
            return;
        }

        if (overcookTime >= activeRecipe.SlightlyBurnedDelay)
            SetResultState(CookingResultState.SlightlyBurned, activeRecipe.SlightlyBurnedResultItem, "um pouco queimado");
    }

    private void SetResultState(CookingResultState newState, ItemData stateItem, string label)
    {
        if (resultState == newState)
            return;

        resultState = newState;

        if (stateItem != null)
        {
            storedItems.Clear();
            storedItems.Add(stateItem);
        }

        ItemData output = GetCurrentOutputData();
        Log(output != null
            ? $"{name}: {output.DisplayName} ficou {label}."
            : $"{name}: o item ficou {label}.");
    }

    private ItemData GetCurrentOutputData()
    {
        return storedItems.Count > 0 ? storedItems[0] : null;
    }

    private void LogCookingTimer()
    {
        if (!showDebugLogs || cookingProcess == null)
            return;

        int currentSecond = Mathf.FloorToInt(cookingProcess.Timer);

        if (currentSecond == lastLoggedSecond)
            return;

        lastLoggedSecond = currentSecond;

        if (!cookingProcess.IsReady)
        {
            float remaining = Mathf.Max(0f, cookingProcess.CookingTime - cookingProcess.Timer);
            Debug.Log($"{name}: cozinhando... faltam {remaining:0}s.");
            return;
        }

        if (!activeRecipe.CanOvercook)
            return;

        Debug.Log($"{name}: pronto ha {cookingProcess.OvercookTime:0}s. +5 pouco queimado, +10 queimado, +15 carbonizado.");
    }

    private void SpawnByproducts(IReadOnlyList<ItemData> byproducts)
    {
        if (byproducts == null || byproducts.Count == 0)
            return;

        Vector3 basePosition = outputSpawnPoint != null
            ? outputSpawnPoint.position
            : transform.position + Vector3.up * 0.35f;

        Quaternion baseRotation = outputSpawnPoint != null
            ? outputSpawnPoint.rotation
            : transform.rotation;

        for (int i = 0; i < byproducts.Count; i++)
        {
            ItemData byproduct = byproducts[i];

            if (byproduct == null || byproduct.Prefab == null)
                continue;

            Vector3 offset = transform.right * ((i - (byproducts.Count - 1) * 0.5f) * 0.22f);
            GameObject spawnedObject = Instantiate(byproduct.Prefab, basePosition + offset, baseRotation);
            Rigidbody spawnedBody = spawnedObject.GetComponent<Rigidbody>();

            if (spawnedBody != null)
            {
                spawnedBody.velocity = Vector3.zero;
                spawnedBody.angularVelocity = Vector3.zero;
            }
        }
    }

    private void LogContents()
    {
        if (!showDebugLogs)
            return;

        if (storedItems.Count == 0)
        {
            Debug.Log($"{name}: o container esta vazio.");
            return;
        }

        StringBuilder contents = new StringBuilder();

        for (int i = 0; i < storedItems.Count; i++)
        {
            ItemData item = storedItems[i];
            contents.Append(item != null ? item.DisplayName : "Item nulo");

            if (i < storedItems.Count - 1)
                contents.Append(", ");
        }

        Debug.Log($"{name} contem: {contents}");
    }

    private void Log(string message)
    {
        if (showDebugLogs)
            Debug.Log(message);
    }

    private void OnValidate()
    {
        maxItems = Mathf.Max(1, maxItems);
    }
}
