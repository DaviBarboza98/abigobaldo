[System.Serializable]
public class CookingRecipe
{
    [System.Obsolete("Use RecipeData ScriptableObject para novas receitas.")]
    public string ingredient1;
    public string ingredient2;
    public ItemData resultItem;
    public float cookingTime = 10f;
    public float burningTime = 7f;
}
