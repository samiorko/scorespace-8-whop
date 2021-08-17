using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Vector3 ToGamePlane(this Vector3 vector)
    {
        return new Vector3
        {
            x = vector.x,
            y = vector.y,
            z = 0f,
        };
    }
}
