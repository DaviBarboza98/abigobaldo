using UnityEngine;

[CreateAssetMenu(
    fileName = "Item",
    menuName = "Abigobaldo's Kitchen/Item"
)]
public class ItemData : ScriptableObject
{
    [Header("General")]
    public string ItemName;

    [TextArea]
    public string Description;

    public Sprite Icon;

    public GameObject Prefab;

    public ItemType ItemType;

    [Header("Properties")]
    public bool Holdable = true;
    public bool Stackable = false;

    [Header("Cooking")]

    public bool CanCook;

    public bool CanBlend;

    public bool CanPlate;

    public bool CanTrash;
}