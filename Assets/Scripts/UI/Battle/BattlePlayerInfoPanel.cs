using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattlePlayerInfoPanel : MonoBehaviour
{
    public WaveInfoPanel nextWavePanel;
    public WaveInfoPanel waveAfterPanel;
    public GameObject menuParent;
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI soulPointText;
    public List<Button> timeControlButtons;
    private float oldTimescale = 1f;

    private void Start()
    {
        menuParent.SetActive(false);
    }

    private void Update()
    {
        lifeText.text = "<sprite=11> " + StageManager.Instance.BattleManager.playerHealth;
        //soulPointText.text = "<sprite=12> " + StageManager.Instance.BattleManager.playerSoulpoints;
    }

    public void InitializeNextWaveInfo(List<EnemyWaveItem> nextWave, List<EnemyWaveItem> waveAfter, float timeUntil, int waveNum, bool isStartWave = false)
    {
        waveAfterPanel.startwaveButton.gameObject.SetActive(isStartWave);

        if (nextWave == null)
        {
            nextWavePanel.gameObject.SetActive(false);
        }
        else
        {
            nextWavePanel.gameObject.SetActive(true);
            nextWavePanel.startwaveButton.gameObject.SetActive(true);
            nextWavePanel.waveNumText.text = "Next Wave: " + waveNum;
            nextWavePanel.timeUntilNext = timeUntil;
            nextWavePanel.originalTime = timeUntil;
            nextWavePanel.timeText.text = timeUntil.ToString("N2");
            string nextWaveEnemyString = "";

            foreach (EnemyWaveItem item in nextWave)
            {
                nextWaveEnemyString += item.enemyName + ", ";
            }
            nextWaveEnemyString = nextWaveEnemyString.Trim(',', ' ');
            nextWavePanel.enemyNamesText.text = nextWaveEnemyString;
        }

        if (isStartWave)
        {
            nextWavePanel.startwaveButton.gameObject.SetActive(false);
            waveAfterPanel.gameObject.SetActive(true);
            waveAfterPanel.enemyNamesText.text = "";
            waveAfterPanel.waveNumText.text = "";
            nextWavePanel.timeText.text = "";
            return;
        }

        if (waveAfter != null)
        {
            waveAfterPanel.gameObject.SetActive(true);
            waveAfterPanel.waveNumText.text = "After Next Wave: " + (waveNum + 1);
            string waveAfterEnemyString = "";

            foreach (EnemyWaveItem item in waveAfter)
            {
                waveAfterEnemyString += item.enemyName + ", ";
            }
            waveAfterEnemyString = waveAfterEnemyString.Trim(',', ' ');
            waveAfterPanel.enemyNamesText.text = waveAfterEnemyString;
        }
        else
        {
            waveAfterPanel.gameObject.SetActive(false);
        }
    }

    public void OpenMenuButton()
    {
        oldTimescale = Time.timeScale;
        Time.timeScale = 0;
        menuParent.SetActive(true);
    }

    public void CloseMenuButton()
    {
        menuParent.SetActive(false);
        Time.timeScale = oldTimescale;
    }

    public void EndBattleButton()
    {
        menuParent.SetActive(false);
#if UNITY_EDITOR
        StageManager.Instance.BattleManager.EndBattle(true);
#else
         StageManager.Instance.BattleManager.EndBattle(false);
#endif
    }

    public void StartBattleButton()
    {
        StageManager.Instance.BattleManager.StartBattle();
    }

    public void RushWaveButton()
    {
        StageManager.Instance.BattleManager.RushNextWave();
    }

    public void SetTimescale(float value)
    {
        GameManager.SetTimescale(value);
    }

    public void SetButtonActiveColor(Button currentButton)
    {
        foreach (Button button in timeControlButtons)
        {
            button.targetGraphic.color = new Color(0.28f, 0.28f, 0.28f);
        }
        timeControlButtons.Find(x => x == currentButton).targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);

    }
}