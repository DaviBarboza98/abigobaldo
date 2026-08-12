using UnityEngine;

namespace Abigobaldo.Game
{
    public class BlenderCupContent : MonoBehaviour
    {
        [SerializeField] private bool hasCrushedCorn;
        [SerializeField] private GameObject crushedCornVisual;

        private HoldableObject holdableObject;
        private Rigidbody body;
        private Transform homeParent;
        private Vector3 homeLocalPosition;
        private Quaternion homeLocalRotation;
        private Vector3 homeLocalScale;

        public bool HasCrushedCorn => hasCrushedCorn;
        public HoldableObject HoldableObject => holdableObject;
        public bool IsAttached => transform.parent == homeParent && homeParent != null;

        private void Awake()
        {
            homeParent = transform.parent;
            homeLocalPosition = transform.localPosition;
            homeLocalRotation = transform.localRotation;
            homeLocalScale = transform.localScale;
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

        public bool TryAttachHome()
        {
            if (homeParent == null)
                return false;

            CacheReferences();
            transform.SetParent(homeParent);
            transform.SetLocalPositionAndRotation(homeLocalPosition, homeLocalRotation);
            transform.localScale = homeLocalScale;

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

        public bool TryPickUpWith(Holder holder)
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
                holdableObject = GetComponent<HoldableObject>();

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
