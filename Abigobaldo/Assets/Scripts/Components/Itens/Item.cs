using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemType itemType;
    public ItemState currentState = ItemState.Cru;

    [Header("=== REFERENCIAS ===")]
    public Rigidbody rb;
    public Collider col;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }
}

public enum ItemType
{
    None,
    Maçã,
    Tomate,
    Cebola,
    Carne,
    Prato
}

public enum ItemLogic
{
    Jogável,
    Picável,
    Panelável,
    Frigiderável,
    Pratável
}

public enum ItemState
{
    Cru,
    Picado,
    Cozinhando,
    Cozinhado,
    Queimado
}