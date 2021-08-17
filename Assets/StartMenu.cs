using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StartMenu : MonoBehaviour
{
    public TextMesh m_score;
    public TextMesh m_levels;
    public GameObject m_newHighScore;

    void Start()
    {
        GameManager.Instance.OnGameLost.AddListener(UpdateScores);
    }

    void UpdateScores()
    {
        m_newHighScore.SetActive(false);
        m_score.gameObject.SetActive(false);
        m_levels.gameObject.SetActive(false);

        var score = GameManager.Instance.PreviousAddedScore;
        var levels = GameManager.Instance.PreviousAddedLevel;
        if (score > 0)
        {
            m_score.text = $"Score: {score}";
            m_levels.text = $"Levels: {levels}";
            m_score.gameObject.SetActive(true);
            m_levels.gameObject.SetActive(true);
        }

        var highScore = GameManager.Instance.HighScores.FirstOrDefault();
        if (score > 0 && score == highScore.score && levels == highScore.levels)
        {
            m_newHighScore.SetActive(true);
        }
    }
}
