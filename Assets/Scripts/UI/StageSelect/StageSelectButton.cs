using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectButton : MonoBehaviour
{
    public int stageNum = 1;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI clearText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI waveText;
    public Image stageBackgroundColor;
    private StageInfoBase stageInfo;

    public void SetStageInfo(StageInfoBase stageInfoBase)
    {
        stageInfo = stageInfoBase;
        stageNum = stageInfoBase.stage;
        nameText.text = "Stage " + stageInfo.stage;
        levelText.text = "Lv. " + stageInfo.monsterLevel;
        waveText.text = "Waves: " + stageInfo.enemyWaves.Count;
        int clearCount = GameManager.Instance.PlayerStats.GetStageClearCount(stageInfo.idName);
        if (clearCount == 0)
            clearText.text = "";
        else if (clearCount > 1000)
            clearText.text = "Cleared: 1000+";
        else
            clearText.text = "Cleared: " + clearCount;

        bool isStageUnlocked = false;

        if (stageInfo.requiredToUnlock == stageInfo.idName)
            isStageUnlocked = true;
        else
            isStageUnlocked = GameManager.Instance.PlayerStats.IsStageUnlocked(stageInfo.requiredToUnlock);

        Button button = GetComponent<Button>();
        button.interactable = isStageUnlocked;
        if (isStageUnlocked)
        {
            if (clearCount == 0)
                stageBackgroundColor.color = new Color(0.82f,0.41f,0.41f);
            else
                stageBackgroundColor.color = new Color(0.24f, 0.73f, 0.44f);
        } else
        {
            stageBackgroundColor.color = new Color(0.6f, 0.6f, 0.6f);
        }
    }
}