using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform m_mask;

    public float Level
    {
        get => m_Level;
        set => m_Level = Mathf.Clamp(value, -1, 1);
    }

    public float Height { get; set; }

    public float m_maxTravel;
    private float m_Level;
    private Vector3 m_startingPos;

    private void Start()
    {
        m_startingPos = transform.position;
    }

    // Update is called once per frame
    public void Update()
    {
        transform.position = m_startingPos + Vector3.up * Height;
        m_mask.localPosition = Vector3.forward * 2.2f + Vector3.right * Level * m_maxTravel;
    }

}
