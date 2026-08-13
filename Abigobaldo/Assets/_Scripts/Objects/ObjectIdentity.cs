using UnityEngine;

namespace Abigobaldo.Game
{
    public class ObjectIdentity : MonoBehaviour
    {
        [SerializeField] private ObjectDefinition definition;
        [HideInInspector] [SerializeField] private bool canBePlated;

        public ObjectDefinition Definition => definition;
        public bool LegacyCanBePlated => canBePlated;
        public string DisplayName => definition != null ? definition.DisplayName : name;
    }
}
