using UnityEngine;

namespace Abigobaldo.Game
{
    public class PlateableObject : MonoBehaviour
    {
        [SerializeField] private bool requireReadyState = true;

        public bool CanPlate(HoldableObject source)
        {
            if (source == null)
                return false;

            if (!requireReadyState)
                return true;

            RecipeProgress progress = source.GetComponent<RecipeProgress>();
            return progress == null || progress.IsReady;
        }
    }
}
