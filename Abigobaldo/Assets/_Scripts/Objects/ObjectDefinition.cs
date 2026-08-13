using UnityEngine;

namespace Abigobaldo.Game
{
    [CreateAssetMenu(fileName = "NewObject", menuName = "Abigobaldo/Object Definition")]
    public sealed class ObjectDefinition : ScriptableObject
    {
        [SerializeField] private string displayName;

        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
    }
}
