using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    public Image lockImage;
    private StageInfoBase stageInfo;

    public void SetStageInfo(StageInfoBase stageInfoBase)
    {
        stageInfo = stageInfoBase;
        stageNum = stageInfoBase.stage;
        nameText.text = "Stage "+ stageInfo.act + "-" + stageInfo.stage;
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

        isStageUnlocked = GameManager.Instance.PlayerStats.IsStageUnlocked(stageInfo.idName);

        Button button = GetComponent<Button>();
        button.interactable = isStageUnlocked;
        if (isStageUnlocked)
        {
            lockImage.gameObject.SetActive(false);

            if (clearCount == 0)
                stageBackgroundColor.color = new Color(0.82f, 0.41f, 0.41f);
            else
                stageBackgroundColor.color = new Color(0.24f, 0.73f, 0.44f);
        }
        else
        {
            stageBackgroundColor.color = new Color(0.6f, 0.6f, 0.6f);
        }
    }

    public void OnClickItemButton()
    {
        if (stageInfo == null)
            return;

        PopUpWindow popUpWindow = UIManager.Instance.PopUpWindow;
        popUpWindow.OpenTextWindow("");
        popUpWindow.SetButtonValues(null, null, "Close", delegate { UIManager.Instance.CloseCurrentWindow(); });
        popUpWindow.textField.text = "";
        popUpWindow.textField.fontSize = 18;
        popUpWindow.textField.paragraphSpacing = 8;
        popUpWindow.textField.alignment = TextAlignmentOptions.Left;

        int sum = 0;

        stageInfo.archetypeDropList.ForEach(x => sum += x.weight);

        popUpWindow.textField.text += "<b>Archetype Drops</b>\n";
        foreach (WeightBase archetypeDrop in stageInfo.archetypeDropList.OrderBy(x => x.weight).ToList())
        {
            string name = LocalizationManager.Instance.GetLocalizationText_ArchetypeName(archetypeDrop.idName);
            popUpWindow.textField.text += ((float)archetypeDrop.weight/sum).ToString("p1") + "<indent=6em>" + name +"</indent>\n";
        }
    }
}