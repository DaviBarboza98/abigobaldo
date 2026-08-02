using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerInventory))]
public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactRange = 2.5f;

    private PlayerInputHandler input;
    private PlayerInventory inventory;
    private Camera mainCamera;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
        inventory = GetComponent<PlayerInventory>();
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (input.InteractPressed)
        {
            TryInteract();
        }
    }

    private void TryInteract()
    {
        if (inventory == null || input == null || mainCamera == null)
            return;

        if (inventory.HasItem)
            return;

        Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, interactRange))
            return;

        Item item = hit.collider.GetComponent<Item>();
        if (item == null || !item.IsHoldable)
            return;

        inventory.PickUp(item);
    }
}
