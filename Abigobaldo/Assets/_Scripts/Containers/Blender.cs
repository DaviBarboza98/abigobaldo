using System.Collections.Generic;
using UnityEngine;

public class Blender : RecipeContainer
{
    [Header("Visual do liquidificador")]
    [SerializeField] private Transform cupContentRoot;
    [SerializeField] private Vector3 contentLocalOffset;
    [SerializeField] private float ingredientVisualScale = 0.18f;
    [SerializeField] private float blendedVisualScale = 0.05f;
    [SerializeField] private float spinSpeed = 720f;
    [SerializeField] private float shakeRadius = 0.035f;
    [SerializeField] private float morphStartTime = 2f;
    [SerializeField] private float morphDuration = 3f;

    private readonly List<GameObject> visuals = new List<GameObject>();
    private bool morphedToResult;

    protected override void Awake()
    {
        ConfigureContainerType(ContainerType.Liquidificador);
        base.Awake();
    }

    protected override void RefreshVisuals()
    {
        ClearVisuals();
        morphedToResult = false;

        if (cupContentRoot == null)
            return;

        for (int i = 0; i < CurrentContents.Count; i++)
            CreateVisual(CurrentContents[i], i, CurrentContents.Count, ingredientVisualScale);
    }

    protected override void UpdateSpecificVisuals()
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
        if (morphedToResult || CurrentRecipe == null || CurrentCookingProcess == null)
            return;

        if (CurrentCookingProcess.Timer < morphStartTime)
            return;

        float transition = morphDuration <= 0f
            ? 1f
            : Mathf.InverseLerp(morphStartTime, morphStartTime + morphDuration, CurrentCookingProcess.Timer);

        float visualScale = Mathf.Lerp(ingredientVisualScale, blendedVisualScale, transition);

        foreach (GameObject visual in visuals)
        {
            if (visual != null)
                visual.transform.localScale = Vector3.one * visualScale;
        }

        if (transition < 1f)
            return;

        morphedToResult = true;
        ClearVisuals();
        CreateVisual(CurrentRecipe.ResultItem, 0, 1, blendedVisualScale);
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
        DisableGameplayComponents(visual);
        visuals.Add(visual);
    }

    private Vector3 GetVisualPosition(int index, int count)
    {
        if (count <= 1)
            return contentLocalOffset;

        float angle = Mathf.PI * 2f * index / count;
        Vector3 radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        return contentLocalOffset + radial * 0.08f;
    }

    private void ClearVisuals()
    {
        for (int i = visuals.Count - 1; i >= 0; i--)
        {
            if (visuals[i] != null)
                Destroy(visuals[i]);
        }

        visuals.Clear();
    }
}
