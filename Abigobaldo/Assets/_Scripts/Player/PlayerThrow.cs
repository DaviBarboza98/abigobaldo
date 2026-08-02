using UnityEngine;

[RequireComponent(typeof(PlayerInventory))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerThrow : MonoBehaviour
{
    [Header("Throw")]

    [SerializeField]
    private float throwForce = 8f;

    private PlayerInventory inventory;
    private PlayerInputHandler input;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        input = GetComponent<PlayerInputHandler>();
    }

    private void Update()
    {
        if (input.DropPressed)
        {
            inventory.Drop();
        }

        if (input.ThrowPressed)
        {
            inventory.Throw(throwForce);
        }
    }
}