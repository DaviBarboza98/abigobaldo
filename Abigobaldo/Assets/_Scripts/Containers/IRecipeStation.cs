public interface IRecipeStation : IInteractable
{
    bool HasStoredObjects { get; }
    bool HasReadyOutput { get; }
    bool TryPickUpContainer(Holder holder);
    bool TryMoveOutputToPlate(PlateContainer plate);
}


