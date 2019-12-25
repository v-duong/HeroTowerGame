using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageSelectPanel : MonoBehaviour
{
    public GameObject teamSelectParent;
    public List<TeamSelectionPanel> teamSelectionPanels;
    public List<WorldSelectButton> worldButtons;
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
        selectedAct = GameManager.Instance.PlayerStats.lastPlayedWorld;
        UpdateWorldButtons();
        UpdateStageButtons();
    }

    private void UpdateWorldButtons()
    {
        foreach (WorldSelectButton worldSelectButton in worldButtons)
        {
            Button button = worldSelectButton.GetComponent<Button>();
            if (worldSelectButton.worldNum == selectedAct)
            {
                button.image.color = Helpers.SELECTION_COLOR;
            }
            else
            {
                button.image.color = Color.white;
            }

            button.interactable = GameManager.Instance.PlayerStats.IsWorldUnlocked(worldSelectButton.worldNum);
        }
    }

    private void UpdateStageButtons()
    {
        List<StageInfoBase> stages = ResourceManager.Instance.GetStagesByAct(selectedAct);

        foreach (StageSelectButton existingButton in stageSelectButtonList)
        {
            Destroy(existingButton.gameObject);
        }
        stageSelectButtonList.Clear();

        foreach (StageInfoBase stageInfo in stages)
        {
            StageSelectButton button = Instantiate(stageSelectButtonPrefab, contentContainer.transform).GetComponent<StageSelectButton>();
            button.SetStageInfo(stageInfo);
            button.GetComponent<Button>().onClick.AddListener(delegate { StageButtonOnClick(stageInfo); });
            stageSelectButtonList.Add(button);
        }
        populatedList = true;
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
                    teamSelectionHeroSlot.sprite.sprite = ResourceManager.Instance.GetHeroSprite(hero.spriteName);
                    teamSelectionHeroSlot.sprite.color = Color.white;
                    heroCount++;
                }
                else
                {
                    teamSelectionHeroSlot.nameText.text = "";
                    teamSelectionHeroSlot.levelText.text = "";
                    teamSelectionHeroSlot.sprite.color = new Color(1f,1f,1f,0f);
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
        {
            int heroesWithoutSkills = 0;
            GameManager.Instance.PlayerStats.lastPlayedWorld = selectedAct;

            foreach (HeroData hero in GameManager.Instance.PlayerStats.heroTeams[GameManager.Instance.selectedTeamNum])
            {
                if (hero == null)
                    continue;
                for (int i = 0; i < 2; i++)
                {
                    if (hero.GetAbilityFromSlot(i) != null && hero.GetAbilityFromSlot(i).IsUsable)
                    {
                        break;
                    }
                    if (i == 1)
                        heroesWithoutSkills++;
                }
            }

            if (heroesWithoutSkills > 0)
            {
                PopUpWindow popUpWindow = UIManager.Instance.PopUpWindow;
                popUpWindow.OpenTextWindow("One or more Heroes have no usable Abilities.\nContinue anyway?", 380, 200);
                popUpWindow.textField.fontSize = 24;
                popUpWindow.textField.paragraphSpacing = 0;
                popUpWindow.textField.alignment = TextAlignmentOptions.Center;

                popUpWindow.SetButtonValues("Confirm", delegate { UIManager.Instance.CloseCurrentWindow(); GameManager.Instance.MoveToBattle(selectedStage); }, "Cancel", delegate { UIManager.Instance.CloseCurrentWindow(); });
            }
            else
            {
                GameManager.Instance.MoveToBattle(selectedStage);
            }
        }
    }


    public int GetSelectedAct()
    {
        return selectedAct;
    }

    public void SetSelectedAct(int num)
    {
        if (selectedAct == num)
            return;
        selectedAct = num;
        UpdateStageButtons();
        UpdateWorldButtons();
    }

    public DifficultyType GetSelectedDifficulty()
    {
        return selectedDifficulty;
    }
}