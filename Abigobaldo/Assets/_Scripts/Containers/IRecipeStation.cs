public interface IRecipeStation : IInteractable
{
    bool HasReadyOutput { get; }
    bool TryPickUpContainer(ItemHolder holder);
    bool TryMoveOutputToPlate(PlateContainer plate);
}
