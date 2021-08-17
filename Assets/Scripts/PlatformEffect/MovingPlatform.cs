using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public static void AddTo(GameObject addToObject, int layer)
    {
        var obj = addToObject.AddComponent<MovingPlatform>();
        obj.m_speed = Mathf.Max(1, Mathf.Min(layer / 25, 10)) * .75f;
    }

    private Vector3 m_left;
    private Vector3 m_right;
    private bool m_moveLeft;
    private Vector3 m_currentVelocity;

    private Rigidbody rb;

    private float m_speed;

    private void Start()
    {
        m_left = transform.position + Vector3.left;
        m_right = transform.position + Vector3.right;

        rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void FixedUpdate()
    {
        if (m_moveLeft)
        {
            if (transform.position.x <= m_left.x + .2f)
            {
                m_moveLeft = false;
                m_currentVelocity = Vector3.zero;
                return;
            }

            rb.MovePosition(Vector3.SmoothDamp(transform.position, m_left, ref m_currentVelocity, .1f, m_speed));
        }
        else
        {
            if (transform.position.x >= m_right.x - .2f)
            {
                m_moveLeft = true;
                m_currentVelocity = Vector3.zero;
                return;
            }

            rb.MovePosition(Vector3.SmoothDamp(transform.position, m_right, ref m_currentVelocity, .1f, m_speed));
        }
    }

}
