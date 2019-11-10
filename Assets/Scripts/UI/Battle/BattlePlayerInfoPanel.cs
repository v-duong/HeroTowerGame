using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattlePlayerInfoPanel : MonoBehaviour
{
    public WaveInfoPanel nextWavePanel;
    public WaveInfoPanel waveAfterPanel;
    public GameObject menuParent;
    public TextMeshProUGUI lifeText;
    public TextMeshProUGUI soulPointText;
    public float timeLeftForNextWave = 0f;
    private float oldTimescale = 1f;

    private void Start()
    {
        menuParent.SetActive(false);
    }

    private void Update()
    {
        lifeText.text = "Life: " + StageManager.Instance.BattleManager.playerHealth;
        soulPointText.text = "SP: " + StageManager.Instance.BattleManager.playerSoulpoints;
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
            nextWavePanel.waveNumText.text = "Wave " + waveNum;
            nextWavePanel.timeUntilNext = timeUntil;
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
            waveAfterPanel.waveNumText.text = "Wave " + (waveNum + 1);
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
        StageManager.Instance.BattleManager.EndBattle(false);
    }

    public void StartBattleButton()
    {
        StageManager.Instance.BattleManager.StartBattle();
    }

    public void RushWaveButton()
    {
        StageManager.Instance.BattleManager.RushNextWave();
    }
}