using UnityEngine;

public class Highlightable : MonoBehaviour
{
    [Header("Renderers")]
    [SerializeField] private bool includeInactiveRenderers;

    [Header("Override opcional")]
    [SerializeField] private bool useLocalColors;
    [SerializeField] private Color localHighlightColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color localEmissionColor = new Color(1f, 0.65f, 0.05f, 1f);
    [SerializeField] private float localEmissionIntensity = 1.1f;

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private Renderer[] renderers;
    private IHighlightStateReceiver[] stateReceivers;
    private MaterialPropertyBlock propertyBlock;
    private bool isHighlighted;

    public bool IsHighlighted => isHighlighted;

    private void Awake()
    {
        RefreshRenderers();
        EnableEmissionKeyword();
    }

    private void OnDisable()
    {
        SetHighlighted(false);
    }

    public void SetHighlighted(bool highlighted)
    {
        if (isHighlighted == highlighted)
            return;

        isHighlighted = highlighted;

        if (renderers == null || renderers.Length == 0)
            RefreshRenderers();

        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();

        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer == null)
                continue;

            targetRenderer.GetPropertyBlock(propertyBlock);

            if (highlighted)
            {
                Color activeHighlightColor = GetHighlightColor();
                Color activeEmissionColor = GetEmissionColor();
                float activeEmissionIntensity = GetEmissionIntensity();

                propertyBlock.SetColor(ColorId, activeHighlightColor);
                propertyBlock.SetColor(BaseColorId, activeHighlightColor);
                propertyBlock.SetColor(EmissionColorId, activeEmissionColor * activeEmissionIntensity);
            }
            else
            {
                propertyBlock.Clear();
            }

            targetRenderer.SetPropertyBlock(propertyBlock);
        }

        NotifyStateReceivers(highlighted);
    }

    public void Highlight()
    {
        SetHighlighted(true);
    }

    public void ClearHighlight()
    {
        SetHighlighted(false);
    }

    public void RefreshRenderers()
    {
        renderers = GetComponentsInChildren<Renderer>(includeInactiveRenderers);
        stateReceivers = GetComponentsInChildren<IHighlightStateReceiver>(includeInactiveRenderers);
        EnableEmissionKeyword();
    }

    private void EnableEmissionKeyword()
    {
        if (renderers == null)
            return;

        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer == null)
                continue;

            foreach (Material material in targetRenderer.sharedMaterials)
            {
                if (material == null)
                    continue;

                if (material.HasProperty(EmissionColorId))
                    material.EnableKeyword("_EMISSION");
            }
        }
    }

    private Color GetHighlightColor()
    {
        if (useLocalColors)
            return localHighlightColor;

        return GameInteractionManager.Instance != null
            ? GameInteractionManager.Instance.HighlightColor
            : Color.yellow;
    }

    private Color GetEmissionColor()
    {
        if (useLocalColors)
            return localEmissionColor;

        return GameInteractionManager.Instance != null
            ? GameInteractionManager.Instance.EmissionColor
            : new Color(1f, 0.65f, 0.05f, 1f);
    }

    private float GetEmissionIntensity()
    {
        if (useLocalColors)
            return localEmissionIntensity;

        return GameInteractionManager.Instance != null
            ? GameInteractionManager.Instance.EmissionIntensity
            : 1.1f;
    }

    private void NotifyStateReceivers(bool highlighted)
    {
        if (stateReceivers == null || stateReceivers.Length == 0)
            stateReceivers = GetComponentsInChildren<IHighlightStateReceiver>(includeInactiveRenderers);

        foreach (IHighlightStateReceiver receiver in stateReceivers)
            receiver?.OnHighlightChanged(highlighted);
    }
}


