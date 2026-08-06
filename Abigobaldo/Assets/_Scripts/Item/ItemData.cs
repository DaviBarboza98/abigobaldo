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

    public string ItemId => itemId;
    public string DisplayName => displayName;
    public GameObject Prefab => prefab;
}