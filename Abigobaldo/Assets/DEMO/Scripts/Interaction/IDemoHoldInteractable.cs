namespace Abigobaldo.Demo
{
    public interface IDemoHoldInteractable
    {
        void BeginHold(DemoPlayerInteractor player);
        void UpdateHold(DemoPlayerInteractor player);
        void EndHold(DemoPlayerInteractor player);
    }
}
