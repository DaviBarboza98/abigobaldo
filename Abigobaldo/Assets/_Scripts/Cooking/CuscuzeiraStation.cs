namespace Abigobaldo.Game
{
    public class CuscuzeiraStation : ContainerStation
    {
        protected override ObjectVisualTarget VisualTarget => ObjectVisualTarget.Cuscuzeira;

        protected override RecipeData FindRecipe(DemoRecipeBook book, ObjectKind inputKind)
        {
            return book.FindCuscuzeiraRecipe(inputKind);
        }
    }
}
