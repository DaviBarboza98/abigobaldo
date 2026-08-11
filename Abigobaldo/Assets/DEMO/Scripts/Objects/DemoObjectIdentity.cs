using UnityEngine;

namespace Abigobaldo.Demo
{
    public class DemoObjectIdentity : MonoBehaviour
    {
        [SerializeField] private DemoObjectKind kind;
        [SerializeField] private string displayName;

        public DemoObjectKind Kind => kind;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? kind.ToString() : displayName;
    }
}
