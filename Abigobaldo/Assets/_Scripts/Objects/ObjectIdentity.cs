using UnityEngine;

namespace Abigobaldo.Game
{
    public class ObjectIdentity : MonoBehaviour
    {
        [SerializeField] private ObjectKind kind;
        [SerializeField] private string displayName;

        public ObjectKind Kind => kind;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? kind.ToString() : displayName;
    }
}
