using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RecipeData))]
public class RecipeDataEditor : Editor
{
    private SerializedProperty requiredContainer;
    private SerializedProperty ingredients;
    private SerializedProperty resultObject;
    private SerializedProperty readyObject;
    private SerializedProperty overcookedObject;
    private SerializedProperty burnedObject;
    private SerializedProperty carbonizedObject;
    private SerializedProperty readyMaterial;
    private SerializedProperty overcookedMaterial;
    private SerializedProperty burnedMaterial;
    private SerializedProperty carbonizedMaterial;
    private SerializedProperty byproducts;
    private SerializedProperty spawnByproductsOnStart;
    private SerializedProperty cookingTime;
    private SerializedProperty canOvercook;
    private SerializedProperty slightlyBurnedDelay;
    private SerializedProperty burnedDelay;
    private SerializedProperty carbonizedDelay;

    private void OnEnable()
    {
        requiredContainer = serializedObject.FindProperty("requiredContainer");
        ingredients = serializedObject.FindProperty("ingredients");
        resultObject = serializedObject.FindProperty("resultObject");
        readyObject = serializedObject.FindProperty("readyObject");
        overcookedObject = serializedObject.FindProperty("overcookedObject");
        burnedObject = serializedObject.FindProperty("burnedObject");
        carbonizedObject = serializedObject.FindProperty("carbonizedObject");
        readyMaterial = serializedObject.FindProperty("readyMaterial");
        overcookedMaterial = serializedObject.FindProperty("overcookedMaterial");
        burnedMaterial = serializedObject.FindProperty("burnedMaterial");
        carbonizedMaterial = serializedObject.FindProperty("carbonizedMaterial");
        byproducts = serializedObject.FindProperty("byproducts");
        spawnByproductsOnStart = serializedObject.FindProperty("spawnByproductsOnStart");
        cookingTime = serializedObject.FindProperty("cookingTime");
        canOvercook = serializedObject.FindProperty("canOvercook");
        slightlyBurnedDelay = serializedObject.FindProperty("slightlyBurnedDelay");
        burnedDelay = serializedObject.FindProperty("burnedDelay");
        carbonizedDelay = serializedObject.FindProperty("carbonizedDelay");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(requiredContainer);
        EditorGUILayout.PropertyField(ingredients);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Result", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(resultObject, new GUIContent("Raw / Cooking Object"));
        DrawStateRow("Ready", readyObject, readyMaterial);
        DrawStateRow("Overcooked", overcookedObject, overcookedMaterial);
        DrawStateRow("Burned", burnedObject, burnedMaterial);
        DrawStateRow("Carbonized", carbonizedObject, carbonizedMaterial);

        EditorGUILayout.Space(8f);
        EditorGUILayout.PropertyField(byproducts);
        EditorGUILayout.PropertyField(spawnByproductsOnStart);

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Timing", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(cookingTime);
        EditorGUILayout.PropertyField(canOvercook);

        if (canOvercook.boolValue)
        {
            EditorGUILayout.PropertyField(slightlyBurnedDelay);
            EditorGUILayout.PropertyField(burnedDelay);
            EditorGUILayout.PropertyField(carbonizedDelay);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static void DrawStateRow(string label, SerializedProperty objectProperty, SerializedProperty materialProperty)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(label);
        EditorGUILayout.PropertyField(objectProperty, GUIContent.none);
        EditorGUILayout.PropertyField(materialProperty, GUIContent.none);
        EditorGUILayout.EndHorizontal();
    }
}
