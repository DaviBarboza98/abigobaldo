using UnityEngine;
using UnityEngine.Rendering;

namespace Abigobaldo.Game
{
    [ExecuteAlways]
    public abstract class LightingPostEffect : MonoBehaviour
    {
        [SerializeField] private bool settingsEnabled = true;

        public bool SettingsEnabled => settingsEnabled && isActiveAndEnabled;
        internal virtual bool RequiresHdr => false;

        internal abstract void Apply(VolumeProfile profile);

        protected virtual void OnEnable()
        {
            NotifyManager();
        }

        protected virtual void OnDisable()
        {
            NotifyManager();
        }

        protected virtual void OnValidate()
        {
            NotifyManager();
        }

        protected void NotifyManager()
        {
            GetComponentInParent<LightingManager>()?.RequestApply();
        }
    }
}
