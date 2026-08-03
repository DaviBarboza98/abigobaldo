using UnityEngine;

[System.Serializable]
public class CookingRecipe
{
    [Header("Ingredientes")]
    public string ingredient1;
    public string ingredient2;

    [Header("Resultado")]
    public GameObject resultPrefab;

    [Header("Tempo")]
    public float cookingTime = 10f;
    public float burningTime = 7f;
}