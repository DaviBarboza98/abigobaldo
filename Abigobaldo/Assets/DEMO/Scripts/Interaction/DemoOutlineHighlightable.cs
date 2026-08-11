using System.Collections.Generic;
using UnityEngine;

namespace Abigobaldo.Demo
{
    public class DemoOutlineHighlightable : MonoBehaviour
    {
        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
        private static Material sharedOutlineMaterial;

        [SerializeField] private Color outlineColor = new Color(1f, 0.78f, 0.1f, 1f);
        [SerializeField] private float outlineWidth = 0.025f;
        [SerializeField] private Renderer[] targetRenderers;

        private readonly Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
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
                if (targetRenderer == null)
                    continue;

                if (!originalMaterials.ContainsKey(targetRenderer))
                    originalMaterials[targetRenderer] = targetRenderer.sharedMaterials;

                Material[] sourceMaterials = originalMaterials[targetRenderer];
                Material[] outlinedMaterials = new Material[sourceMaterials.Length + 1];

                for (int i = 0; i < sourceMaterials.Length; i++)
                    outlinedMaterials[i] = sourceMaterials[i];

                outlinedMaterials[outlinedMaterials.Length - 1] = outlineMaterial;
                targetRenderer.sharedMaterials = outlinedMaterials;
            }
        }

        private void RemoveOutline()
        {
            foreach (KeyValuePair<Renderer, Material[]> pair in originalMaterials)
            {
                if (pair.Key != null)
                    pair.Key.sharedMaterials = pair.Value;
            }

            originalMaterials.Clear();
        }

        private Material GetOutlineMaterial()
        {
            if (outlineMaterialInstance != null)
                return outlineMaterialInstance;

            if (sharedOutlineMaterial == null)
            {
                Shader shader = Shader.Find("Abigobaldo/Demo/Outline");

                if (shader == null)
                {
                    Debug.LogWarning("Demo outline shader not found. Highlight will not render.", this);
                    return null;
                }

                sharedOutlineMaterial = new Material(shader)
                {
                    name = "Demo Outline Runtime Material"
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
