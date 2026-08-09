using System.Collections.Generic;
using UnityEngine;

public class FryingPan : RecipeContainer
{
    [Header("Visual da frigideira")]
    [SerializeField] private Transform itemSurface;
    [SerializeField] private Vector3 itemLocalOffset = new Vector3(0f, 0.08f, 0f);
    [SerializeField] private float itemVisualScale = 0.22f;
    [SerializeField] private float fryingMotionRadius = 0.025f;
    [SerializeField] private float fryingMotionSpeed = 18f;

    private readonly List<GameObject> visuals = new List<GameObject>();

    protected override void Awake()
    {
        ConfigureContainerType(ContainerType.Frigideira);
        base.Awake();
    }

    protected override void RefreshVisuals()
    {
        ClearVisuals();

        if (itemSurface == null)
            return;

        for (int i = 0; i < CurrentContents.Count; i++)
            CreateVisual(CurrentContents[i], i, CurrentContents.Count);
    }

    protected override void UpdateSpecificVisuals()
    {
        if (!IsCooking || visuals.Count == 0)
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

    private void CreateVisual(ItemData data, int index, int count)
    {
        if (data == null || data.Prefab == null)
            return;

        GameObject visual = Instantiate(data.Prefab, itemSurface);
        visual.name = $"FrigideiraVisual_{data.DisplayName}";
        visual.transform.localPosition = GetVisualPosition(index, count);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * itemVisualScale;
        DisableGameplayComponents(visual);
        visuals.Add(visual);
    }

    private Vector3 GetVisualPosition(int index, int count)
    {
        if (count <= 1)
            return itemLocalOffset;

        float angle = Mathf.PI * 2f * index / count;
        Vector3 radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        return itemLocalOffset + radial * 0.12f;
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
