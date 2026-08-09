using UnityEngine;

[CreateAssetMenu(
    fileName = "NovoObjeto",
    menuName = "Abigobaldos/Objeto Data"
)]
public class ItemData : ScriptableObject
{
    [Header("Identificacao")]
    [SerializeField] private string itemId;
    [SerializeField] private string displayName;

    [Header("Prefab")]
    [SerializeField] private GameObject prefab;

    [Header("Estado")]
    [SerializeField] private ItemCookState cookState = ItemCookState.Cru;

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public GameObject Prefab => prefab;
    public ItemCookState CookState => cookState;
}
