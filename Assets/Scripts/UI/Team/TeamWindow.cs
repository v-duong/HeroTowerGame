﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TeamWindow : MonoBehaviour
{
    public int selectedTeam = 0;
    private int selectedSlot = 0;
    public List<TeamMemberSlot> members;

    private void OnEnable()
    {
        selectedTeam = 0;
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
            } else
            {
                members[i].heroNameText.text = "EMPTY";
            }
        }
    }
}