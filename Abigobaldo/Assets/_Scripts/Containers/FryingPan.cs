using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class FryingPan : MonoBehaviour, IRecipeStation, HoldStateReceiver, ObjectReturnStateReceiver
{
    private enum ResultState { Raw, Ready, Overcooked, Burned, Carbonized }

    [Header("Recipes")]
    [SerializeField] private RecipeDatabase recipeDatabase;
    [SerializeField] private List<RecipeData> localRecipes = new List<RecipeData>();
    [SerializeField] private int maxObjects = 3;
    [SerializeField] private ObjectData carbonizedObject;

    [Header("Holdable Object")]
    [SerializeField] private bool canBePickedUp = true;
    [SerializeField] private ObjectData fryingPanData;
    [SerializeField] private bool createReturnPoint = true;
    [SerializeField] private Vector3 returnPointSize = Vector3.one * 0.7f;

    [Header("Pan Visuals")]
    [SerializeField] private Transform objectSurface;
    [SerializeField] private Vector3 objectLocalOffset = new Vector3(0f, 0.08f, 0f);
    [SerializeField] private float objectVisualScale = 0.22f;
    [SerializeField] private float fryingMotionRadius = 0.025f;
    [SerializeField] private float fryingMotionSpeed = 18f;
    [SerializeField] private bool usePhysicalObjectVisual = true;
    [SerializeField] private float visualDropHeight = 0.18f;
    [SerializeField] private float visualMass = 0.08f;
    [SerializeField] private float visualDrag = 0.05f;
    [SerializeField] private PhysicMaterial lowFrictionMaterial;
    [Header("Particles")]
    [SerializeField] private ParticleEmitterController steamParticles;
    [SerializeField] private Color steamColor = new Color(0.85f, 0.85f, 0.85f, 0.45f);
    [SerializeField] private Color burnedSteamColor = new Color(0.25f, 0.22f, 0.2f, 0.6f);
    [SerializeField] private float steamRate = 8f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private readonly List<ObjectData> contents = new List<ObjectData>();
    private readonly List<GameObject> visuals = new List<GameObject>();
    private CookingProcess cookingProcess;
    private RecipeData activeRecipe;
    private ResultState resultState;
    private readonly List<ObjectData> pendingByproducts = new List<ObjectData>();
    private int lastLoggedSecond = -1;
    private bool loggedAlmostReady;
    private bool cookingEnabled = true;
    private Rigidbody rb;
    private HoldableObject holdableObject;

    public bool HasReadyOutput => cookingProcess != null && cookingProcess.IsReady && contents.Count == 1;
    private bool IsCooking => cookingProcess != null && cookingProcess.IsRunning && !cookingProcess.IsReady;

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
        UpdateVisualMotion();
        UpdateSteam();
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.Holder == null)
            return;

        if (player.Holder.IsEmpty())
        {
            if (TryTakeOutput(player.Holder) || TryTakePendingByproduct(player.Holder))
                return;

            LogContents();
            return;
        }

        TryStoreHeldObject(player.Holder);
    }

    public bool TryPickUpContainer(Holder holder)
    {
        if (!canBePickedUp || holder == null || !holder.IsEmpty())
            return false;

        EnsurePickupComponents();
        return holder.TryPickUp(holdableObject);
    }

    public bool TryMoveOutputToPlate(PlateContainer plate)
    {
        if (plate == null)
            return false;

        if (cookingProcess != null && cookingProcess.IsReady && contents.Count == 1)
        {
            if (!plate.TryAddObject(GetCurrentOutputObject(), null, GetCurrentOutputMaterial()))
                return false;

            FinishAndClearRecipe();
            RefreshVisuals();
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

        contents.Add(removed.Data);
        Destroy(removed.gameObject);
        RefreshVisuals();
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

        Log($"{name}: frying recipe started. Time: {recipe.CookingTime:0}s.");
    }

    private bool TryFindRecipe(out RecipeData recipe)
    {
        foreach (RecipeData localRecipe in localRecipes)
        {
            if (localRecipe != null && localRecipe.Matches(ContainerType.FryingPan, contents))
            {
                recipe = localRecipe;
                return true;
            }
        }

        if (recipeDatabase != null)
            return recipeDatabase.TryFindRecipe(ContainerType.FryingPan, contents, out recipe);

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
        if (activeRecipe.ResultObject != null)
            contents.Add(activeRecipe.ResultObject);

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

        if (state == ResultState.Carbonized && carbonizedObject != null)
        {
            contents.Clear();
            contents.Add(carbonizedObject);
            RefreshVisuals();
            return;
        }

        ApplyCookMaterialForCurrentState();
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

    private void RefreshVisuals()
    {
        ClearVisuals();
        if (objectSurface == null)
            return;

        for (int i = 0; i < contents.Count; i++)
            CreateVisual(contents[i], i, contents.Count);
    }

    private void CreateVisual(ObjectData data, int index, int count)
    {
        if (data == null || data.Prefab == null)
            return;

        GameObject visual = Instantiate(data.Prefab, objectSurface);
        visual.name = $"FryingPanVisual_{data.DisplayName}";
        visual.transform.localPosition = GetVisualPosition(index, count) + Vector3.up * (usePhysicalObjectVisual ? visualDropHeight : 0f);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * objectVisualScale;
        RecipeVisualUtility.DisableGameplayComponents(visual, !usePhysicalObjectVisual);
        ConfigureVisualPhysics(visual);
        visuals.Add(visual);
        ApplyCookMaterialForCurrentState();
    }

    private void ConfigureVisualPhysics(GameObject visual)
    {
        if (!usePhysicalObjectVisual)
            return;

        Rigidbody body = visual.GetComponent<Rigidbody>();
        if (body == null)
            body = visual.AddComponent<Rigidbody>();

        body.mass = visualMass;
        body.drag = visualDrag;
        body.angularDrag = visualDrag;
        body.useGravity = true;
        body.isKinematic = false;
        body.detectCollisions = true;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        body.interpolation = RigidbodyInterpolation.Interpolate;

        if (lowFrictionMaterial == null)
            return;

        foreach (Collider targetCollider in visual.GetComponentsInChildren<Collider>())
            targetCollider.material = lowFrictionMaterial;
    }

    private Vector3 GetVisualPosition(int index, int count)
    {
        if (count <= 1)
            return objectLocalOffset;

        float angle = Mathf.PI * 2f * index / count;
        return objectLocalOffset + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.12f;
    }

    private void UpdateVisualMotion()
    {
        if (!IsCooking)
            return;

        if (usePhysicalObjectVisual)
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

    private void ApplyCookMaterialForCurrentState()
    {
        Material material = GetCurrentOutputMaterial();

        if (material == null)
            return;

        foreach (GameObject visual in visuals)
        {
            if (visual == null)
                continue;

            foreach (Renderer targetRenderer in visual.GetComponentsInChildren<Renderer>())
                targetRenderer.material = material;
        }
    }

    private ObjectData GetCurrentOutputObject()
    {
        if (resultState == ResultState.Carbonized && carbonizedObject != null)
            return carbonizedObject;

        return contents.Count > 0 ? contents[0] : null;
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
            Debug.Log($"{name}: frying... {Mathf.Max(0f, cookingProcess.CookingTime - cookingProcess.Timer):0}s left.");
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
            Debug.Log($"{name}: frying pan is empty.");
            return;
        }

        StringBuilder text = new StringBuilder();
        for (int i = 0; i < contents.Count; i++)
        {
            text.Append(contents[i] != null ? contents[i].DisplayName : "Null data");
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

        holdableObject.Configure(fryingPanData, true, false);
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
        visualDropHeight = Mathf.Max(0f, visualDropHeight);
        visualMass = Mathf.Max(0.001f, visualMass);
        visualDrag = Mathf.Max(0f, visualDrag);
        returnPointSize.x = Mathf.Max(0.05f, returnPointSize.x);
        returnPointSize.y = Mathf.Max(0.05f, returnPointSize.y);
        returnPointSize.z = Mathf.Max(0.05f, returnPointSize.z);
    }
}

