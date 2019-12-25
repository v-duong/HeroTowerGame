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

    private void OnDisable()
    {
        SaveManager.CurrentSave.SavePlayerData();
        SaveManager.Save();
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
            members[i].levelText.text = "";
            members[i].ability1Text.text = "";
            members[i].ability2Text.text = "";
            members[i].ability3Text.text = "";
            if (hero != null)
            {
                members[i].heroNameText.text = hero.Name;
                members[i].levelText.text = "Lv" + hero.Level;
                members[i].sprite.sprite = ResourceManager.Instance.GetHeroSprite(hero.spriteName);
                members[i].sprite.color = new Color(1f, 1f, 1f, 1f);

                if (hero.GetAbilityFromSlot(0) != null)
                {
                    members[i].ability1Text.text = "Ability 1\n<b>" + hero.GetAbilityFromSlot(0).abilityBase.LocalizedName + "</b>";

                    if (!hero.GetAbilityFromSlot(0).IsUsable)
                        members[i].ability1Text.text = "<color=#b00000>" + members[i].ability1Text.text.Trim('\n') + "\n(Unusable)</color>";
                }
                else
                {
                    members[i].ability1Text.text = "Ability 1\n<b>N/A</b>";
                }

                if (hero.GetAbilityFromSlot(1) != null)
                {
                    members[i].ability2Text.text = "Ability 2\n<b>" + hero.GetAbilityFromSlot(1).abilityBase.LocalizedName + "</b>";


                    if (!hero.GetAbilityFromSlot(1).IsUsable)
                        members[i].ability2Text.text = "<color=#b00000>" + members[i].ability2Text.text.Trim('\n') + "\n(Unusable)</color>";
                }
                else
                {
                    members[i].ability2Text.text = "Ability 2\n<b>N/A</b>";
                }

                if (hero.GetAbilityFromSlot(2) != null)
                {
                    members[i].ability3Text.text = "Soul Ability\n<b>" + hero.GetAbilityFromSlot(2).abilityBase.LocalizedName + "</b>";


                    if (!hero.GetAbilityFromSlot(2).IsUsable)
                        members[i].ability3Text.text = "<color=#b00000>" + members[i].ability2Text.text.Trim('\n') + "\n(Unusable)</color>";
                }
                else
                {
                    members[i].ability3Text.text = "Soul Ability\n<b>N/A</b>";
                }

            }
            else
            {
                members[i].heroNameText.text = "EMPTY";
                members[i].sprite.color = new Color(1f, 1f, 1f, 0f);
            }
        }
    }
}