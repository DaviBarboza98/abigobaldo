using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Cuscuzeira : MonoBehaviour, IRecipeStation, HoldStateReceiver, ObjectReturnStateReceiver
{
    private enum ResultState { Raw, Ready, Overcooked, Burned, Carbonized }

    [Header("Recipes")]
    [SerializeField] private RecipeDatabase recipeDatabase;
    [SerializeField] private List<RecipeData> localRecipes = new List<RecipeData>();
    [SerializeField] private int maxObjects = 3;

    [Header("Holdable Object")]
    [SerializeField] private bool canBePickedUp = true;
    [SerializeField] private ObjectData cuscuzeiraData;
    [SerializeField] private bool createReturnPoint = true;
    [SerializeField] private Vector3 returnPointSize = Vector3.one * 0.7f;

    [Header("Particles")]
    [SerializeField] private ParticleEmitterController steamParticles;
    [SerializeField] private Color steamColor = new Color(0.9f, 0.9f, 0.86f, 0.55f);
    [SerializeField] private Color burnedSteamColor = new Color(0.22f, 0.2f, 0.18f, 0.75f);
    [SerializeField] private float steamRate = 20f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private readonly List<RuntimeObjectState> contents = new List<RuntimeObjectState>();
    private CookingProcess cookingProcess;
    private RecipeData activeRecipe;
    private ResultState resultState;
    private readonly List<ObjectData> pendingByproducts = new List<ObjectData>();
    private int lastLoggedSecond = -1;
    private bool loggedAlmostReady;
    private bool cookingEnabled = true;
    private Rigidbody rb;
    private HoldableObject holdableObject;

    public bool HasStoredObjects => contents.Count > 0 || pendingByproducts.Count > 0;
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
        if (player == null || player.Holder == null)
            return;

        if (player.Holder.IsEmpty())
        {
            if (!TryTakeLastContent(player.Holder) && !TryTakePendingByproduct(player.Holder))
                LogContents();
            return;
        }

        TryStoreHeldObject(player.Holder);
    }

    public bool TryPickUpContainer(Holder holder)
    {
        if (!canBePickedUp || HasStoredObjects || holder == null || !holder.IsEmpty())
            return false;

        EnsurePickupComponents();
        return holder.TryPickUp(holdableObject);
    }

    public bool TryMoveOutputToPlate(PlateContainer plate)
    {
        if (plate == null)
            return false;

        if (contents.Count == 1 && (cookingProcess == null || cookingProcess.IsReady))
        {
            RuntimeObjectState output = contents[0];
            if (!plate.TryAddObject(output.Data, output.CookState, null, output.Material))
                return false;

            FinishAndClearRecipe();
            return true;
        }

        return TryMovePendingByproductToPlate(plate);
    }

    private bool TryStoreHeldObject(Holder holder)
    {
        if (cookingProcess != null || contents.Count >= maxObjects)
            return false;

        HoldableObject held = holder.CurrentObject;
        if (held == null || held.Data == null)
        {
            Debug.LogWarning($"{held?.name ?? "HoldableObject"} has no recipe data.");
            return false;
        }

        HoldableObject removed = holder.RemoveObject();
        if (removed == null)
            return false;

        contents.Add(RuntimeObjectState.FromObject(removed));
        Destroy(removed.gameObject);
        TryStartRecipe();
        return true;
    }

    private bool TryTakeOutput(Holder holder)
    {
        if (holder == null || !holder.IsEmpty() || cookingProcess == null || !cookingProcess.IsReady || contents.Count != 1)
            return false;

        ObjectData output = GetCurrentOutputObject();
        if (output == null || output.Prefab == null)
            return false;

        if (!ObjectDeliveryUtility.TryDeliverToHolder(output, holder, GetCurrentCookState(), null, GetCurrentOutputMaterial()))
            return false;

        FinishAndClearRecipe();
        return true;
    }

    private bool TryTakeLastContent(Holder holder)
    {
        if (holder == null || !holder.IsEmpty() || contents.Count == 0)
            return false;

        int lastIndex = contents.Count - 1;
        RuntimeObjectState storedObject = contents[lastIndex];
        ObjectData output = storedObject.Data;
        ObjectCookState cookState = storedObject.CookState;
        Material outputMaterial = storedObject.Material;

        if (output == null || output.Prefab == null)
            return false;

        if (!ObjectDeliveryUtility.TryDeliverToHolder(output, holder, cookState, null, outputMaterial))
            return false;

        contents.RemoveAt(lastIndex);
        CancelActiveRecipe();
        LogContents();
        return true;
    }

    private void TryStartRecipe()
    {
        if (cookingProcess != null || HasCookedContents() || !TryFindRecipe(out RecipeData recipe))
            return;

        activeRecipe = recipe;
        cookingProcess = new CookingProcess(recipe.CookingTime);
        resultState = ResultState.Raw;
        lastLoggedSecond = -1;
        loggedAlmostReady = false;

        if (recipe.SpawnByproductsOnStart)
            QueueByproducts(recipe.Byproducts);

        ReplaceContentsWithCurrentRecipeObject();

        if (cookingEnabled)
            cookingProcess.Start();

        Log($"{name}: cuscuzeira recipe started. Time: {recipe.CookingTime:0}s.");
    }

    private bool TryFindRecipe(out RecipeData recipe)
    {
        foreach (RecipeData localRecipe in localRecipes)
        {
            if (localRecipe != null && localRecipe.Matches(ContainerType.Cuscuzeira, GetContentData()))
            {
                recipe = localRecipe;
                return true;
            }
        }

        if (recipeDatabase != null)
            return recipeDatabase.TryFindRecipe(ContainerType.Cuscuzeira, GetContentData(), out recipe);

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
        if (!activeRecipe.SpawnByproductsOnStart)
            QueueByproducts(activeRecipe.Byproducts);

        SetResultState(ResultState.Ready);
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

        ReplaceContentsWithCurrentRecipeObject();
    }

    private ObjectData GetCurrentOutputObject()
    {
        if (activeRecipe != null)
            return activeRecipe.GetObjectForCookState(GetCurrentCookState());

        return contents.Count > 0 ? contents[0].Data : null;
    }

    private List<ObjectData> GetContentData()
    {
        List<ObjectData> data = new List<ObjectData>(contents.Count);

        foreach (RuntimeObjectState content in contents)
            data.Add(content.Data);

        return data;
    }

    private bool HasCookedContents()
    {
        foreach (RuntimeObjectState content in contents)
        {
            if (content.CookState != ObjectCookState.Raw)
                return true;
        }

        return false;
    }

    private void ReplaceContentsWithCurrentRecipeObject()
    {
        if (activeRecipe == null)
            return;

        ObjectData stateObject = GetCurrentOutputObject();
        if (stateObject == null)
            return;

        contents.Clear();
        contents.Add(new RuntimeObjectState(stateObject, GetCurrentCookState(), GetCurrentOutputMaterial()));
    }

    private ObjectCookState GetCurrentCookState()
    {
        return resultState switch
        {
            ResultState.Ready => ObjectCookState.Ready,
            ResultState.Overcooked => ObjectCookState.Overcooked,
            ResultState.Burned => ObjectCookState.Burned,
            ResultState.Carbonized => ObjectCookState.Carbonized,
            _ => ObjectCookState.Raw
        };
    }

    private Material GetCurrentOutputMaterial()
    {
        if (activeRecipe == null)
            return null;

        return resultState switch
        {
            ResultState.Ready => activeRecipe.ReadyMaterial,
            ResultState.Overcooked => activeRecipe.OvercookedMaterial,
            ResultState.Burned => activeRecipe.BurnedMaterial,
            ResultState.Carbonized => activeRecipe.CarbonizedMaterial,
            _ => null
        };
    }

    private void QueueByproducts(IReadOnlyList<ObjectData> byproducts)
    {
        if (byproducts == null)
            return;

        foreach (ObjectData byproduct in byproducts)
        {
            if (byproduct != null)
                pendingByproducts.Add(byproduct);
        }
    }

    private bool TryTakePendingByproduct(Holder holder)
    {
        if (pendingByproducts.Count == 0)
            return false;

        ObjectData byproduct = pendingByproducts[0];
        if (!ObjectDeliveryUtility.TryDeliverToHolder(byproduct, holder))
            return false;

        pendingByproducts.RemoveAt(0);
        return true;
    }

    private bool TryMovePendingByproductToPlate(PlateContainer plate)
    {
        if (pendingByproducts.Count == 0)
            return false;

        if (!plate.TryAddObject(pendingByproducts[0]))
            return false;

        pendingByproducts.RemoveAt(0);
        return true;
    }

    private void UpdateSteam()
    {
        if (steamParticles == null)
            return;

        bool emit = cookingProcess != null && cookingProcess.IsRunning;
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

    private void CancelActiveRecipe()
    {
        cookingProcess?.Stop();
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
            Debug.Log($"{name}: cooking... {Mathf.Max(0f, cookingProcess.CookingTime - cookingProcess.Timer):0}s left.");
            if (!loggedAlmostReady && cookingProcess.Progress >= 0.8f)
            {
                loggedAlmostReady = true;
                Debug.Log($"{name}: almost ready.");
            }
        }
    }

    private void LogContents()
    {
        if (!showDebugLogs)
            return;

        if (contents.Count == 0)
        {
            Debug.Log($"{name}: cuscuzeira is empty.");
            return;
        }

        StringBuilder text = new StringBuilder();
        for (int i = 0; i < contents.Count; i++)
        {
            ObjectData data = contents[i].Data;
            text.Append(data != null ? $"{data.DisplayName} ({contents[i].CookState})" : "Null data");
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
        holdableObject = GetComponent<HoldableObject>();
        rb = GetComponent<Rigidbody>();

        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();

        if (holdableObject == null)
            holdableObject = gameObject.AddComponent<HoldableObject>();

        holdableObject.Configure(cuscuzeiraData, true, false);
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
        if (!createReturnPoint || holdableObject == null)
            return;

        GameObject point = new GameObject($"{name}_ReturnPoint");
        point.transform.SetPositionAndRotation(transform.position, transform.rotation);

        ObjectReturnPoint returnPoint = point.AddComponent<ObjectReturnPoint>();
        returnPoint.Initialize(holdableObject, returnPointSize);
    }

    private void OnValidate()
    {
        maxObjects = Mathf.Max(1, maxObjects);
        steamRate = Mathf.Max(0f, steamRate);
        returnPointSize.x = Mathf.Max(0.05f, returnPointSize.x);
        returnPointSize.y = Mathf.Max(0.05f, returnPointSize.y);
        returnPointSize.z = Mathf.Max(0.05f, returnPointSize.z);
    }
}


