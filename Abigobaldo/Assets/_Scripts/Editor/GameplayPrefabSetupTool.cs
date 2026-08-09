using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class GameplayPrefabSetupTool
{
    private const string ObjectsFolder = "Assets/_Prefabs/Objects";
    private const string ContainersFolder = "Assets/_Prefabs/Containers";
    private const string SpawnersFolder = "Assets/_Prefabs/Spawners";
    [MenuItem("Abigobaldo/Gameplay/Normalizar cena e prefabs")]
    public static void NormalizeOpenSceneAndPrefabs()
    {
        EnsureFolders();
        PromoteLikelySceneObjects();

        foreach (Objeto objeto in Object.FindObjectsOfType<Objeto>(true))
            NormalizeObjeto(objeto);

        foreach (RecipeContainer container in Object.FindObjectsOfType<RecipeContainer>(true))
            NormalizeContainer(container);

        foreach (ObjetoSpawner spawner in Object.FindObjectsOfType<ObjetoSpawner>(true))
            NormalizeSpawner(spawner);

        foreach (OpenableDoor door in Object.FindObjectsOfType<OpenableDoor>(true))
            NormalizeDoor(door);

        foreach (ParticleEmitterController emitter in Object.FindObjectsOfType<ParticleEmitterController>(true))
            NormalizeParticleEmitter(emitter);

        foreach (ParticleSystem particleSystem in Object.FindObjectsOfType<ParticleSystem>(true))
            EnsureParticleController(particleSystem);

        SaveScenePrefabs();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Abigobaldo: cena normalizada, dados criados quando faltavam e prefabs salvos/atualizados.");
    }

    private static void NormalizeObjeto(Objeto objeto)
    {
        if (objeto == null)
            return;

        GameObject root = objeto.gameObject;
        GameLayers.SetLayerRecursivelyIfDefault(root, GameLayers.Objeto);

        Rigidbody body = GetOrAdd<Rigidbody>(root);
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        body.interpolation = RigidbodyInterpolation.Interpolate;

        EnsureAtLeastOneCollider(root);
        MakeMeshCollidersConvex(root);
        GetOrAdd<Highlightable>(root);

        EditorUtility.SetDirty(root);
    }

    private static void PromoteLikelySceneObjects()
    {
        foreach (GameObject sceneObject in Object.FindObjectsOfType<GameObject>(true))
        {
            if (!sceneObject.scene.IsValid())
                continue;

            string normalizedName = sceneObject.name.ToLowerInvariant();

            if (LooksLikeDoor(normalizedName))
                GetOrAdd<OpenableDoor>(sceneObject);

            if (LooksLikeLooseObject(normalizedName))
                GetOrAdd<Objeto>(sceneObject);

            if (TryGetContainerType(normalizedName, out ContainerType containerType))
                ConfigureContainer(GetOrAddSpecificContainer(sceneObject, containerType), containerType, normalizedName);
        }
    }

    private static bool LooksLikeDoor(string normalizedName)
    {
        return normalizedName.Contains("door") || normalizedName.Contains("porta");
    }

    private static bool LooksLikeLooseObject(string normalizedName)
    {
        return normalizedName.Contains("saleiro")
            || normalizedName.Contains("pimenteiro")
            || normalizedName.Contains("macaco")
            || normalizedName.Contains("suzanne")
            || normalizedName.Contains("tabua")
            || normalizedName.Contains("tábua");
    }

    private static bool TryGetContainerType(string normalizedName, out ContainerType containerType)
    {
        if (normalizedName.Contains("frigideira") || normalizedName.Contains("frying"))
        {
            containerType = ContainerType.Frigideira;
            return true;
        }

        if (normalizedName.Contains("cuscuzeira") || normalizedName.Contains("cuscuz"))
        {
            containerType = ContainerType.Cuscuzeira;
            return true;
        }

        if (normalizedName.Contains("liquidificador") || normalizedName.Contains("blender"))
        {
            containerType = ContainerType.Liquidificador;
            return true;
        }

        containerType = default;
        return false;
    }

    private static void ConfigureContainer(RecipeContainer container, ContainerType containerType, string normalizedName)
    {
        SerializedObject serializedObject = new SerializedObject(container);
        SetEnum(serializedObject, "containerType", (int)containerType);
        SetBool(serializedObject, "canBePickedUp", containerType != ContainerType.Liquidificador);
        SetBool(serializedObject, "requiresManualActivation", containerType == ContainerType.Liquidificador);

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(container);
    }

    private static RecipeContainer GetOrAddSpecificContainer(GameObject target, ContainerType containerType)
    {
        RecipeContainer existing = target.GetComponent<RecipeContainer>();

        if (existing != null)
            return existing;

        switch (containerType)
        {
            case ContainerType.Frigideira:
                return target.AddComponent<FryingPan>();
            case ContainerType.Cuscuzeira:
                return target.AddComponent<Cuscuzeira>();
            case ContainerType.Liquidificador:
                return target.AddComponent<Blender>();
            default:
                return target.AddComponent<RecipeContainer>();
        }
    }

    private static void NormalizeContainer(RecipeContainer container)
    {
        if (container == null)
            return;

        GameLayers.SetLayerRecursivelyIfDefault(container.gameObject, GameLayers.Container);
        GetOrAdd<Highlightable>(container.gameObject);
        EnsureAtLeastOneCollider(container.gameObject);
        MakeMeshCollidersConvex(container.gameObject);

        EditorUtility.SetDirty(container);
    }

    private static void NormalizeSpawner(ObjetoSpawner spawner)
    {
        if (spawner == null)
            return;

        GameLayers.SetLayerRecursivelyIfDefault(spawner.gameObject, GameLayers.Spawner);
        GetOrAdd<Highlightable>(spawner.gameObject);

        Collider collider = spawner.GetComponent<Collider>();
        if (collider == null)
            collider = spawner.gameObject.AddComponent<BoxCollider>();

        collider.isTrigger = true;
        EditorUtility.SetDirty(spawner.gameObject);
    }

    private static void NormalizeDoor(OpenableDoor door)
    {
        if (door == null)
            return;

        GameLayers.SetLayerRecursivelyIfDefault(door.gameObject, GameLayers.Door);
        GetOrAdd<Highlightable>(door.gameObject);
        EnsureAtLeastOneCollider(door.gameObject);
        EditorUtility.SetDirty(door.gameObject);
    }

    private static void NormalizeParticleEmitter(ParticleEmitterController emitter)
    {
        if (emitter == null)
            return;

        emitter.ApplyBestPresetFromHierarchy();
        EditorUtility.SetDirty(emitter);
    }

    private static void EnsureParticleController(ParticleSystem particleSystem)
    {
        if (particleSystem == null)
            return;

        ParticleEmitterController controller = particleSystem.GetComponentInParent<ParticleEmitterController>();
        if (controller == null)
            controller = particleSystem.gameObject.AddComponent<ParticleEmitterController>();

        controller.ApplyBestPresetFromHierarchy();
        EditorUtility.SetDirty(controller);
    }

    private static void SaveScenePrefabs()
    {
        foreach (Objeto objeto in Object.FindObjectsOfType<Objeto>(true))
        {
            if (objeto.GetComponent<RecipeContainer>() != null)
                continue;

            SavePrefabFor(objeto.gameObject, ObjectsFolder);
        }

        foreach (RecipeContainer container in Object.FindObjectsOfType<RecipeContainer>(true))
            SavePrefabFor(container.gameObject, ContainersFolder);

        foreach (ObjetoSpawner spawner in Object.FindObjectsOfType<ObjetoSpawner>(true))
            SavePrefabFor(spawner.gameObject, SpawnersFolder);
    }

    private static void SavePrefabFor(GameObject root, string folder)
    {
        if (root == null || root.scene.IsValid() == false)
            return;

        if (PrefabUtility.IsPartOfPrefabAsset(root))
            return;

        GameObject outerRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(root);
        if (outerRoot != null && outerRoot != root)
            return;

        string path = $"{folder}/{SanitizeFileName(root.name)}.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(root, path, InteractionMode.AutomatedAction);
    }

    private static void EnsureAtLeastOneCollider(GameObject root)
    {
        if (root.GetComponentInChildren<Collider>() != null)
            return;

        BoxCollider collider = root.AddComponent<BoxCollider>();
        Bounds bounds = GetRendererBounds(root);

        if (bounds.size == Vector3.zero)
        {
            collider.size = Vector3.one * 0.35f;
            return;
        }

        collider.center = root.transform.InverseTransformPoint(bounds.center);
        collider.size = Abs(root.transform.InverseTransformVector(bounds.size));
    }

    private static void MakeMeshCollidersConvex(GameObject root)
    {
        foreach (MeshCollider meshCollider in root.GetComponentsInChildren<MeshCollider>(true))
        {
            if (meshCollider == null)
                continue;

            meshCollider.convex = true;
            EditorUtility.SetDirty(meshCollider);
        }
    }

    private static Bounds GetRendererBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        Bounds bounds = new Bounds(root.transform.position, Vector3.zero);
        bool hasBounds = false;

        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer == null)
                continue;

            if (!hasBounds)
            {
                bounds = targetRenderer.bounds;
                hasBounds = true;
                continue;
            }

            bounds.Encapsulate(targetRenderer.bounds);
        }

        return hasBounds ? bounds : new Bounds(root.transform.position, Vector3.zero);
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : target.AddComponent<T>();
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "_Prefabs");
        EnsureFolder("Assets/_Prefabs", "Objects");
        EnsureFolder("Assets/_Prefabs", "Containers");
        EnsureFolder("Assets/_Prefabs", "Spawners");
        EnsureFolder("Assets", "_Data");
        EnsureFolder("Assets/_Data", "Objetos");
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path))
            AssetDatabase.CreateFolder(parent, child);
    }

    private static string SanitizeFileName(string value)
    {
        string clean = value.Trim();

        foreach (char invalid in Path.GetInvalidFileNameChars())
            clean = clean.Replace(invalid.ToString(CultureInfo.InvariantCulture), string.Empty);

        return string.IsNullOrWhiteSpace(clean) ? "Objeto" : clean;
    }

    private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.boolValue = value;
    }

    private static void SetEnum(SerializedObject serializedObject, string propertyName, int value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
            property.enumValueIndex = value;
    }

    private static Vector3 Abs(Vector3 value)
    {
        return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }
}
