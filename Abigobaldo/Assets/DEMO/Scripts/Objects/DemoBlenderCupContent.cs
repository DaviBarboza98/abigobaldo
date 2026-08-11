using UnityEngine;

namespace Abigobaldo.Demo
{
    public class DemoBlenderCupContent : MonoBehaviour
    {
        [SerializeField] private bool hasCrushedCorn;
        [SerializeField] private GameObject crushedCornVisual;

        private DemoHoldableObject holdableObject;
        private Rigidbody body;

        public bool HasCrushedCorn => hasCrushedCorn;
        public DemoHoldableObject HoldableObject => holdableObject;

        private void Awake()
        {
            CacheReferences();
            RefreshVisual();
        }

        public void SetCrushedCorn(bool value)
        {
            hasCrushedCorn = value;
            RefreshVisual();
        }

        public bool TryConsumeCrushedCorn()
        {
            if (!hasCrushedCorn)
                return false;

            SetCrushedCorn(false);
            return true;
        }

        public bool TryAttachTo(Transform anchor)
        {
            if (anchor == null)
                return false;

            CacheReferences();
            transform.SetParent(anchor);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (body != null)
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.useGravity = false;
                body.isKinematic = true;
                body.detectCollisions = true;
            }

            return true;
        }

        public bool TryPickUpWith(DemoHolder holder)
        {
            CacheReferences();

            if (holder == null || holdableObject == null || !holder.IsEmpty)
                return false;

            transform.SetParent(null);
            return holder.TryPickUp(holdableObject);
        }

        private void CacheReferences()
        {
            if (holdableObject == null)
                holdableObject = GetComponent<DemoHoldableObject>();

            if (body == null)
                body = GetComponent<Rigidbody>();
        }

        private void RefreshVisual()
        {
            if (crushedCornVisual != null)
                crushedCornVisual.SetActive(hasCrushedCorn);
        }
    }
}
