using UnityEngine;

public class Highlightable : MonoBehaviour
{
    [Header("Highlight")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color emissionColor = new Color(1f, 0.65f, 0.05f, 1f);
    [SerializeField] private float emissionIntensity = 0.9f;
    [SerializeField] private bool includeInactiveRenderers;

    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private bool isHighlighted;

    public bool IsHighlighted => isHighlighted;

    private void Awake()
    {
        RefreshRenderers();
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
                propertyBlock.SetColor(ColorId, highlightColor);
                propertyBlock.SetColor(BaseColorId, highlightColor);
                propertyBlock.SetColor(EmissionColorId, emissionColor * emissionIntensity);
            }
            else
            {
                propertyBlock.Clear();
            }

            targetRenderer.SetPropertyBlock(propertyBlock);
        }
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
    }
}
