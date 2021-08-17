using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanePositioner : MonoBehaviour
{
    void Update()
    {
        if (!Mathf.Approximately(transform.position.z, 0f))
        {
            transform.position = new Vector3
            {
                x = transform.position.x,
                z = 0f,
                y = transform.position.y
            };
        }
    }
}
