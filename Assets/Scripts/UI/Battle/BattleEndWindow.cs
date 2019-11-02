using TMPro;
using UnityEngine;

public class BattleEndWindow : MonoBehaviour
{
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI bodyText;

    public void ShowVictoryWindow()
    {
        this.gameObject.SetActive(true);
        headerText.text = "Winner";
        bodyText.text = "";
    }

    public void ShowLoseWindow()
    {
        this.gameObject.SetActive(true);
        headerText.text = "Lose";
        bodyText.text = "";
    }

    public void AddToBodyText(string s)
    {
        bodyText.text += s;
    }
}