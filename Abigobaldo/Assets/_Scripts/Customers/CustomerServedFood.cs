using UnityEngine;

namespace Abigobaldo.Game
{
    public readonly struct CustomerServedFood
    {
        public readonly Plate Plate;
        public readonly HoldableObject Object;
        public readonly string FoodName;
        public readonly FoodState State;

        public bool IsValid => Object != null;
        public bool IsCharcoal => FoodName == "Charcoal" || State == FoodState.Carbonized;

        private CustomerServedFood(Plate plate, HoldableObject foodObject, string foodName, FoodState state)
        {
            Plate = plate;
            Object = foodObject;
            FoodName = foodName;
            State = state;
        }

        public static CustomerServedFood FromCollider(Collider collider)
        {
            Plate plate = collider.GetComponentInParent<Plate>();
            HoldableObject food = plate != null ? plate.ContentObject : collider.GetComponentInParent<HoldableObject>();
            if (food == null)
                return default;

            ObjectIdentity identity = food.GetComponent<ObjectIdentity>();
            RecipeProgress progress = food.GetComponent<RecipeProgress>();
            string name = identity != null && identity.Definition != null ? identity.Definition.name : food.name.Replace("(Clone)", string.Empty).Trim();
            FoodState state = progress != null ? progress.State : FoodState.Ready;
            return new CustomerServedFood(plate, food, name, state);
        }

        public void Consume()
        {
            if (Plate != null)
            {
                // A delivered dish belongs to the customer now. Destroy the whole
                // plate instead of only its child food, so it cannot be left behind.
                UnityEngine.Object.Destroy(Plate.gameObject);
                return;
            }

            if (Object != null)
                UnityEngine.Object.Destroy(Object.gameObject);
        }
    }
}
