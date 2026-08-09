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
    [MenuItem("Abigobaldo/Gameplay/Normalize Scene And Prefabs")]
    public static void NormalizeOpenSceneAndPrefabs()
    {
        EnsureFolders();
        PromoteLikelySceneObjects();

        foreach (HoldableObject holdableObject in Object.FindObjectsOfType<HoldableObject>(true))
            NormalizeObject(holdableObject);

        foreach (FryingPan fryingPan in Object.FindObjectsOfType<FryingPan>(true))
            NormalizeStation(fryingPan);

        foreach (CouscousPot couscousPot in Object.FindObjectsOfType<CouscousPot>(true))
            NormalizeStation(couscousPot);

        foreach (Blender blender in Object.FindObjectsOfType<Blender>(true))
            NormalizeStation(blender);

        foreach (ObjectSpawner spawner in Object.FindObjectsOfType<ObjectSpawner>(true))
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

        Debug.Log("Abigobaldo: scene normalized, missing data created, and prefabs saved/updated.");
    }

    private static void NormalizeObject(HoldableObject holdableObject)
    {
        if (holdableObject == null)
            return;

        GameObject root = holdableObject.gameObject;
        GameLayers.SetLayerRecursivelyIfDefault(root, GameLayers.HoldableObject);

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
                GetOrAdd<HoldableObject>(sceneObject);

            if (TryGetContainerType(normalizedName, out ContainerType containerType))
                ConfigureStation(GetOrAddSpecificStation(sceneObject, containerType), containerType, normalizedName);
        }
    }

    private static bool LooksLikeDoor(string normalizedName)
    {
        return normalizedName.Contains("door");
    }

    private static bool LooksLikeLooseObject(string normalizedName)
    {
        return normalizedName.Contains("salt")
            || normalizedName.Contains("pepper")
            || normalizedName.Contains("monkey")
            || normalizedName.Contains("suzanne")
            || normalizedName.Contains("cuttingboard")
            || normalizedName.Contains("cutting_board")
            || normalizedName.Contains("cutting board");
    }

    private static bool TryGetContainerType(string normalizedName, out ContainerType containerType)
    {
        if (normalizedName.Contains("frying"))
        {
            containerType = ContainerType.FryingPan;
            return true;
        }

        if (normalizedName.Contains("couscous"))
        {
            containerType = ContainerType.CouscousPot;
            return true;
        }

        if (normalizedName.Contains("blender"))
        {
            containerType = ContainerType.Blender;
            return true;
        }

        containerType = default;
        return false;
    }

    private static void ConfigureStation(MonoBehaviour station, ContainerType containerType, string normalizedName)
    {
        SerializedObject serializedObject = new SerializedObject(station);
        SetBool(serializedObject, "canBePickedUp", containerType != ContainerType.Blender);
        SetBool(serializedObject, "requiresManualActivation", containerType == ContainerType.Blender);

        serializedObject.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(station);
    }

    private static MonoBehaviour GetOrAddSpecificStation(GameObject target, ContainerType containerType)
    {
        switch (containerType)
        {
            case ContainerType.FryingPan:
                return GetOrAdd<FryingPan>(target);
            case ContainerType.CouscousPot:
                return GetOrAdd<CouscousPot>(target);
            case ContainerType.Blender:
                return GetOrAdd<Blender>(target);
            default:
                return null;
        }
    }

    private static void NormalizeStation(MonoBehaviour station)
    {
        if (station == null)
            return;

        GameLayers.SetLayerRecursivelyIfDefault(station.gameObject, GameLayers.Container);
        GetOrAdd<Highlightable>(station.gameObject);
        EnsureAtLeastOneCollider(station.gameObject);
        MakeMeshCollidersConvex(station.gameObject);

        EditorUtility.SetDirty(station);
    }

    private static void NormalizeSpawner(ObjectSpawner spawner)
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
        foreach (HoldableObject holdableObject in Object.FindObjectsOfType<HoldableObject>(true))
        {
            if (HasRecipeStation(holdableObject.gameObject))
                continue;

            SavePrefabFor(holdableObject.gameObject, ObjectsFolder);
        }

        foreach (FryingPan fryingPan in Object.FindObjectsOfType<FryingPan>(true))
            SavePrefabFor(fryingPan.gameObject, ContainersFolder);

        foreach (CouscousPot couscousPot in Object.FindObjectsOfType<CouscousPot>(true))
            SavePrefabFor(couscousPot.gameObject, ContainersFolder);

        foreach (Blender blender in Object.FindObjectsOfType<Blender>(true))
            SavePrefabFor(blender.gameObject, ContainersFolder);

        foreach (ObjectSpawner spawner in Object.FindObjectsOfType<ObjectSpawner>(true))
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

    private static bool HasRecipeStation(GameObject target)
    {
        return target.GetComponent<FryingPan>() != null
            || target.GetComponent<CouscousPot>() != null
            || target.GetComponent<Blender>() != null;
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets", "_Prefabs");
        EnsureFolder("Assets/_Prefabs", "Objects");
        EnsureFolder("Assets/_Prefabs", "Containers");
        EnsureFolder("Assets/_Prefabs", "Spawners");
        EnsureFolder("Assets", "_Data");
        EnsureFolder("Assets/_Data", "Objects");
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

        return string.IsNullOrWhiteSpace(clean) ? "HoldableObject" : clean;
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

