using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Plane = UnityEngine.Plane;
using Vector3 = UnityEngine.Vector3;

public class CameraProjectionPlanePositioner : MonoBehaviour
{
    public Transform m_wayPoint;
    public Transform m_camera;

    private Plane m_plane;

    void Start()
    {
        m_plane = new Plane(Vector3.back, Vector3.up);
    }

    // Update is called once per frame
    void Update()
    {
        var direction = (m_wayPoint.position - m_camera.position).normalized;
        var ray = new Ray(m_camera.position, direction * 10f);

        if (m_plane.Raycast(ray, out var distance))
        {
            transform.position = m_camera.position + direction * distance;
        }
    }
}
