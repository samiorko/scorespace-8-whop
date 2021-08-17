using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScrollingText : MonoBehaviour
{
    public float m_movementTime;

    public float m_startScale;
    private bool m_left;
    private void Start()
    {
        m_left = GameManager.Instance.PlayerOnLeftSideAbsolute;
        StartCoroutine(nameof(MoveCoroutine));
    }

    private IEnumerator MoveCoroutine()
    {
        var passedTime = 0f;
        var text = GetComponentInChildren<TextMeshProUGUI>();
        var startPos = transform.position + Vector3.up;
        var targetPos = transform.position + (Vector3.up + (m_left ? Vector3.left : Vector3.right)) * 2;

        var startScale = m_startScale * Vector3.one;

        float t;
        do
        {
            passedTime += Time.deltaTime;
            t = passedTime / m_movementTime;

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t * t);
            //transform.position += (transform.TransformDirection(Vector3.up + (m_left ? Vector3.left : Vector3.right)) + Vector3.up) * Time.deltaTime * m_movementSpeed;
            //transform.Rotate((m_left ? Vector3.forward : Vector3.back) * m_rotationAmount * Time.deltaTime, Space.Self);

            text.color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), t);
            yield return null;
        } while (t < 1f);

        Destroy(transform.parent.gameObject);
    }

}
