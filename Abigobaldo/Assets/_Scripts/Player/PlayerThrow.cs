using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerThrow : MonoBehaviour
{
    private PlayerInputHandler input;

    private void Awake()
    {
        input = GetComponent<PlayerInputHandler>();
    }

    private void Update()
    {
        if (input.DropPressed)
        {
            // Drop input detected. Item logic is disabled.
        }

        if (input.ThrowPressed)
        {
            // Throw input detected. Item logic is disabled.
        }
    }
}