using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Abigobaldo/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Informações")]
    public string itemName;

    [TextArea]
    public string description;

    [Header("Prefab do Item")]
    [SerializeField] private GameObject prefab;

    [Header("Propriedades")]
    public bool canBeHeld = true;
    public bool canBeThrown = true;

    public GameObject Prefab => prefab;
}