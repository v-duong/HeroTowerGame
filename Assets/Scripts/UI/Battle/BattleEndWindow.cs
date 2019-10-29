using TMPro;
using UnityEngine;

public class BattleEndWindow : MonoBehaviour
{
    public TextMeshProUGUI text;

    public void ShowVictoryWindow()
    {
        this.gameObject.SetActive(true);
        text.text = "Winner";
    }

    public void ShowLoseWindow()
    {
        this.gameObject.SetActive(true);
        text.text = "Lose";
    }
}