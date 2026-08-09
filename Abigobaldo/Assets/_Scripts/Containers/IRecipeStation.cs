public interface IRecipeStation : IInteractable
{
    bool HasReadyOutput { get; }
    bool TryPickUpContainer(Holder holder);
    bool TryMoveOutputToPlate(PlateContainer plate);
}

