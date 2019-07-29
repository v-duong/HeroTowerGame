using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectPanel : MonoBehaviour
{
    int selectedAct = 1;
    DifficultyType selectedDifficulty = DifficultyType.NORMAL;
    public GameObject stageSelectButtonPrefab;
    List<StageSelectButton> stageSelectButtonList = new List<StageSelectButton>();
    public GameObject contentContainer;

    private void OnEnable()
    {
        selectedAct = 1;
        selectedDifficulty = DifficultyType.NORMAL;
        List<StageInfoBase> stages = ResourceManager.Instance.GetStagesByAct(selectedAct);
        if (stages.Count != 0)
        {
            foreach(StageInfoBase stageInfo in stages)
            {
                StageSelectButton button = Instantiate(stageSelectButtonPrefab, contentContainer.transform).GetComponent<StageSelectButton>();
                button.SetStageInfo(stageInfo);
            }
        }
    }

    public int GetSelectedAct()
    {
        return selectedAct;
    }

    public DifficultyType GetSelectedDifficulty()
    {
        return selectedDifficulty;
    }
}
