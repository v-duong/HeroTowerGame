using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectPanel : MonoBehaviour
{
    public GameObject teamSelectParent;
    public List<TeamSelectionPanel> teamSelectionPanels;
    public Button confirmButton;

    private int selectedAct = 1;
    private DifficultyType selectedDifficulty = DifficultyType.NORMAL;
    public GameObject stageSelectButtonPrefab;
    private List<StageSelectButton> stageSelectButtonList = new List<StageSelectButton>();
    public GameObject contentContainer;
    private bool populatedList = false;
    private bool isTeamSelected = false;

    private StageInfoBase selectedStage = null;

    private void OnEnable()
    {
        selectedAct = 1;
        selectedDifficulty = DifficultyType.NORMAL;
        List<StageInfoBase> stages = ResourceManager.Instance.GetStagesByAct(selectedAct);
        if (stages.Count != 0 && !populatedList)
        {
            foreach (StageInfoBase stageInfo in stages)
            {
                StageSelectButton button = Instantiate(stageSelectButtonPrefab, contentContainer.transform).GetComponent<StageSelectButton>();
                button.SetStageInfo(stageInfo);
                button.GetComponent<Button>().onClick.AddListener(delegate { StageButtonOnClick(stageInfo); });
            }
            populatedList = true;
        }
    }

    public void StageButtonOnClick(StageInfoBase stageInfo)
    {
        confirmButton.interactable = false;
        isTeamSelected = false;
        selectedStage = stageInfo;
        UIManager.Instance.OpenWindow(teamSelectParent, false);
        for (int i = 0; i < teamSelectionPanels.Count; i++)
        {
            TeamSelectionPanel selectionPanel = teamSelectionPanels[i];
            Button button = selectionPanel.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(delegate { TeamPanelOnClick(selectionPanel); });
            selectionPanel.GetComponent<Image>().color = Color.white;

            int heroCount = 0;
            for (int j = 0; j < 5; j++)
            {
                HeroData hero = GameManager.Instance.PlayerStats.heroTeams[i][j];
                TeamSelectionHeroSlot teamSelectionHeroSlot = teamSelectionPanels[i].heroSlots[j];
                teamSelectionHeroSlot.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f);
                if (hero != null)
                {
                    teamSelectionHeroSlot.nameText.text = hero.Name;
                    teamSelectionHeroSlot.levelText.text = "Lv" + hero.Level.ToString("N0");
                    heroCount++;
                }
                else
                {
                    teamSelectionHeroSlot.nameText.text = "";
                    teamSelectionHeroSlot.levelText.text = "";
                }
            }

            button.interactable = heroCount > 0;
        }
    }

    public void TeamPanelOnClick(TeamSelectionPanel panel)
    {
        confirmButton.interactable = true;
        isTeamSelected = true;
        GameManager.Instance.selectedTeamNum = panel.teamNum;
        foreach (TeamSelectionPanel selectionPanel in teamSelectionPanels)
        {
            Image image = selectionPanel.GetComponent<Image>();
            if (selectionPanel == panel)
            {
                image.color = Helpers.SELECTION_COLOR;
                selectionPanel.heroSlots.ForEach(x => x.GetComponent<Image>().color = Color.white);
            }
            else
            {
                image.color = Color.white;
                selectionPanel.heroSlots.ForEach(x => x.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f));
            }
        }
    }

    public void TeamConfirmOnClick()
    {
        if (isTeamSelected)
            GameManager.Instance.MoveToBattle(selectedStage);
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