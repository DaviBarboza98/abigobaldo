using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ItemContainer : MonoBehaviour, IInteractable, ItemHoldStateReceiver
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
    [SerializeField] private ContainerType containerType;

    [Header("Capacidade")]
    [SerializeField] private int maxItems = 5;

    [Header("Receitas")]
    [SerializeField] private RecipeDatabase recipeDatabase;
    [SerializeField] private List<RecipeData> localRecipes = new List<RecipeData>();

    [Header("Saida")]
    [SerializeField] private Transform outputSpawnPoint;

    [Header("Item pegavel")]
    [SerializeField] private bool canBePickedUp;
    [SerializeField] private ItemData containerItemData;
    [SerializeField] private bool createStoveSlotOnAwake;
    [SerializeField] private Vector3 stoveSlotSize = new Vector3(0.75f, 0.35f, 0.75f);

    [Header("Visual interno")]
    [SerializeField] private Transform contentVisualRoot;
    [SerializeField] private Vector3 contentVisualLocalOffset = new Vector3(0f, 0.12f, 0f);
    [SerializeField] private float contentVisualScale = 0.22f;
    [SerializeField] private float fryingMotionRadius = 0.025f;
    [SerializeField] private float fryingMotionSpeed = 18f;
    [SerializeField] private float blenderMorphStartTime = 2f;
    [SerializeField] private float blenderMorphDuration = 3f;
    [SerializeField] private float blenderShrinkScale = 0.03f;

    [Header("Particulas")]
    [SerializeField] private bool createDefaultSteam = true;
    [SerializeField] private Transform particlesRoot;
    [SerializeField] private Vector3 particlesLocalOffset = new Vector3(0f, 0.25f, 0f);
    [SerializeField] private Color steamColor = new Color(0.85f, 0.85f, 0.85f, 0.45f);
    [SerializeField] private Color burnedSteamColor = new Color(0.25f, 0.22f, 0.2f, 0.6f);
    [SerializeField] private float steamRate = 8f;
    [SerializeField] private float particleLifetime = 1.1f;
    [SerializeField] private float particleSpeed = 0.45f;
    [SerializeField] private float particleSize = 0.12f;
    [SerializeField] private float particleRadius = 0.12f;
    [SerializeField] private float particleConeAngle = 18f;
    [SerializeField] private Sprite particleSprite;

    [Header("Ativacao")]
    [SerializeField] private bool requiresManualActivation;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private readonly List<ItemData> storedItems = new List<ItemData>();
    private CookingProcess cookingProcess;
    private RecipeData activeRecipe;
    private CookingResultState resultState;
    private int lastLoggedSecond = -1;
    private readonly List<GameObject> contentVisuals = new List<GameObject>();
    private bool cookingEnabled = true;
    private bool isInstalledOnHeat;
    private ParticleSystem steamParticles;
    private Rigidbody containerBody;
    private Item containerItem;
    private StoveSlot homeSlot;
    private bool loggedAlmostReady;
    private bool blenderVisualMorphed;

    public ContainerType Type => containerType;
    public IReadOnlyList<ItemData> StoredItems => storedItems;
    public int ItemCount => storedItems.Count;
    public bool IsEmpty => storedItems.Count == 0;
    public bool IsFull => storedItems.Count >= maxItems;
    public bool IsCooking => cookingProcess != null && cookingProcess.IsRunning && !cookingProcess.IsReady;
    public bool HasReadyOutput => cookingProcess != null && cookingProcess.IsReady && storedItems.Count == 1;
    public float CookingProgress => cookingProcess != null ? cookingProcess.Progress : 0f;
    public bool CanBePickedUp => canBePickedUp;
    public bool IsInstalledOnHeat => isInstalledOnHeat;
    public bool NeedsHeat => containerType == ContainerType.Frigideira || containerType == ContainerType.Cuscuzeira;

    private void Awake()
    {
        if (contentVisualRoot == null)
            contentVisualRoot = containerType == ContainerType.Liquidificador
                ? GetOrCreateContentVisualRoot()
                : transform;

        if (canBePickedUp)
            EnsurePickupComponents();

        if (createDefaultSteam)
        {
            if (particlesRoot == null)
                particlesRoot = RuntimeParticleFactory.GetOrCreateParticlesRoot(transform, particlesLocalOffset);

            steamParticles = RuntimeParticleFactory.CreateSteam(
                particlesRoot,
                $"{name}_Steam",
                steamColor,
                steamRate,
                particleLifetime,
                particleSpeed,
                particleSize,
                particleRadius,
                particleConeAngle,
                particleSprite
            );
        }

        if (containerType == ContainerType.Liquidificador)
            TryMakeBlenderJarTransparent();

        if (createStoveSlotOnAwake && NeedsHeat)
        {
            homeSlot = StoveSlot.CreateFor(this, stoveSlotSize);
            DockToSlot(homeSlot);
        }
        else
        {
            cookingEnabled = !NeedsHeat && !requiresManualActivation;
        }
    }

    private void Update()
    {
        if (cookingProcess == null && steamParticles == null)
            return;

        UpdateCookingProcess();
        UpdateContentVisualMotion();
        UpdateBlenderVisualTransition();
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
        RefreshContentVisuals();
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
        RefreshContentVisuals();
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
        RefreshContentVisuals();
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
            RefreshContentVisuals();
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
        blenderVisualMorphed = false;
        RefreshContentVisuals();

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
        blenderVisualMorphed = false;

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
        RefreshContentVisuals();

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
        blenderVisualMorphed = false;
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
            RefreshContentVisuals();
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

    public void DockToSlot(StoveSlot slot)
    {
        if (slot == null)
            return;

        homeSlot = slot;
        isInstalledOnHeat = true;

        transform.SetParent(null);
        transform.SetPositionAndRotation(slot.ContainerAnchor.position, slot.ContainerAnchor.rotation);

        EnsurePickupComponents();
        containerBody.isKinematic = true;
        containerBody.useGravity = false;
        containerBody.velocity = Vector3.zero;
        containerBody.angularVelocity = Vector3.zero;

        SetCookingEnabled(true);
    }

    public void OnPickedUp()
    {
        isInstalledOnHeat = false;
        SetCookingEnabled(false);

        if (homeSlot != null)
            homeSlot.ClearIfCurrent(this);
    }

    public void OnDropped()
    {
        isInstalledOnHeat = false;
        SetCookingEnabled(false);
    }

    public void OnThrown()
    {
        OnDropped();
    }

    private bool CanCookNow()
    {
        if (NeedsHeat)
            return cookingEnabled && isInstalledOnHeat;

        return cookingEnabled;
    }

    private void EnsurePickupComponents()
    {
        containerItem = GetComponent<Item>();
        containerBody = GetComponent<Rigidbody>();

        if (containerBody == null)
            containerBody = gameObject.AddComponent<Rigidbody>();

        if (containerItem == null)
            containerItem = gameObject.AddComponent<Item>();

        containerItem.Configure(containerItemData, true, false);
    }

    private void RefreshContentVisuals()
    {
        RefreshContentVisuals(null);
    }

    private void RefreshContentVisuals(ItemData overrideSingleVisual)
    {
        for (int i = contentVisuals.Count - 1; i >= 0; i--)
        {
            if (contentVisuals[i] != null)
                Destroy(contentVisuals[i]);
        }

        contentVisuals.Clear();

        if (contentVisualRoot == null)
            return;

        if (overrideSingleVisual != null)
        {
            CreateContentVisual(overrideSingleVisual, 0, 1);
            return;
        }

        int count = storedItems.Count;

        for (int i = 0; i < count; i++)
        {
            ItemData item = storedItems[i];
            CreateContentVisual(item, i, count);
        }
    }

    private void CreateContentVisual(ItemData item, int index, int count)
    {
        if (item == null || item.Prefab == null)
            return;

        GameObject visual = Instantiate(item.Prefab, contentVisualRoot);
        visual.name = $"Visual_{item.DisplayName}";
        visual.transform.localPosition = GetContentVisualPosition(index, count);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * contentVisualScale;
        DisableGameplayComponents(visual);
        contentVisuals.Add(visual);
    }

    private Vector3 GetContentVisualPosition(int index, int count)
    {
        if (count <= 1)
            return contentVisualLocalOffset;

        float angle = Mathf.PI * 2f * index / count;
        Vector3 radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        return contentVisualLocalOffset + radial * 0.14f;
    }

    private void UpdateContentVisualMotion()
    {
        if (!IsCooking || contentVisuals.Count == 0)
            return;

        for (int i = 0; i < contentVisuals.Count; i++)
        {
            GameObject visual = contentVisuals[i];

            if (visual == null)
                continue;

            float phase = Time.time * fryingMotionSpeed + i * 1.73f;
            Vector3 motion = new Vector3(Mathf.Sin(phase), 0f, Mathf.Cos(phase * 0.8f));
            visual.transform.localPosition = GetContentVisualPosition(i, contentVisuals.Count)
                + motion * fryingMotionRadius;
        }
    }

    private void UpdateBlenderVisualTransition()
    {
        if (containerType != ContainerType.Liquidificador)
            return;

        if (blenderVisualMorphed || activeRecipe == null || cookingProcess == null)
            return;

        if (!cookingProcess.IsRunning || cookingProcess.Timer < blenderMorphStartTime)
            return;

        float morphEndTime = blenderMorphStartTime + blenderMorphDuration;
        float transition = blenderMorphDuration <= 0f
            ? 1f
            : Mathf.InverseLerp(blenderMorphStartTime, morphEndTime, cookingProcess.Timer);

        float visualScale = Mathf.Lerp(contentVisualScale, blenderShrinkScale, transition);

        for (int i = 0; i < contentVisuals.Count; i++)
        {
            GameObject visual = contentVisuals[i];

            if (visual != null)
                visual.transform.localScale = Vector3.one * visualScale;
        }

        if (transition < 1f)
            return;

        blenderVisualMorphed = true;
        RefreshContentVisuals(activeRecipe.ResultItem);

        if (activeRecipe.ResultItem != null)
            Log($"{name}: os ingredientes comecaram a virar {activeRecipe.ResultItem.DisplayName}.");
    }

    private Transform GetOrCreateContentVisualRoot()
    {
        Transform existingRoot = transform.Find("ContentVisualRoot");

        if (existingRoot != null)
            return existingRoot;

        GameObject rootObject = new GameObject("ContentVisualRoot");
        Transform root = rootObject.transform;
        root.SetParent(transform, false);
        root.localPosition = Vector3.zero;
        root.localRotation = Quaternion.identity;
        root.localScale = Vector3.one;

        return root;
    }

    private void UpdateSteam()
    {
        if (steamParticles == null)
            return;

        bool shouldEmit = cookingProcess != null && cookingProcess.IsRunning && cookingProcess.Timer > 0.5f;
        ParticleSystem.EmissionModule emission = steamParticles.emission;
        emission.enabled = shouldEmit;

        ParticleSystem.MainModule main = steamParticles.main;
        main.startColor = resultState == CookingResultState.Burned || resultState == CookingResultState.Carbonized
            ? burnedSteamColor
            : steamColor;
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

            transparentMaterial.SetFloat("_Mode", 3f);
            transparentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            transparentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            transparentMaterial.SetInt("_ZWrite", 0);
            transparentMaterial.DisableKeyword("_ALPHATEST_ON");
            transparentMaterial.EnableKeyword("_ALPHABLEND_ON");
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

    private static void DisableGameplayComponents(GameObject visual)
    {
        foreach (Item item in visual.GetComponentsInChildren<Item>())
            item.enabled = false;

        foreach (ItemContainer container in visual.GetComponentsInChildren<ItemContainer>())
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
        particleLifetime = Mathf.Max(0.01f, particleLifetime);
        particleSpeed = Mathf.Max(0f, particleSpeed);
        particleSize = Mathf.Max(0.01f, particleSize);
        particleRadius = Mathf.Max(0f, particleRadius);
        particleConeAngle = Mathf.Max(0f, particleConeAngle);
        blenderMorphStartTime = Mathf.Max(0f, blenderMorphStartTime);
        blenderMorphDuration = Mathf.Max(0f, blenderMorphDuration);
        blenderShrinkScale = Mathf.Max(0.001f, blenderShrinkScale);
    }
}
