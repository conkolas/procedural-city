using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
    public static Vector2 ToXZ(this Vector3 v) {
        return new Vector2(v.x, v.z);
    }
}
