namespace Abigobaldo.Game
{
    public class CuscuzeiraStation : HeatedContainerStation
    {
        protected override RecipeStationType StationType => RecipeStationType.Cuscuzeira;
        protected override ObjectVisualTarget VisualTarget => ObjectVisualTarget.Cuscuzeira;
        protected override bool ShouldShowContents => false;
    }
}
