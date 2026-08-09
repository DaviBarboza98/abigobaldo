using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Blender : MonoBehaviour, IRecipeStation
{
    [Header("Receitas")]
    [SerializeField] private RecipeDatabase recipeDatabase;
    [SerializeField] private List<RecipeData> localRecipes = new List<RecipeData>();
    [SerializeField] private int maxItems = 5;

    [Header("Copo")]
    [SerializeField] private BlenderCupSlot requiredCupSlot;

    [Header("Visual do liquidificador")]
    [SerializeField] private Transform cupContentRoot;
    [SerializeField] private Vector3 contentLocalOffset;
    [SerializeField] private float ingredientVisualScale = 0.18f;
    [SerializeField] private float blendedVisualScale = 0.05f;
    [SerializeField] private float spinSpeed = 720f;
    [SerializeField] private float shakeRadius = 0.035f;
    [SerializeField] private float morphStartTime = 2f;
    [SerializeField] private float morphDuration = 3f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private readonly List<ItemData> contents = new List<ItemData>();
    private readonly List<GameObject> visuals = new List<GameObject>();
    private CookingProcess cookingProcess;
    private RecipeData activeRecipe;
    private int lastLoggedSecond = -1;
    private bool powered;
    private bool morphedToResult;

    public bool HasReadyOutput => cookingProcess != null && cookingProcess.IsReady && contents.Count == 1;
    private bool IsCooking => cookingProcess != null && cookingProcess.IsRunning && !cookingProcess.IsReady;

    private void Awake()
    {
        GameLayers.SetLayerRecursivelyIfDefault(gameObject, GameLayers.Container);

        if (requiredCupSlot == null)
            requiredCupSlot = GetComponentInChildren<BlenderCupSlot>();

        if (requiredCupSlot != null)
            requiredCupSlot.SetBlender(this);
    }

    private void Update()
    {
        UpdateCooking();
        UpdateVisuals();
    }

    public void Interact(PlayerInteraction player)
    {
        if (player == null || player.ItemHolder == null)
            return;

        if (player.ItemHolder.IsEmpty())
        {
            if (TryTakeOutput(player.ItemHolder))
                return;

            TogglePower();
            return;
        }

        TryStoreHeldObject(player.ItemHolder);
    }

    public bool TryPickUpContainer(ItemHolder holder)
    {
        return false;
    }

    public bool TryMoveOutputToPlate(PlateContainer plate)
    {
        if (plate == null || cookingProcess == null || !cookingProcess.IsReady || contents.Count != 1)
            return false;

        if (!plate.TryAddItem(contents[0]))
            return false;

        FinishAndClear();
        RefreshVisuals();
        return true;
    }

    public void SetRequiredCupSlot(BlenderCupSlot slot)
    {
        requiredCupSlot = slot;
    }

    public void NotifyRequiredCupChanged()
    {
        if (requiredCupSlot == null || !requiredCupSlot.IsOccupied)
        {
            SetPower(false);
            return;
        }

        if (powered)
            TryStartRecipe();
    }

    private void TogglePower()
    {
        SetPower(!powered);
    }

    private void SetPower(bool enabled)
    {
        if (enabled && (requiredCupSlot == null || !requiredCupSlot.IsOccupied))
        {
            Log($"{name}: encaixe o copo antes de ligar.");
            powered = false;
            return;
        }

        powered = enabled;

        if (cookingProcess == null)
        {
            if (powered)
                TryStartRecipe();
            return;
        }

        if (powered)
            cookingProcess.Resume();
        else
            cookingProcess.Pause();

        Log(powered ? $"{name}: ligado." : $"{name}: desligado.");
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

        if (powered)
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

        FinishAndClear();
        RefreshVisuals();
        return true;
    }

    private void TryStartRecipe()
    {
        if (cookingProcess != null || !TryFindRecipe(out RecipeData recipe))
            return;

        activeRecipe = recipe;
        cookingProcess = new CookingProcess(recipe.CookingTime);
        lastLoggedSecond = -1;
        morphedToResult = false;

        if (powered)
            cookingProcess.Start();

        Log($"{name}: receita iniciada no liquidificador. Tempo: {recipe.CookingTime:0}s.");
    }

    private bool TryFindRecipe(out RecipeData recipe)
    {
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
        if (cookingProcess == null || !cookingProcess.IsRunning)
            return;

        cookingProcess.Update(Time.deltaTime);
        LogTimer();

        if (cookingProcess.IsReady)
            PrepareOutput();
    }

    private void PrepareOutput()
    {
        if (activeRecipe == null || contents.Count == 1 && contents[0] == activeRecipe.ResultItem)
            return;

        contents.Clear();
        if (activeRecipe.ResultItem != null)
            contents.Add(activeRecipe.ResultItem);

        RefreshVisuals();
    }

    private void RefreshVisuals()
    {
        ClearVisuals();
        morphedToResult = false;

        if (cupContentRoot == null)
            return;

        for (int i = 0; i < contents.Count; i++)
            CreateVisual(contents[i], i, contents.Count, ingredientVisualScale);
    }

    private void UpdateVisuals()
    {
        if (!IsCooking || cupContentRoot == null)
            return;

        for (int i = 0; i < visuals.Count; i++)
        {
            GameObject visual = visuals[i];
            if (visual == null)
                continue;

            float phase = Time.time * 18f + i;
            Vector3 shake = new Vector3(Mathf.Sin(phase), Mathf.Cos(phase * 1.3f), Mathf.Sin(phase * 0.7f));
            visual.transform.localPosition = GetVisualPosition(i, visuals.Count) + shake * shakeRadius;
            visual.transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
        }

        TryMorphToResult();
    }

    private void TryMorphToResult()
    {
        if (morphedToResult || activeRecipe == null || cookingProcess == null || cookingProcess.Timer < morphStartTime)
            return;

        float transition = morphDuration <= 0f
            ? 1f
            : Mathf.InverseLerp(morphStartTime, morphStartTime + morphDuration, cookingProcess.Timer);

        float visualScale = Mathf.Lerp(ingredientVisualScale, blendedVisualScale, transition);
        foreach (GameObject visual in visuals)
            if (visual != null)
                visual.transform.localScale = Vector3.one * visualScale;

        if (transition < 1f)
            return;

        morphedToResult = true;
        ClearVisuals();
        CreateVisual(activeRecipe.ResultItem, 0, 1, blendedVisualScale);
    }

    private void CreateVisual(ItemData data, int index, int count, float scale)
    {
        if (data == null || data.Prefab == null)
            return;

        GameObject visual = Instantiate(data.Prefab, cupContentRoot);
        visual.name = $"BlenderVisual_{data.DisplayName}";
        visual.transform.localPosition = GetVisualPosition(index, count);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * scale;
        RecipeVisualUtility.DisableGameplayComponents(visual);
        visuals.Add(visual);
    }

    private Vector3 GetVisualPosition(int index, int count)
    {
        if (count <= 1)
            return contentLocalOffset;

        float angle = Mathf.PI * 2f * index / count;
        return contentLocalOffset + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 0.08f;
    }

    private void ClearVisuals()
    {
        for (int i = visuals.Count - 1; i >= 0; i--)
            if (visuals[i] != null)
                Destroy(visuals[i]);

        visuals.Clear();
    }

    private void FinishAndClear()
    {
        cookingProcess?.Stop();
        contents.Clear();
        cookingProcess = null;
        activeRecipe = null;
        lastLoggedSecond = -1;
        morphedToResult = false;
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
        maxItems = Mathf.Max(1, maxItems);
        morphStartTime = Mathf.Max(0f, morphStartTime);
        morphDuration = Mathf.Max(0f, morphDuration);
    }
}
