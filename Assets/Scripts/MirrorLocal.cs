using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class MirrorLocal : MonoBehaviour
{
    public Transform m_parent;

    // Update is called once per frame
    void LateUpdate()
    {
        if (m_parent == null)
        {
            return;
        }

        transform.localPosition = m_parent.localPosition;
        transform.localScale = m_parent.localScale;
        transform.localRotation = m_parent.localRotation;
    }
}
