using TMPro;
using UnityEngine;

public class LevelText : MonoBehaviour
{
    public enum TextSide
    {
        Left,
        Right
    }

    private string m_text;
    private TextSide m_side;

    public GameObject m_leftText;
    public GameObject m_rightText;
    public GameObject m_canvas;

    public string Text
    {
        get => m_text;
        set {
            m_text = value;
            foreach (var textMesh in GetComponentsInChildren<TextMeshProUGUI>())
            {
                textMesh.text = m_text;
            }
        }
    }

    public TextSide Side
    {
        get => m_side;
        set
        {
            m_side = value;
            gameObject.layer = Side == TextSide.Left ? 10 : 20;
            m_canvas.layer = gameObject.layer;

            m_leftText.SetActive(Side == TextSide.Left);
            m_rightText.SetActive(Side == TextSide.Right);
        }
    }
}
