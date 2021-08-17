using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    private HashSet<GameObject> m_scoredPlatforms = new HashSet<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.CompareTag("Platform") || m_scoredPlatforms.Contains(other.gameObject)) return;

        var platformController = other.transform.parent.gameObject.GetComponent<PlatformControl>();
        var level = platformController.Level;

        m_scoredPlatforms.Add(other.gameObject);
        var multiplier = GameManager.Instance.PlayerOnLeftSide ? 1 : -1;
        GameManager.Instance.AddScore(10f);
        GameManager.Instance.HighestLevel = level;
        GameManager.Instance.NudgeLevel(.075f * multiplier);
    }
}
