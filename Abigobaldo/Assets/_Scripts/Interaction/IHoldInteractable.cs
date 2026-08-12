namespace Abigobaldo.Game
{
    public interface IHoldInteractable
    {
        void BeginHold(PlayerInteractor player);
        void UpdateHold(PlayerInteractor player);
        void EndHold(PlayerInteractor player);
    }
}
