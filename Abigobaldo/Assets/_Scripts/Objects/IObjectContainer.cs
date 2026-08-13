namespace Abigobaldo.Game
{
    public interface IObjectContainer
    {
        bool HasContent { get; }
        HoldableObject Holdable { get; }
        bool TryInsertObject(HoldableObject item, PlayerInteractor player);
        bool TryTakeLastObject(PlayerInteractor player);
        bool TryMoveLastObjectTo(IObjectContainer target, PlayerInteractor player);
    }
}
