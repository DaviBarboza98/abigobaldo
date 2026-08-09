using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class FryingPan : MonoBehaviour, IRecipeStation, ItemHoldStateReceiver
{
    private enum ResultState { Raw, Ready, Overcooked, Burned, Carbonized }

    [Header("Receitas")]
    [SerializeField] private RecipeDatabase recipeDatabase;
    [SerializeField] private List<RecipeData> localRecipes = new List<RecipeData>();
    [SerializeField] private int maxItems = 3;

    [Header("Objeto pegavel")]
    [SerializeField] private bool canBePickedUp = true;
    [SerializeField] private ItemData panData;

    [Header("Visual da frigideira")]
    [SerializeField] private Transform itemSurface;
    [SerializeField] private Vector3 itemLocalOffset = new Vector3(0f, 0.08f, 0f);
    [SerializeField] private float itemVisualScale = 0.22f;
    [SerializeField] private float fryingMotionRadius = 0.025f;
    [SerializeField] private float fryingMotionSpeed = 18f;

    [Header("Particulas")]
    [SerializeField] private ParticleEmitterController steamParticles;
    [SerializeField] private Color steamColor = new Color(0.85f, 0.85f, 0.85f, 0.45f);
    [SerializeField] private Color burnedSteamColor = new Color(0.25f, 0.22f, 0.2f, 0.6f);
    [SerializeField] private float steamRate = 8f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private readonly List<ItemData> contents = new List<ItemData>();
    private readonly List<GameObject> visuals = new List<GameObject>();
    private CookingProcess cookingProcess;
    private RecipeData activeRecipe;
    private ResultState resultState;
    private readonly List<ItemData> pendingByproducts = new List<ItemData>();
    private int lastLoggedSecond = -1;
    private bool loggedAlmostReady;
    private bool cookingEnabled = true;
    private Rigidbody rb;
    private Objeto objeto;

    public bool HasReadyOutput => cookingProcess != null && cookingProcess.IsReady && contents.Count == 1;
    private bool IsCooking => cookingProcess != null && cookingProcess.IsRunning && !cookingProcess.IsReady;

    private void Awake()
    {
        GameLayers.SetLayerRecursivelyIfDefault(gameObject, GameLayers.Container);
        if (canBePickedUp)
            EnsurePickupComponents();
    }

    private void Update()
    {
        UpdateCooking();
        UpdateVisualMotion();
        UpdateSteam();
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.ItemHolder == null)
            return;

        if (player.ItemHolder.IsEmpty())
        {
            if (TryTakeOutput(player.ItemHolder) || TryTakePendingByproduct(player.ItemHolder))
                return;

            LogContents();
            return;
        }

        TryStoreHeldObject(player.ItemHolder);
    }

    public bool TryPickUpContainer(ItemHolder holder)
    {
        if (!canBePickedUp || holder == null || !holder.IsEmpty())
            return false;

        EnsurePickupComponents();
        return holder.TryPickUp(objeto);
    }

    public bool TryMoveOutputToPlate(PlateContainer plate)
    {
        if (plate == null)
            return false;

        if (cookingProcess != null && cookingProcess.IsReady && contents.Count == 1)
        {
            if (!plate.TryAddItem(contents[0]))
                return false;

            FinishAndClearRecipe();
            RefreshVisuals();
            return true;
        }

        return TryMovePendingByproductToPlate(plate);
    }

    private bool TryStoreHeldObject(ItemHolder holder)
    {
        if (cookingProcess != null || contents.Count >= maxItems)
            return false;

        Objeto held = holder.CurrentObjeto;
        if (held == null || held.Data == null)
        {
            Debug.LogWarning($"{held?.name ?? "Objeto"} nao possui dado de receita.");
            return false;
        }

        Objeto removed = holder.RemoveObjeto();
        if (removed == null)
            return false;

        contents.Add(removed.Data);
        Destroy(removed.gameObject);
        RefreshVisuals();
        TryStartRecipe();
        return true;
    }

    private bool TryTakeOutput(ItemHolder holder)
    {
        if (holder == null || !holder.IsEmpty() || cookingProcess == null || !cookingProcess.IsReady || contents.Count != 1)
            return false;

        ItemData output = contents[0];
        if (output == null || output.Prefab == null)
            return false;

        if (!ObjetoDeliveryUtility.TryDeliverToHolder(output, holder))
            return false;

        FinishAndClearRecipe();
        RefreshVisuals();
        return true;
    }

    private void TryStartRecipe()
    {
        if (cookingProcess != null || !TryFindRecipe(out RecipeData recipe))
            return;

        activeRecipe = recipe;
        cookingProcess = new CookingProcess(recipe.CookingTime);
        resultState = ResultState.Raw;
        lastLoggedSecond = -1;
        loggedAlmostReady = false;

        if (recipe.SpawnByproductsOnStart)
            QueueByproducts(recipe.Byproducts);

        if (cookingEnabled)
            cookingProcess.Start();

        Log($"{name}: receita iniciada na frigideira. Tempo: {recipe.CookingTime:0}s.");
    }

    private bool TryFindRecipe(out RecipeData recipe)
    {
        foreach (RecipeData localRecipe in localRecipes)
        {
            if (localRecipe != null && localRecipe.Matches(ContainerType.Frigideira, contents))
            {
                recipe = localRecipe;
                return true;
            }
        }

        if (recipeDatabase != null)
            return recipeDatabase.TryFindRecipe(ContainerType.Frigideira, contents, out recipe);

        recipe = null;
        return false;
    }

    private void UpdateCooking()
    {
        if (cookingProcess == null || !cookingProcess.IsRunning)
            return;

        cookingProcess.Update(Time.deltaTime);
        LogTimer();

        if (cookingProcess.IsReady && resultState == ResultState.Raw)
            PrepareOutput();

        UpdateOvercook();
    }

    private void PrepareOutput()
    {
        contents.Clear();
        if (activeRecipe.ResultItem != null)
            contents.Add(activeRecipe.ResultItem);

        if (!activeRecipe.SpawnByproductsOnStart)
            QueueByproducts(activeRecipe.Byproducts);

        resultState = ResultState.Ready;
        RefreshVisuals();
    }

    private void UpdateOvercook()
    {
        if (activeRecipe == null || cookingProcess == null || !cookingProcess.IsReady || !activeRecipe.CanOvercook)
            return;

        float over = cookingProcess.OvercookTime;
        if (over >= activeRecipe.CarbonizedDelay)
            SetResultState(ResultState.Carbonized, activeRecipe.CarbonizedResultItem);
        else if (over >= activeRecipe.BurnedDelay)
            SetResultState(ResultState.Burned, activeRecipe.BurnedResultItem);
        else if (over >= activeRecipe.SlightlyBurnedDelay)
            SetResultState(ResultState.Overcooked, activeRecipe.SlightlyBurnedResultItem);
    }

    private void SetResultState(ResultState state, ItemData data)
    {
        if (resultState == state)
            return;

        resultState = state;
        if (data != null)
        {
            contents.Clear();
            contents.Add(data);
            RefreshVisuals();
        }
    }

    private void QueueByproducts(IReadOnlyList<ItemData> byproducts)
    {
        if (byproducts == null)
            return;

        foreach (ItemData byproduct in byproducts)
        {
            if (byproduct != null)
                pendingByproducts.Add(byproduct);
        }
    }

    private bool TryTakePendingByproduct(ItemHolder holder)
    {
        if (pendingByproducts.Count == 0)
            return false;

        ItemData byproduct = pendingByproducts[0];
        if (!ObjetoDeliveryUtility.TryDeliverToHolder(byproduct, holder))
            return false;

        pendingByproducts.RemoveAt(0);
        return true;
    }

    private bool TryMovePendingByproductToPlate(PlateContainer plate)
    {
        if (pendingByproducts.Count == 0)
            return false;

        if (!plate.TryAddItem(pendingByproducts[0]))
            return false;

        pendingByproducts.RemoveAt(0);
        return true;
    }

    private void RefreshVisuals()
    {
        ClearVisuals();
        if (itemSurface == null)
            return;

        for (int i = 0; i < contents.Count; i++)
            CreateVisual(contents[i], i, contents.Count);
    }

    private void CreateVisual(ItemData data, int index, int count)
    {
        if (data == null || data.Prefab == null)
            return;

        GameObject visual = Instantiate(data.Prefab, itemSurface);
        visual.name = $"FrigideiraVisual_{data.DisplayName}";
        visual.transform.localPosition = GetVisualPosition(index, count);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * itemVisualScale;
        RecipeVisualUtility.DisableGameplayComponents(visual);
        visuals.Add(visual);
    }

    private Vector3 GetVisualPosition(int index, int count)
    {
        if (count <= 1)
            return itemLocalOffset;

        float angle = Mathf.PI * 2f * index / count;
        return itemLocalOffset + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.12f;
    }

    private void UpdateVisualMotion()
    {
        if (!IsCooking)
            return;

        for (int i = 0; i < visuals.Count; i++)
        {
            GameObject visual = visuals[i];
            if (visual == null)
                continue;

            float phase = Time.time * fryingMotionSpeed + i * 1.73f;
            Vector3 motion = new Vector3(Mathf.Sin(phase), 0f, Mathf.Cos(phase * 0.8f));
            visual.transform.localPosition = GetVisualPosition(i, visuals.Count) + motion * fryingMotionRadius;
        }
    }

    private void ClearVisuals()
    {
        for (int i = visuals.Count - 1; i >= 0; i--)
            if (visuals[i] != null)
                Destroy(visuals[i]);

        visuals.Clear();
    }

    private void UpdateSteam()
    {
        if (steamParticles == null)
            return;

        bool emit = cookingProcess != null && cookingProcess.IsRunning && cookingProcess.Timer > 0.5f;
        steamParticles.SetRate(steamRate);
        steamParticles.SetColor(resultState == ResultState.Burned || resultState == ResultState.Carbonized ? burnedSteamColor : steamColor);

        if (emit)
            steamParticles.Play();
        else
            steamParticles.Stop();
    }

    private void FinishAndClearRecipe()
    {
        cookingProcess?.Stop();
        contents.Clear();
        cookingProcess = null;
        activeRecipe = null;
        resultState = ResultState.Raw;
        lastLoggedSecond = -1;
        loggedAlmostReady = false;
    }

    private void LogTimer()
    {
        if (!showDebugLogs || cookingProcess == null)
            return;

        int second = Mathf.FloorToInt(cookingProcess.Timer);
        if (second == lastLoggedSecond)
            return;

        lastLoggedSecond = second;
        if (!cookingProcess.IsReady)
        {
            Debug.Log($"{name}: fritando... faltam {Mathf.Max(0f, cookingProcess.CookingTime - cookingProcess.Timer):0}s.");
            if (!loggedAlmostReady && cookingProcess.Progress >= 0.8f)
            {
                loggedAlmostReady = true;
                Debug.Log($"{name}: quase no ponto.");
            }
        }
    }

    private void LogContents()
    {
        if (!showDebugLogs)
            return;

        if (contents.Count == 0)
        {
            Debug.Log($"{name}: frigideira vazia.");
            return;
        }

        StringBuilder text = new StringBuilder();
        for (int i = 0; i < contents.Count; i++)
        {
            text.Append(contents[i] != null ? contents[i].DisplayName : "Dado nulo");
            if (i < contents.Count - 1)
                text.Append(", ");
        }

        Debug.Log($"{name}: {text}");
    }

    private void Log(string message)
    {
        if (showDebugLogs)
            Debug.Log(message);
    }

    private void EnsurePickupComponents()
    {
        objeto = GetComponent<Objeto>();
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        if (objeto == null)
            objeto = gameObject.AddComponent<Objeto>();

        objeto.Configure(panData, true, false);
    }

    public void OnPickedUp() => cookingEnabled = false;
    public void OnDropped() => cookingEnabled = false;
    public void OnThrown() => OnDropped();

    private void OnValidate()
    {
        maxItems = Mathf.Max(1, maxItems);
        steamRate = Mathf.Max(0f, steamRate);
    }
}
