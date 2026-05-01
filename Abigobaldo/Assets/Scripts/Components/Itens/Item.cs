using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemType itemType;
    public ItemState currentState = ItemState.Cru;
    [HideInInspector] public Transform followTarget;

    [Header("=== REFERENCIAS ===")]
    public Rigidbody rb;
    public Collider col;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }
    void LateUpdate()
    {
        if (followTarget != null)
        {
            transform.position = followTarget.position;
            transform.rotation = followTarget.rotation;
        }
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