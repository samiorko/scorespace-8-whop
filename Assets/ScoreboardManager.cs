using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScoreboardManager : MonoBehaviour
{
    public Transform m_scoreBoard;
    public RectTransform m_scoresContainer;
    public GameObject m_scoreNodePrefab;

    public static ScoreboardManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void UpdateScores()
    {
        foreach (Transform node in m_scoresContainer)
        {
            Destroy(node.gameObject);
        }

        var scores = GameManager.Instance.HighScores;

        foreach (var (levels, score) in scores)
        {
            var node = Instantiate(m_scoreNodePrefab, m_scoresContainer).GetComponent<ScoreNode>();
            node.SetScores(score, levels);
            if (score == GameManager.Instance.PreviousAddedScore && levels== GameManager.Instance.PreviousAddedLevel)
            {
                node.MarkAsLatest();
            }
        }

    }

    public bool Open => m_scoreBoard.gameObject.activeSelf;

    public void SetVisible(bool visible)
    {
        m_scoreBoard.gameObject.SetActive(visible);
    }
}
