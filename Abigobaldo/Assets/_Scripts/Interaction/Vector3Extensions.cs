using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 FlattenY(this Vector3 value)
    {
        value.y = 0f;
        return value.sqrMagnitude > 0.0001f ? value.normalized : Vector3.forward;
    }
}
