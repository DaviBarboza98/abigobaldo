using UnityEngine;

namespace Abigobaldo.Game
{
    public class OutlineHighlightable : MonoBehaviour
    {
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
        private static Material sharedOutlineMaterial;

        [SerializeField] private Color outlineColor = new Color(1f, 0.78f, 0.1f, 1f);
        [SerializeField] private float outlineWidth = 0.025f;
        [SerializeField] private Renderer[] targetRenderers;

        private Material outlineMaterialInstance;
        private bool highlighted;

        private void Awake()
        {
            CacheRenderers();
        }

        public void SetHighlighted(bool value)
        {
            if (highlighted == value)
                return;

            highlighted = value;

            if (highlighted)
                ApplyOutline();
            else
                RemoveOutline();
        }

        private void ApplyOutline()
        {
            CacheRenderers();
            Material outlineMaterial = GetOutlineMaterial();

            if (outlineMaterial == null)
                return;

            foreach (Renderer targetRenderer in targetRenderers)
            {
                if (targetRenderer == null || !BelongsToThisHighlight(targetRenderer))
                    continue;

                Material[] sourceMaterials = targetRenderer.sharedMaterials;

                if (ContainsMaterial(sourceMaterials, outlineMaterial))
                    continue;

                Material[] outlinedMaterials = new Material[sourceMaterials.Length + 1];

                for (int i = 0; i < sourceMaterials.Length; i++)
                    outlinedMaterials[i] = sourceMaterials[i];

                outlinedMaterials[outlinedMaterials.Length - 1] = outlineMaterial;
                targetRenderer.sharedMaterials = outlinedMaterials;
            }
        }

        private void RemoveOutline()
        {
            if (targetRenderers == null || outlineMaterialInstance == null)
                return;

            foreach (Renderer targetRenderer in targetRenderers)
            {
                if (targetRenderer == null)
                    continue;

                Material[] currentMaterials = targetRenderer.sharedMaterials;
                int outlineCount = 0;

                foreach (Material material in currentMaterials)
                {
                    if (material == outlineMaterialInstance)
                        outlineCount++;
                }

                if (outlineCount == 0)
                    continue;

                Material[] restoredMaterials = new Material[currentMaterials.Length - outlineCount];
                int destinationIndex = 0;

                foreach (Material material in currentMaterials)
                {
                    if (material != outlineMaterialInstance)
                        restoredMaterials[destinationIndex++] = material;
                }

                targetRenderer.sharedMaterials = restoredMaterials;
            }
        }

        private static bool ContainsMaterial(Material[] materials, Material target)
        {
            if (materials == null || target == null)
                return false;

            foreach (Material material in materials)
            {
                if (material == target)
                    return true;
            }

            return false;
        }

        private Material GetOutlineMaterial()
        {
            if (outlineMaterialInstance != null)
                return outlineMaterialInstance;

            if (sharedOutlineMaterial == null)
            {
                Shader shader = Shader.Find("Abigobaldo/Outline");

                if (shader == null)
                {
                    Debug.LogWarning("Outline shader not found. Highlight will not render.", this);
                    return null;
                }

                sharedOutlineMaterial = new Material(shader)
                {
                    name = "Outline Runtime Material"
                };
            }

            outlineMaterialInstance = new Material(sharedOutlineMaterial)
            {
                name = $"{name} Outline Material"
            };

            outlineMaterialInstance.SetColor(OutlineColorId, outlineColor);
            outlineMaterialInstance.SetFloat(OutlineWidthId, outlineWidth);
            return outlineMaterialInstance;
        }

        private void CacheRenderers()
        {
            if (targetRenderers != null && targetRenderers.Length > 0)
                return;

            targetRenderers = GetComponentsInChildren<Renderer>();
        }

        private bool BelongsToThisHighlight(Renderer targetRenderer)
        {
            OutlineHighlightable closestHighlight = targetRenderer.GetComponentInParent<OutlineHighlightable>();
            return closestHighlight == null || closestHighlight == this;
        }

        private void OnDisable()
        {
            if (highlighted)
                SetHighlighted(false);
        }

        private void OnValidate()
        {
            outlineWidth = Mathf.Clamp(outlineWidth, 0f, 0.08f);

            if (outlineMaterialInstance != null)
            {
                outlineMaterialInstance.SetColor(OutlineColorId, outlineColor);
                outlineMaterialInstance.SetFloat(OutlineWidthId, outlineWidth);
            }
        }
    }
}
