using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleEndWindow : MonoBehaviour
{
    public Button endBattleButton;
    public Button nextLoopButton;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI bodyText;
    public bool isVictory;

    public void ShowVictoryWindow()
    {
        this.gameObject.SetActive(true);
        headerText.text = "Winner";
        bodyText.text = "";
        nextLoopButton.gameObject.SetActive(true);
    }

    public void ShowLoseWindow()
    {
        this.gameObject.SetActive(true);
        headerText.text = "Lose";
        bodyText.text = "";
        nextLoopButton.gameObject.SetActive(false);
    }

    public void AddToBodyText(string s)
    {
        bodyText.text += s;
    }

    public void LoadMainMenu()
    {
        StageManager.Instance.BattleManager.AllocateRewards(isVictory);
        GameManager.Instance.MoveToMainMenu();
    }

    public void StartNextLoop()
    {
        StageManager.Instance.BattleManager.StartSurvivalBattle();
        this.gameObject.SetActive(false);
    }
}