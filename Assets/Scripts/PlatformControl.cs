using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlatformControl : MonoBehaviour
{
    private bool m_left;
    public float Width { get; set; } = 1f;

    public float Multiplier { get; set; } = 1f;

    public GameObject m_leftContainer;
    public GameObject m_rightContainer;

    public bool Left
    {
        get => m_left;
        set
        {
            m_left = value;
            m_leftContainer.SetActive(m_left);
            m_rightContainer.SetActive(!m_left);
        }
    }

    public Collider m_collider;

    public bool ColliderEnabled
    {
        get => m_collider.enabled;
        set => m_collider.enabled = value;
    }

    public int Level { get; set; }


    private void Start()
    {
        GameManager.Instance.Platforms.Add(this);
    }

    // Update is called once per frame
    private void Update()
    {
        if (m_left)
        {
            transform.localScale = new Vector3(Width * Multiplier, 1, 1);
        }

        //foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
        //{
        //    renderer.enabled = !Mathf.Approximately(transform.localScale.x, 0f);
        //}
    }

    private void OnDestroy()
    {
        GameManager.Instance.Platforms.Remove(this);
    }
}
