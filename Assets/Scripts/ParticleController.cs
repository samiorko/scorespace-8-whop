using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public ParticleSystem[] m_ParticleSystems;

    public float m_maxParticleVelocity;
    public float m_particleRateOverTimeAtMaxVelocity;

    public AnimationCurve m_particleCurve;

    private void Update()
    {
        var velocity = GetComponent<Rigidbody>().velocity.sqrMagnitude;

        var particleT = Mathf.Clamp01(velocity / (m_maxParticleVelocity * m_maxParticleVelocity));

        particleT = m_particleCurve.Evaluate(particleT);

        foreach (var system in m_ParticleSystems)
        {
            var emissionModule = system.emission;
            emissionModule.rateOverTime = Mathf.Lerp(0, m_particleRateOverTimeAtMaxVelocity, particleT);
        }
    }
}
