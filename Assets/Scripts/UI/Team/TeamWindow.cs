using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamWindow : MonoBehaviour
{
    public int selectedTeam = 0;
    private int selectedSlot = 0;
    public List<TeamMemberSlot> members;
    public List<Button> teamNumButtons;

    private void OnEnable()
    {
        selectedTeam = 0;
        for (int i = 0; i < teamNumButtons.Count; i++)
        {
            Button button = teamNumButtons[i];
            if (i == 0)
            {
                button.image.color = Helpers.SELECTION_COLOR;
            }
            else
            {
                button.image.color = Color.white;
            }
        }

        UpdateTeamSlots();
    }

    public void OnClickTeamNumber(int teamNum)
    {
        if (selectedTeam == teamNum)
            return;
        selectedTeam = teamNum;
        for (int i = 0; i < teamNumButtons.Count; i++)
        {
            Button button = teamNumButtons[i];
            if (i == teamNum)
            {
                button.image.color = Helpers.SELECTION_COLOR;
            }
            else
            {
                button.image.color = Color.white;
            }
        }
        UpdateTeamSlots();
    }

    public void OnClickHeroSelect(int slotNum)
    {
        selectedSlot = slotNum;
        UIManager.Instance.OpenHeroWindow(false);
        UIManager.Instance.HeroScrollContent.SetCallback(SetHeroToSlot);
    }

    public void SetHeroToSlot(HeroData hero)
    {
        UIManager.Instance.CloseCurrentWindow();
        GameManager.Instance.PlayerStats.SetHeroToTeamSlot(hero, selectedTeam, selectedSlot);
        members[selectedSlot].heroNameText.text = hero.Name;
        UpdateTeamSlots();
    }

    public void UpdateTeamSlots()
    {
        for (int i = 0; i < 5; i++)
        {
            HeroData hero = GameManager.Instance.PlayerStats.heroTeams[selectedTeam][i];
            if (hero != null)
            {
                members[i].heroNameText.text = hero.Name;
            }
            else
            {
                members[i].heroNameText.text = "EMPTY";
            }
        }
    }
}