using UnityEngine;

[CreateAssetMenu(
    fileName = "NewObject",
    menuName = "Abigobaldos/Object Data"
)]
public class ObjectData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string objectId;
    [SerializeField] private string displayName;

    [Header("Prefab")]
    [SerializeField] private GameObject prefab;

    [Header("State")]
    [SerializeField] private ObjectCookState cookState = ObjectCookState.Raw;

    public string ObjectId => objectId;
    public string DisplayName => displayName;
    public GameObject Prefab => prefab;
    public ObjectCookState CookState => cookState;
}

