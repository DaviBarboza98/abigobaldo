using UnityEngine;

[CreateAssetMenu(
    fileName = "NovoItem",
    menuName = "Abigobaldos/Item Data"
)]
public class ItemData : ScriptableObject
{
    [Header("Identificação")]
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;

    [Header("Visual")]
    [SerializeField] private GameObject prefab;

    [Header("Estado")]
    [SerializeField] private ItemCookState cookState = ItemCookState.Cru;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public GameObject Prefab => prefab;
    public ItemCookState CookState => cookState;
}
