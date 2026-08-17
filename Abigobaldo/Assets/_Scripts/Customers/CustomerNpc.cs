using UnityEngine;

namespace Abigobaldo.Game
{
    [RequireComponent(typeof(BoxCollider))]
    // Kept on each character prefab so spawned NPCs are immediately interactive.
    public sealed class CustomerNpc : MonoBehaviour, IInteractable
    {
        private BoxCollider hitbox;
        private bool acceptingDelivery;
        private bool delivered;

        public string CustomerId { get; private set; }
        public string RealName { get; private set; }
        public CustomerTemperament Temperament { get; private set; }
        public bool NameKnown { get; private set; }
        public string SpeakerName => NameKnown ? RealName : "???";

        public void Configure(string id, string realName, CustomerTemperament temperament, bool nameKnown)
        {
            CustomerId = id;
            RealName = realName;
            Temperament = temperament;
            NameKnown = nameKnown;
            delivered = false;
            // Food only counts after the customer has finished asking for it.
            // This prevents a plate thrown near a newly spawned NPC from
            // skipping their Console dialogue.
            acceptingDelivery = false;
            hitbox = GetComponent<BoxCollider>();
            hitbox.isTrigger = true;
        }

        public void RevealName() => NameKnown = true;

        public void SetAcceptingDelivery(bool value)
        {
            acceptingDelivery = value;
        }

        public void MarkDeliveryAccepted()
        {
            delivered = true;
            acceptingDelivery = false;
        }

        public void Interact(PlayerInteractor player) => CustomerManager.Instance?.TalkTo(this);

        private void OnTriggerEnter(Collider other)
        {
            if (!acceptingDelivery || delivered || other == null)
                return;

            CustomerServedFood food = CustomerServedFood.FromCollider(other);
            if (!food.IsValid)
                return;

            CustomerManager.Instance?.ReceiveFood(this, food);
        }
    }

    public enum CustomerTemperament { Warm, Timid, Gruff }
}
