using System.Collections.Generic;
using UnityEngine;

public class Blender : MonoBehaviour, IRecipeStation
{
    [Header("Recipes")]
    [SerializeField] private RecipeDatabase recipeDatabase;
    [SerializeField] private List<RecipeData> localRecipes = new List<RecipeData>();

    [Header("Cup")]
    [SerializeField] private Transform cupAnchor;
    [SerializeField] private BlenderCup startingCup;
    [SerializeField] private bool autoAttachNearbyCup = true;
    [SerializeField] private float autoAttachRadius = 1.25f;

    [Header("Mixing")]
    [SerializeField] private float spinSpeed = 720f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private BlenderCup currentCup;
    private CookingProcess cookingProcess;
    private RecipeData activeRecipe;
    private int lastLoggedSecond = -1;
    private bool powered;
    private bool outputPrepared;

    public bool HasReadyOutput => currentCup != null && cookingProcess != null && cookingProcess.IsReady && outputPrepared && currentCup.HasSingleOutput;
    private bool IsCooking => currentCup != null && cookingProcess != null && cookingProcess.IsRunning && !cookingProcess.IsReady;

    private void Awake()
    {
        GameLayers.SetLayerRecursivelyIfDefault(gameObject, GameLayers.Container);

        if (cupAnchor == null)
            cupAnchor = transform;
    }

    private void Start()
    {
        BlenderCup initialCup = startingCup != null
            ? startingCup
            : FindInitialCup();

        if (initialCup != null)
            AttachCup(initialCup);
    }

    private void Update()
    {
        UpdateCooking();

        if (currentCup != null)
            currentCup.UpdateBlendVisuals(IsCooking, spinSpeed);
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.Holder == null)
            return;

        Holder holder = player.Holder;

        if (!holder.IsEmpty())
        {
            BlenderCup heldCup = holder.CurrentObject != null
                ? holder.CurrentObject.GetComponent<BlenderCup>()
                : null;

            if (heldCup != null)
            {
                AttachHeldCup(holder, heldCup);
                return;
            }
        }

        TogglePower();
    }

    public bool TryPickUpContainer(Holder holder)
    {
        return false;
    }

    public bool TryMoveOutputToPlate(PlateContainer plate)
    {
        if (currentCup == null || plate == null)
            return false;

        if (!HasReadyOutput)
            return false;

        if (!currentCup.TryMoveOutputToPlate(plate))
            return false;

        FinishAndClearRecipe();
        return true;
    }

    public bool TryTakeOutput(Holder holder)
    {
        if (currentCup == null || holder == null || !holder.IsEmpty() || !HasReadyOutput)
            return false;

        if (!currentCup.TryTakeOutput(holder))
            return false;

        FinishAndClearRecipe();
        return true;
    }

    public void NotifyCupPickedUp(BlenderCup cup)
    {
        if (cup != currentCup)
            return;

        SetPower(false);
        currentCup = null;
        FinishAndClearRecipe(false);
    }

    private void AttachHeldCup(Holder holder, BlenderCup cup)
    {
        if (currentCup != null || holder == null || cup == null)
            return;

        holder.RemoveObject();
        AttachCup(cup);
    }

    private void AttachCup(BlenderCup cup)
    {
        if (currentCup != null || cup == null)
            return;

        currentCup = cup;
        currentCup.AttachTo(this, cupAnchor);
        Log($"{name}: cup attached.");

        if (powered)
            TryStartRecipe();
    }

    private BlenderCup FindInitialCup()
    {
        BlenderCup childCup = GetComponentInChildren<BlenderCup>(true);
        if (childCup != null)
            return childCup;

        if (!autoAttachNearbyCup || cupAnchor == null)
            return null;

        BlenderCup[] cups = FindObjectsOfType<BlenderCup>(true);
        BlenderCup closestCup = null;
        float closestDistance = autoAttachRadius;

        foreach (BlenderCup cup in cups)
        {
            if (cup == null || cup.IsAttached)
                continue;

            float distance = Vector3.Distance(cup.transform.position, cupAnchor.position);
            if (distance > closestDistance)
                continue;

            closestCup = cup;
            closestDistance = distance;
        }

        return closestCup;
    }

    private void TogglePower()
    {
        SetPower(!powered);
    }

    private void SetPower(bool enabled)
    {
        if (enabled && currentCup == null)
        {
            powered = false;
            Log($"{name}: attach the cup before powering on.");
            return;
        }

        powered = enabled;

        if (cookingProcess == null)
        {
            if (powered)
                TryStartRecipe();

            Log(powered ? $"{name}: powered on." : $"{name}: powered off.");
            return;
        }

        if (powered)
            cookingProcess.Resume();
        else
            cookingProcess.Pause();

        Log(powered ? $"{name}: powered on." : $"{name}: powered off.");
    }

    private void TryStartRecipe()
    {
        if (!powered || currentCup == null || cookingProcess != null)
            return;

        if (!TryFindRecipe(out RecipeData recipe))
        {
            Log($"{name}: ingredients do not match a recipe.");
            return;
        }

        activeRecipe = recipe;
        cookingProcess = new CookingProcess(recipe.CookingTime);
        lastLoggedSecond = -1;
        outputPrepared = false;
        cookingProcess.Start();

        Log($"{name}: recipe started. Time: {recipe.CookingTime:0}s.");
    }

    private bool TryFindRecipe(out RecipeData recipe)
    {
        IReadOnlyList<ObjectData> contents = currentCup != null
            ? currentCup.Contents
            : null;

        foreach (RecipeData localRecipe in localRecipes)
        {
            if (localRecipe != null && localRecipe.Matches(ContainerType.Blender, contents))
            {
                recipe = localRecipe;
                return true;
            }
        }

        if (recipeDatabase != null)
            return recipeDatabase.TryFindRecipe(ContainerType.Blender, contents, out recipe);

        recipe = null;
        return false;
    }

    private void UpdateCooking()
    {
        if (currentCup == null || cookingProcess == null || !cookingProcess.IsRunning)
            return;

        cookingProcess.Update(Time.deltaTime);
        LogTimer();

        if (cookingProcess.IsReady)
            PrepareOutput();
    }

    private void PrepareOutput()
    {
        if (activeRecipe == null || currentCup == null || outputPrepared)
            return;

        currentCup.ReplaceContentsWithResult(activeRecipe.ResultObject);
        outputPrepared = true;
        SetPower(false);
        Log($"{name}: recipe ready.");
    }

    private void FinishAndClearRecipe(bool clearCupOutput = true)
    {
        cookingProcess?.Stop();
        cookingProcess = null;
        activeRecipe = null;
        lastLoggedSecond = -1;
        outputPrepared = false;

        if (clearCupOutput && currentCup != null)
            currentCup.ClearContents();
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
            Debug.Log($"{name}: blending... {Mathf.Max(0f, cookingProcess.CookingTime - cookingProcess.Timer):0}s left.");
    }

    private void Log(string message)
    {
        if (showDebugLogs)
            Debug.Log(message);
    }

    private void OnValidate()
    {
        spinSpeed = Mathf.Max(0f, spinSpeed);
        autoAttachRadius = Mathf.Max(0.01f, autoAttachRadius);
    }
}

