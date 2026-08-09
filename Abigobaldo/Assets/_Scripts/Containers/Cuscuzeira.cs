using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Cuscuzeira : MonoBehaviour, IRecipeStation, ItemHoldStateReceiver, ObjetoReturnStateReceiver
{
    private enum ResultState { Raw, Ready, Overcooked, Burned, Carbonized }

    [Header("Receitas")]
    [SerializeField] private RecipeDatabase recipeDatabase;
    [SerializeField] private List<RecipeData> localRecipes = new List<RecipeData>();
    [SerializeField] private int maxItems = 3;
    [SerializeField] private ItemData carbonizedItem;

    [Header("Objeto pegavel")]
    [SerializeField] private bool canBePickedUp = true;
    [SerializeField] private ItemData cuscuzeiraData;
    [SerializeField] private bool createReturnPoint = true;
    [SerializeField] private Vector3 returnPointSize = Vector3.one * 0.7f;

    [Header("Particulas")]
    [SerializeField] private ParticleEmitterController steamParticles;
    [SerializeField] private Color steamColor = new Color(0.9f, 0.9f, 0.86f, 0.55f);
    [SerializeField] private Color burnedSteamColor = new Color(0.22f, 0.2f, 0.18f, 0.75f);
    [SerializeField] private float steamRate = 20f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private readonly List<ItemData> contents = new List<ItemData>();
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

    private void Awake()
    {
        GameLayers.SetLayerRecursivelyIfDefault(gameObject, GameLayers.Container);
        if (canBePickedUp)
        {
            EnsurePickupComponents();
            CreateReturnPoint();
        }
    }

    private void Update()
    {
        UpdateCooking();
        UpdateSteam();
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.ItemHolder == null)
            return;

        if (player.ItemHolder.IsEmpty())
        {
            if (!TryTakeOutput(player.ItemHolder) && !TryTakePendingByproduct(player.ItemHolder))
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
            if (!plate.TryAddItem(GetCurrentOutputItem(), null, GetCurrentOutputMaterial()))
                return false;

            FinishAndClearRecipe();
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
        TryStartRecipe();
        return true;
    }

    private bool TryTakeOutput(ItemHolder holder)
    {
        if (holder == null || !holder.IsEmpty() || cookingProcess == null || !cookingProcess.IsReady || contents.Count != 1)
            return false;

        ItemData output = GetCurrentOutputItem();
        if (output == null || output.Prefab == null)
            return false;

        if (!ObjetoDeliveryUtility.TryDeliverToHolder(output, holder, GetCurrentCookState(), null, GetCurrentOutputMaterial()))
            return false;

        FinishAndClearRecipe();
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

        Log($"{name}: receita iniciada na cuscuzeira. Tempo: {recipe.CookingTime:0}s.");
    }

    private bool TryFindRecipe(out RecipeData recipe)
    {
        foreach (RecipeData localRecipe in localRecipes)
        {
            if (localRecipe != null && localRecipe.Matches(ContainerType.Cuscuzeira, contents))
            {
                recipe = localRecipe;
                return true;
            }
        }

        if (recipeDatabase != null)
            return recipeDatabase.TryFindRecipe(ContainerType.Cuscuzeira, contents, out recipe);

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
    }

    private void UpdateOvercook()
    {
        if (activeRecipe == null || cookingProcess == null || !cookingProcess.IsReady || !activeRecipe.CanOvercook)
            return;

        float over = cookingProcess.OvercookTime;
        if (over >= activeRecipe.CarbonizedDelay)
            SetResultState(ResultState.Carbonized);
        else if (over >= activeRecipe.BurnedDelay)
            SetResultState(ResultState.Burned);
        else if (over >= activeRecipe.SlightlyBurnedDelay)
            SetResultState(ResultState.Overcooked);
    }

    private void SetResultState(ResultState state)
    {
        if (resultState == state)
            return;

        resultState = state;

        if (state == ResultState.Carbonized && carbonizedItem != null)
        {
            contents.Clear();
            contents.Add(carbonizedItem);
        }
    }

    private ItemData GetCurrentOutputItem()
    {
        if (resultState == ResultState.Carbonized && carbonizedItem != null)
            return carbonizedItem;

        return contents.Count > 0 ? contents[0] : null;
    }

    private ItemCookState GetCurrentCookState()
    {
        return resultState switch
        {
            ResultState.Ready => ItemCookState.AoPonto,
            ResultState.Overcooked => ItemCookState.Passado,
            ResultState.Burned => ItemCookState.Queimado,
            ResultState.Carbonized => ItemCookState.Carbonizado,
            _ => ItemCookState.Cru
        };
    }

    private Material GetCurrentOutputMaterial()
    {
        if (activeRecipe == null)
            return null;

        return resultState switch
        {
            ResultState.Overcooked => activeRecipe.OvercookedMaterial,
            ResultState.Burned => activeRecipe.BurnedMaterial,
            ResultState.Carbonized => activeRecipe.CarbonizedMaterial,
            _ => null
        };
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
            Debug.Log($"{name}: cozinhando... faltam {Mathf.Max(0f, cookingProcess.CookingTime - cookingProcess.Timer):0}s.");
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
            Debug.Log($"{name}: cuscuzeira vazia.");
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

        objeto.Configure(cuscuzeiraData, true, false);
    }

    public void OnPickedUp() => SetCookingEnabled(false);
    public void OnDropped() => cookingEnabled = false;
    public void OnThrown() => OnDropped();
    public void OnReturnedToOrigin() => SetCookingEnabled(true);

    private void SetCookingEnabled(bool enabled)
    {
        cookingEnabled = enabled;

        if (cookingProcess == null)
        {
            if (enabled)
                TryStartRecipe();

            return;
        }

        if (enabled)
            cookingProcess.Resume();
        else
            cookingProcess.Pause();
    }

    private void CreateReturnPoint()
    {
        if (!createReturnPoint || objeto == null)
            return;

        GameObject point = new GameObject($"{name}_ReturnPoint");
        point.transform.SetPositionAndRotation(transform.position, transform.rotation);

        ObjetoReturnPoint returnPoint = point.AddComponent<ObjetoReturnPoint>();
        returnPoint.Initialize(objeto, returnPointSize);
    }

    private void OnValidate()
    {
        maxItems = Mathf.Max(1, maxItems);
        steamRate = Mathf.Max(0f, steamRate);
        returnPointSize.x = Mathf.Max(0.05f, returnPointSize.x);
        returnPointSize.y = Mathf.Max(0.05f, returnPointSize.y);
        returnPointSize.z = Mathf.Max(0.05f, returnPointSize.z);
    }
}
