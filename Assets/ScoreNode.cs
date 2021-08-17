using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreNode : MonoBehaviour
{
    public TextMeshProUGUI m_score;
    public TextMeshProUGUI m_level;

    public void SetScores(int score, int levels)
    {
        m_score.text = score.ToString();
        m_level.text = levels.ToString();
    }

    public void MarkAsLatest()
    {
        m_score.faceColor = Color.yellow;
        m_level.faceColor = Color.yellow;
    }
}
