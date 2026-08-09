using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class RecipeContainer : MonoBehaviour, IInteractable, ItemHoldStateReceiver
{
    private enum CookingResultState
    {
        Raw,
        Ready,
        Overcooked,
        Burned,
        Carbonized
    }

    [Header("Container")]
    [SerializeField, HideInInspector] private ContainerType containerType;

    [Header("Capacidade")]
    [SerializeField] private int maxItems = 5;

    [Header("Receitas")]
    [SerializeField] private RecipeDatabase recipeDatabase;
    [SerializeField] private List<RecipeData> localRecipes = new List<RecipeData>();

    [Header("Saida de objeto pronto")]
    [SerializeField] private Transform outputSpawnPoint;

    [Header("Objeto pegavel")]
    [SerializeField] private bool canBePickedUp;
    [SerializeField] private ItemData containerItemData;

    [Header("Particulas configuradas no prefab")]
    [SerializeField] private ParticleEmitterController steamParticles;
    [SerializeField] private Color steamColor = new Color(0.85f, 0.85f, 0.85f, 0.45f);
    [SerializeField] private Color burnedSteamColor = new Color(0.25f, 0.22f, 0.2f, 0.6f);
    [SerializeField] private float steamRate = 8f;

    [Header("Ativacao")]
    [SerializeField] private bool requiresManualActivation;
    [SerializeField] private BlenderCupSlot requiredCupSlot;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private readonly List<ItemData> storedItems = new List<ItemData>();
    private CookingProcess cookingProcess;
    private RecipeData activeRecipe;
    private CookingResultState resultState;
    private int lastLoggedSecond = -1;
    private bool cookingEnabled = true;
    private Rigidbody containerBody;
    private Objeto containerItem;
    private bool loggedAlmostReady;

    public ContainerType Type => containerType;
    public IReadOnlyList<ItemData> StoredItems => storedItems;
    public int ItemCount => storedItems.Count;
    public bool IsEmpty => storedItems.Count == 0;
    public bool IsFull => storedItems.Count >= maxItems;
    public bool IsCooking => cookingProcess != null && cookingProcess.IsRunning && !cookingProcess.IsReady;
    public bool HasReadyOutput => cookingProcess != null && cookingProcess.IsReady && storedItems.Count == 1;
    public float CookingProgress => cookingProcess != null ? cookingProcess.Progress : 0f;
    public bool CanBePickedUp => canBePickedUp;
    protected IReadOnlyList<ItemData> CurrentContents => storedItems;
    protected CookingProcess CurrentCookingProcess => cookingProcess;
    protected RecipeData CurrentRecipe => activeRecipe;

    protected virtual void Awake()
    {
        GameLayers.SetLayerRecursivelyIfDefault(gameObject, GameLayers.Container);

        if (canBePickedUp)
            EnsurePickupComponents();

        cookingEnabled = !requiresManualActivation;
    }

    private void Update()
    {
        if (cookingProcess == null && steamParticles == null)
            return;

        UpdateCookingProcess();
        UpdateSpecificVisuals();
        UpdateSteam();
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

            if (TryToggleManualActivation())
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

        Objeto heldItem = holder.CurrentObjeto;

        if (heldItem == null)
            return false;

        if (heldItem.Data == null)
        {
            Debug.LogWarning($"{heldItem.name} nao possui dado de receita. Ele pode existir no mundo, mas nao entra em receita ainda.");
            return false;
        }

        Objeto removedItem = holder.RemoveObjeto();

        if (removedItem == null)
            return false;

        ItemData storedData = removedItem.Data;
        storedItems.Add(storedData);
        Destroy(removedItem.gameObject);

        Log($"{storedData.DisplayName} foi colocado em {containerType}. Total: {storedItems.Count}");
        RefreshVisuals();
        TryStartRecipe();

        return true;
    }

    public bool TryPickUpContainer(ItemHolder holder)
    {
        if (!canBePickedUp || holder == null || !holder.IsEmpty())
            return false;

        EnsurePickupComponents();

        return holder.TryPickUp(containerItem);
    }

    public bool TryToggleManualActivation()
    {
        if (!requiresManualActivation)
            return false;

        if (requiredCupSlot != null && !requiredCupSlot.IsOccupied)
        {
            Log($"{name}: encaixe o copo antes de ligar.");
            return true;
        }

        bool nextState = !cookingEnabled;
        SetCookingEnabled(nextState);
        Log(nextState ? $"{name}: ligado." : $"{name}: desligado.");

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
        Objeto outputItem = outputObject.GetComponent<Objeto>();

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
        RefreshVisuals();
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
        RefreshVisuals();
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
        {
            RefreshVisuals();
            TryStartRecipe();
        }

        return removed;
    }

    public void ClearContainer()
    {
        storedItems.Clear();
        cookingProcess = null;
        activeRecipe = null;
        resultState = CookingResultState.Raw;
        lastLoggedSecond = -1;
        loggedAlmostReady = false;
        RefreshVisuals();

        Log($"{name}: todos os itens foram removidos.");
    }

    public List<ItemData> GetContentsCopy()
    {
        return new List<ItemData>(storedItems);
    }

    private void TryStartRecipe()
    {
        if (cookingProcess != null)
            return;

        if (!TryFindRecipe(out RecipeData recipe))
            return;

        activeRecipe = recipe;
        cookingProcess = new CookingProcess(recipe.CookingTime);
        resultState = CookingResultState.Raw;
        lastLoggedSecond = -1;
        loggedAlmostReady = false;

        if (recipe.SpawnByproductsOnStart)
            SpawnByproducts(recipe.Byproducts);

        if (CanCookNow())
        {
            cookingProcess.Start();
            Log($"{name}: receita iniciada em {containerType}. Tempo: {recipe.CookingTime:0}s.");
            Log(recipe.ResultItem != null
                ? $"{name}: preparando {recipe.ResultItem.DisplayName}."
                : $"{name}: preparando receita sem resultado definido.");
        }
        else
        {
            Log($"{name}: receita montada, mas pausada. Coloque/ligue o container para cozinhar.");
        }

        if (recipe.CookingTime <= 0f)
            PrepareReadyOutput();
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
        RefreshVisuals();

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
        loggedAlmostReady = false;
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
            SetResultState(CookingResultState.Overcooked, activeRecipe.SlightlyBurnedResultItem, "passado");
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
            RefreshVisuals();
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

            if (!loggedAlmostReady && cookingProcess.Progress >= 0.8f)
            {
                loggedAlmostReady = true;
                Debug.Log($"{name}: quase no ponto.");
            }

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

    public void SetCookingEnabled(bool enabled)
    {
        if (enabled && requiredCupSlot != null && !requiredCupSlot.IsOccupied)
        {
            cookingEnabled = false;
            if (cookingProcess != null)
                cookingProcess.Pause();
            return;
        }

        cookingEnabled = enabled;

        if (cookingProcess == null)
        {
            if (enabled)
                TryStartRecipe();

            return;
        }

        if (enabled)
        {
            cookingProcess.Resume();
            Log($"{name}: cozimento retomado em {cookingProcess.Timer:0}s.");
        }
        else
        {
            cookingProcess.Pause();
            Log($"{name}: cozimento pausado em {cookingProcess.Timer:0}s.");
        }
    }

    public void OnPickedUp()
    {
        SetCookingEnabled(false);
    }

    public void OnDropped()
    {
        SetCookingEnabled(false);
    }

    public void OnThrown()
    {
        OnDropped();
    }

    private bool CanCookNow()
    {
        if (requiredCupSlot != null && !requiredCupSlot.IsOccupied)
            return false;

        return cookingEnabled;
    }

    public void SetRequiredCupSlot(BlenderCupSlot cupSlot)
    {
        requiredCupSlot = cupSlot;
    }

    public void NotifyRequiredCupChanged()
    {
        if (requiredCupSlot != null && !requiredCupSlot.IsOccupied)
            SetCookingEnabled(false);
        else if (cookingEnabled)
            TryStartRecipe();
    }

    protected void ConfigureContainerType(ContainerType type)
    {
        containerType = type;
    }

    private void EnsurePickupComponents()
    {
        containerItem = GetComponent<Objeto>();
        containerBody = GetComponent<Rigidbody>();

        if (containerBody == null)
            containerBody = gameObject.AddComponent<Rigidbody>();

        if (containerItem == null)
            containerItem = gameObject.AddComponent<Objeto>();

        containerItem.Configure(containerItemData, true, false);
    }

    protected virtual void RefreshVisuals()
    {
    }

    protected virtual void UpdateSpecificVisuals()
    {
    }

    private void UpdateSteam()
    {
        if (steamParticles == null)
            return;

        bool shouldEmit = cookingProcess != null && cookingProcess.IsRunning && cookingProcess.Timer > 0.5f;
        steamParticles.SetRate(steamRate);
        steamParticles.SetColor(resultState == CookingResultState.Burned || resultState == CookingResultState.Carbonized
            ? burnedSteamColor
            : steamColor);

        if (shouldEmit)
            steamParticles.Play();
        else
            steamParticles.Stop();
    }

    private void TryMakeBlenderJarTransparent()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            if (renderer.sharedMaterial == null)
                continue;

            if (!LooksLikeGlass(renderer))
                continue;

            Material transparentMaterial = new Material(renderer.sharedMaterial);
            Color color = transparentMaterial.HasProperty("_Color")
                ? transparentMaterial.color
                : Color.white;

            color.a = 0.28f;

            if (transparentMaterial.HasProperty("_Color"))
                transparentMaterial.color = color;

            if (transparentMaterial.HasProperty("_BaseColor"))
                transparentMaterial.SetColor("_BaseColor", color);

            SetMaterialFloatIfPresent(transparentMaterial, "_Mode", 3f);
            SetMaterialFloatIfPresent(transparentMaterial, "_Surface", 1f);
            SetMaterialFloatIfPresent(transparentMaterial, "_Blend", 0f);
            SetMaterialFloatIfPresent(transparentMaterial, "_AlphaClip", 0f);
            SetMaterialFloatIfPresent(transparentMaterial, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            SetMaterialFloatIfPresent(transparentMaterial, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            SetMaterialFloatIfPresent(transparentMaterial, "_ZWrite", 0f);
            transparentMaterial.DisableKeyword("_ALPHATEST_ON");
            transparentMaterial.EnableKeyword("_ALPHABLEND_ON");
            transparentMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            transparentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            transparentMaterial.renderQueue = 3000;

            renderer.material = transparentMaterial;
        }
    }

    private static bool LooksLikeGlass(Renderer renderer)
    {
        string rendererName = renderer.name.ToLowerInvariant();
        string materialName = renderer.sharedMaterial != null
            ? renderer.sharedMaterial.name.ToLowerInvariant()
            : string.Empty;

        return ContainsGlassToken(rendererName) || ContainsGlassToken(materialName);
    }

    private static bool ContainsGlassToken(string text)
    {
        return text.Contains("copo")
            || text.Contains("vidro")
            || text.Contains("jar")
            || text.Contains("glass")
            || text.Contains("liquidificador_copo");
    }

    private static void SetMaterialFloatIfPresent(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
            material.SetFloat(propertyName, value);
    }

    protected static void DisableGameplayComponents(GameObject visual)
    {
        foreach (Objeto item in visual.GetComponentsInChildren<Objeto>())
            item.enabled = false;

        foreach (RecipeContainer container in visual.GetComponentsInChildren<RecipeContainer>())
            container.enabled = false;

        foreach (PlateContainer plate in visual.GetComponentsInChildren<PlateContainer>())
            plate.enabled = false;

        foreach (Collider collider in visual.GetComponentsInChildren<Collider>())
            collider.enabled = false;

        foreach (Rigidbody body in visual.GetComponentsInChildren<Rigidbody>())
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.useGravity = false;
            body.isKinematic = true;
            body.detectCollisions = false;
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
        steamRate = Mathf.Max(0f, steamRate);
    }
}
