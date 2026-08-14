using UnityEngine;

namespace Abigobaldo.Game
{
    public interface IBodyPushable
    {
        void PushFromBody(Vector3 contactPoint, Vector3 pushDirection, float moveDistance);
    }
}
