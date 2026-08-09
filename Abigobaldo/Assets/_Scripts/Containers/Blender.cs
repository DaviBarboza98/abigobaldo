using System.Collections.Generic;
using UnityEngine;

public class Blender : MonoBehaviour, IRecipeStation
{
    [Header("Receitas")]
    [SerializeField] private RecipeDatabase recipeDatabase;
    [SerializeField] private List<RecipeData> localRecipes = new List<RecipeData>();

    [Header("Copo")]
    [SerializeField] private Transform cupAnchor;
    [SerializeField] private BlenderCup startingCup;

    [Header("Mistura")]
    [SerializeField] private float spinSpeed = 720f;
    [SerializeField] private float shakeRadius = 0.035f;
    [SerializeField] private float morphStartTime = 2f;
    [SerializeField] private float morphDuration = 3f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private BlenderCup currentCup;
    private CookingProcess cookingProcess;
    private RecipeData activeRecipe;
    private int lastLoggedSecond = -1;
    private bool powered;
    private bool morphedToResult;
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
        if (startingCup != null)
            AttachCup(startingCup);
    }

    private void Update()
    {
        UpdateCooking();

        if (currentCup != null)
            currentCup.UpdateBlendVisuals(IsCooking, spinSpeed, shakeRadius, morphStartTime, morphDuration, cookingProcess, activeRecipe, ref morphedToResult);
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.ItemHolder == null)
            return;

        ItemHolder holder = player.ItemHolder;

        if (!holder.IsEmpty())
        {
            BlenderCup heldCup = holder.CurrentObjeto != null
                ? holder.CurrentObjeto.GetComponent<BlenderCup>()
                : null;

            if (heldCup != null)
            {
                AttachHeldCup(holder, heldCup);
                return;
            }
        }

        TogglePower();
    }

    public bool TryPickUpContainer(ItemHolder holder)
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

    public bool TryTakeOutput(ItemHolder holder)
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

    private void AttachHeldCup(ItemHolder holder, BlenderCup cup)
    {
        if (currentCup != null || holder == null || cup == null)
            return;

        holder.RemoveObjeto();
        AttachCup(cup);
    }

    private void AttachCup(BlenderCup cup)
    {
        if (currentCup != null || cup == null)
            return;

        currentCup = cup;
        currentCup.AttachTo(this, cupAnchor);
        Log($"{name}: copo encaixado.");

        if (powered)
            TryStartRecipe();
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
            Log($"{name}: encaixe o copo antes de ligar.");
            return;
        }

        powered = enabled;

        if (cookingProcess == null)
        {
            if (powered)
                TryStartRecipe();

            Log(powered ? $"{name}: ligado." : $"{name}: desligado.");
            return;
        }

        if (powered)
            cookingProcess.Resume();
        else
            cookingProcess.Pause();

        Log(powered ? $"{name}: ligado." : $"{name}: desligado.");
    }

    private void TryStartRecipe()
    {
        if (!powered || currentCup == null || cookingProcess != null)
            return;

        if (!TryFindRecipe(out RecipeData recipe))
        {
            Log($"{name}: ingredientes nao formam uma receita.");
            return;
        }

        activeRecipe = recipe;
        cookingProcess = new CookingProcess(recipe.CookingTime);
        lastLoggedSecond = -1;
        morphedToResult = false;
        outputPrepared = false;
        cookingProcess.Start();

        Log($"{name}: receita iniciada. Tempo: {recipe.CookingTime:0}s.");
    }

    private bool TryFindRecipe(out RecipeData recipe)
    {
        IReadOnlyList<ItemData> contents = currentCup != null
            ? currentCup.Contents
            : null;

        foreach (RecipeData localRecipe in localRecipes)
        {
            if (localRecipe != null && localRecipe.Matches(ContainerType.Liquidificador, contents))
            {
                recipe = localRecipe;
                return true;
            }
        }

        if (recipeDatabase != null)
            return recipeDatabase.TryFindRecipe(ContainerType.Liquidificador, contents, out recipe);

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

        currentCup.ReplaceContentsWithResult(activeRecipe.ResultItem);
        outputPrepared = true;
        SetPower(false);
        Log($"{name}: receita pronta.");
    }

    private void FinishAndClearRecipe(bool clearCupOutput = true)
    {
        cookingProcess?.Stop();
        cookingProcess = null;
        activeRecipe = null;
        lastLoggedSecond = -1;
        morphedToResult = false;
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
            Debug.Log($"{name}: batendo... faltam {Mathf.Max(0f, cookingProcess.CookingTime - cookingProcess.Timer):0}s.");
    }

    private void Log(string message)
    {
        if (showDebugLogs)
            Debug.Log(message);
    }

    private void OnValidate()
    {
        morphStartTime = Mathf.Max(0f, morphStartTime);
        morphDuration = Mathf.Max(0f, morphDuration);
    }
}
