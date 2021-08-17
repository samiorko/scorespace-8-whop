using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMaskController : MonoBehaviour
{
    [Range(-1, 1)]
    public float Level;

    private void Update()
    {
        transform.localPosition = new Vector3(-8f + (Level * 8f), 0f, -5f);
    }
}
