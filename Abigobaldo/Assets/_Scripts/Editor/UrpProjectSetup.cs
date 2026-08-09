using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[InitializeOnLoad]
public static class UrpProjectSetup
{
    private const string UrpFolder = "Assets/Settings/URP";
    private const string PipelineAssetPath = UrpFolder + "/Abigobaldo_URP.asset";
    private const string RendererAssetPath = "Assets/UniversalRenderer.asset";
    private const string AutoSetupKey = "Abigobaldo.URP.AutoSetupDone";

    static UrpProjectSetup()
    {
        EditorApplication.delayCall += RunAutomaticSetupOnce;
    }

    [MenuItem("Abigobaldo/Rendering/Setup URP")]
    public static void SetupUrp()
    {
        EnsureFolders();

        UniversalRenderPipelineAsset pipelineAsset = GetOrCreatePipelineAsset();
        ConfigurePipelineAsset(pipelineAsset);
        ApplyPipelineAsset(pipelineAsset);
        ConvertBuiltInMaterialsToUrp();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Abigobaldo URP: pipeline configurado e materiais convertidos para URP.");
    }

    [MenuItem("Abigobaldo/Rendering/Convert Materials To URP")]
    public static void ConvertBuiltInMaterialsToUrp()
    {
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");

        if (litShader == null)
        {
            Debug.LogWarning("Abigobaldo URP: Universal Render Pipeline/Lit shader was not found.");
            return;
        }

        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets" });

        foreach (string materialGuid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(materialGuid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null || material.shader == null)
                continue;

            string shaderName = material.shader.name;

            if (shaderName.StartsWith("Universal Render Pipeline"))
                continue;

            if (shaderName.StartsWith("Skybox"))
                continue;

            ConvertMaterial(material, litShader, unlitShader);
            EditorUtility.SetDirty(material);
        }
    }

    private static void RunAutomaticSetupOnce()
    {
        if (SessionState.GetBool(AutoSetupKey, false))
            return;

        SessionState.SetBool(AutoSetupKey, true);
        SetupUrp();
    }

    private static UniversalRenderPipelineAsset GetOrCreatePipelineAsset()
    {
        UniversalRenderPipelineAsset pipelineAsset =
            AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelineAssetPath);

        if (pipelineAsset != null)
            return pipelineAsset;

        pipelineAsset = UniversalRenderPipelineAsset.Create();
        AssetDatabase.CreateAsset(pipelineAsset, PipelineAssetPath);
        pipelineAsset.LoadBuiltinRendererData(RendererType.UniversalRenderer);

        return pipelineAsset;
    }

    private static void ConfigurePipelineAsset(UniversalRenderPipelineAsset pipelineAsset)
    {
        if (pipelineAsset == null)
            return;

        pipelineAsset.supportsCameraDepthTexture = true;
        pipelineAsset.supportsCameraOpaqueTexture = false;
        pipelineAsset.supportsHDR = true;
        pipelineAsset.msaaSampleCount = 2;
        pipelineAsset.renderScale = 1f;
        pipelineAsset.useSRPBatcher = true;

        EditorUtility.SetDirty(pipelineAsset);
    }

    private static void ApplyPipelineAsset(RenderPipelineAsset pipelineAsset)
    {
        GraphicsSettings.renderPipelineAsset = pipelineAsset;

        string[] qualityNames = QualitySettings.names;
        int currentQuality = QualitySettings.GetQualityLevel();

        for (int i = 0; i < qualityNames.Length; i++)
        {
            QualitySettings.SetQualityLevel(i, false);
            QualitySettings.renderPipeline = pipelineAsset;
        }

        QualitySettings.SetQualityLevel(currentQuality, false);
    }

    private static void ConvertMaterial(Material material, Shader litShader, Shader unlitShader)
    {
        Color baseColor = material.HasProperty("_Color")
            ? material.GetColor("_Color")
            : Color.white;

        Texture mainTexture = material.HasProperty("_MainTex")
            ? material.GetTexture("_MainTex")
            : null;

        Texture normalTexture = material.HasProperty("_BumpMap")
            ? material.GetTexture("_BumpMap")
            : null;

        Texture metallicTexture = material.HasProperty("_MetallicGlossMap")
            ? material.GetTexture("_MetallicGlossMap")
            : null;

        float metallic = material.HasProperty("_Metallic")
            ? material.GetFloat("_Metallic")
            : 0f;

        float smoothness = material.HasProperty("_Glossiness")
            ? material.GetFloat("_Glossiness")
            : 0.5f;

        material.shader = litShader != null ? litShader : unlitShader;

        SetColorIfPresent(material, "_BaseColor", baseColor);
        SetColorIfPresent(material, "_Color", baseColor);
        SetTextureIfPresent(material, "_BaseMap", mainTexture);
        SetTextureIfPresent(material, "_MainTex", mainTexture);
        SetTextureIfPresent(material, "_BumpMap", normalTexture);
        SetTextureIfPresent(material, "_MetallicGlossMap", metallicTexture);
        SetFloatIfPresent(material, "_Metallic", metallic);
        SetFloatIfPresent(material, "_Smoothness", smoothness);

        ConfigureSurface(material, baseColor.a);
    }

    private static void ConfigureSurface(Material material, float alpha)
    {
        bool transparent = alpha < 0.99f;

        SetFloatIfPresent(material, "_Surface", transparent ? 1f : 0f);
        SetFloatIfPresent(material, "_Blend", 0f);
        SetFloatIfPresent(material, "_AlphaClip", 0f);
        SetFloatIfPresent(material, "_SrcBlend", transparent ? (float)BlendMode.SrcAlpha : (float)BlendMode.One);
        SetFloatIfPresent(material, "_DstBlend", transparent ? (float)BlendMode.OneMinusSrcAlpha : (float)BlendMode.Zero);
        SetFloatIfPresent(material, "_ZWrite", transparent ? 0f : 1f);

        material.renderQueue = transparent
            ? (int)RenderQueue.Transparent
            : -1;

        if (transparent)
        {
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
        }
        else
        {
            material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHATEST_ON");
        }
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "Settings");
        EnsureFolder("Assets/Settings", "URP");

        if (!Directory.Exists("Assets/_Scripts/Editor"))
            Directory.CreateDirectory("Assets/_Scripts/Editor");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = parent + "/" + child;

        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }

    private static void SetColorIfPresent(Material material, string propertyName, Color value)
    {
        if (material.HasProperty(propertyName))
            material.SetColor(propertyName, value);
    }

    private static void SetTextureIfPresent(Material material, string propertyName, Texture value)
    {
        if (value != null && material.HasProperty(propertyName))
            material.SetTexture(propertyName, value);
    }

    private static void SetFloatIfPresent(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
            material.SetFloat(propertyName, value);
    }
}

