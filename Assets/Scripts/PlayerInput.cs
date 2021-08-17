using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public float m_movementForce;
    public float m_movementTorque;
    public float m_jumpForce;
    public float m_maxHorizontalVelocity;
    public float m_downForce;

    public float m_maxGroundedDistance;

    public int m_maxJumpCount;

    public bool m_autoJump;

    [SerializeField]
    private bool m_grounded;

    private float m_xAxis;
    private float m_yAxis;
    private bool m_jump;

    private int m_jumpCount = 0;

    private bool m_hanging;
    public float[] m_jumpDiminishingReturns;

    public float m_backFlipTime;

    public float m_hangPushDownForce;
    private float m_hangTime;

    private bool m_didJump;

    private bool m_backFlipping;

    private void Update()
    {
        if (!GameManager.Instance.ControlsEnabled) return;

        var rb = GetComponent<Rigidbody>();
        m_grounded = GetGrounded();

        if (m_didJump && !m_grounded)
        {
            m_didJump = false;
        }

        if (m_grounded && !m_didJump)
        {
            m_jumpCount = 0;
        }

        if (m_grounded)
        {
            m_hanging = false;
            m_hangTime = 0f;
        }
        else
        {
            if (rb.velocity.sqrMagnitude < .01f)
            {
                m_hangTime += Time.deltaTime;
            }

            //if (!m_backFlipping && m_yAxis > .1f)
            //{
            //    StopCoroutine(nameof(BackFlip));
            //    StartCoroutine(nameof(BackFlip));
            //}
        }
    }

    private IEnumerator BackFlip()
    {
        m_backFlipping = true;

        float time = 0f, t;


        var rb = GetComponent<Rigidbody>();
        var constraints = rb.constraints;
        rb.constraints &= ~RigidbodyConstraints.FreezeRotationX;
        do
        {
            time += Time.deltaTime;
            t = Mathf.Clamp01(time / m_backFlipTime);
            transform.rotation = Quaternion.Euler(new Vector3(180f + 360f * t, transform.rotation.y, transform.rotation.z));
            yield return null;
        } while (t < 1f);

        rb.constraints = constraints;
        m_backFlipping = false;
    }

    private bool GetGrounded()
    {
        return 
            Physics.Raycast(transform.position, Vector3.down, m_maxGroundedDistance)
            || Physics.Raycast(transform.position + (Vector3.right * .3f), Vector3.down, m_maxGroundedDistance)
            || Physics.Raycast(transform.position + (Vector3.left * .3f), Vector3.down, m_maxGroundedDistance);
    }

    private void LateUpdate()
    {
        if (!GameManager.Instance.ControlsEnabled) return;

        m_xAxis = Input.GetAxis("Horizontal");
        m_yAxis = Input.GetAxisRaw("Vertical");
        m_jump = m_jump || m_autoJump ? Input.GetButton("Jump") : Input.GetButtonDown("Jump");
    }

    private void FixedUpdate()
    {
        if (!GameManager.Instance.ControlsEnabled) return;

        var rb = GetComponent<Rigidbody>();
        if (!Mathf.Approximately(m_xAxis, 0) || m_jump)
        {
            rb.AddTorque(Vector3.back * m_xAxis * m_movementTorque);
            rb.AddForce(Vector3.right * m_xAxis * m_movementForce);
        }

        if (m_hanging || !m_grounded && m_hangTime > .1f)
        {
            m_hanging = true;
            rb.AddForce(Vector3.down * m_hangPushDownForce);
        }

        if (m_jump && m_jumpCount < m_maxJumpCount)
        {
            GetComponent<AudioSource>().Play();
            m_jumpCount++;
            m_didJump = true;

            var jumpForce = Vector3.up * m_jumpForce;

            var index = m_jumpCount - 1;
            var multiplier = m_jumpDiminishingReturns.Length >= m_jumpCount 
                ? m_jumpDiminishingReturns[index] 
                : m_jumpDiminishingReturns[m_jumpDiminishingReturns.Length - 1];

            jumpForce *= multiplier;

            var counterForce = Vector3.Project(rb.velocity, Vector3.down) * -1;

            if (Vector3.Dot(counterForce, rb.velocity) > 0)
            {
                counterForce = Vector3.zero;
            }

            jumpForce += counterForce;

            rb.AddForce(jumpForce, ForceMode.Impulse);
        }

        if (m_yAxis < 0f)
        {
            var force = Vector3.down * m_downForce * Mathf.Abs(m_yAxis);
            rb.AddForce(force, ForceMode.VelocityChange);
        }

        rb.velocity = new Vector3
        {
            x = Mathf.Sign(rb.velocity.x) * Mathf.Clamp(Mathf.Abs(rb.velocity.x), 0, m_maxHorizontalVelocity),
            y = rb.velocity.y,
        };

        m_jump = false;
    }
}
