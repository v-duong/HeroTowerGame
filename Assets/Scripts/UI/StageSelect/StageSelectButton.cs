using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectButton : MonoBehaviour
{
    public int stageNum = 1;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI clearText;
    private StageInfoBase stageInfo;

    public void SetStageInfo(StageInfoBase stageInfoBase)
    {
        stageInfo = stageInfoBase;
        stageNum = stageInfoBase.stage;
        nameText.text = "Stage " + stageInfo.stage;
        int clearCount = GameManager.Instance.PlayerStats.GetStageClearCount(stageInfo.idName);
        if (clearCount == 0)
            clearText.text = "";
        else
            clearText.text = "Cleared: " + clearCount;

        if (stageInfo.requiredToUnlock == stageInfo.idName)
            return;

        Button button = GetComponent<Button>();
        Debug.Log("TEST" + stageInfo.idName);
        button.interactable = GameManager.Instance.PlayerStats.IsStageUnlocked(stageInfo.requiredToUnlock);
    }
}