using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScrollingScore : MonoBehaviour
{
    public void SetScore(int score)
    {
        foreach (var text in gameObject.GetComponentsInChildren<TextMeshProUGUI>())
        {
            text.text = score.ToString();
        }
    }
}
