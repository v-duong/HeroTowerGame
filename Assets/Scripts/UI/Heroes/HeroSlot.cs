using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroSlot : MonoBehaviour
{
    public HeroData hero;
    public Image slotImage;
    public Image levelImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI archetypeText;
    public TextMeshProUGUI ability1Text;
    public TextMeshProUGUI ability2Text;
    public TextMeshProUGUI teamText;
    public TextMeshProUGUI apText;
    public TextMeshProUGUI levelText;
    public Action<HeroData> callback;

    public void UpdateSlot()
    {
    }

    public void SetSlot(HeroData hero)
    {
        this.hero = hero;
        nameText.text = hero.Name;

        archetypeText.text =  LocalizationManager.Instance.GetLocalizationText_ArchetypeName(hero.PrimaryArchetype.Base.idName);
        if (hero.SecondaryArchetype != null)
        {
            archetypeText.text += " / " + LocalizationManager.Instance.GetLocalizationText_ArchetypeName(hero.SecondaryArchetype.Base.idName);
        }


        if (hero.GetAbilityFromSlot(0) != null)
        {
            ability1Text.text = hero.GetAbilityFromSlot(0).abilityBase.idName;

            if (!hero.GetAbilityFromSlot(0).IsUsable)
                ability1Text.text = "<color=#b00000>" + ability1Text.text + " (Unusable)</color>";
        } else
        {
            ability1Text.text = "";
        }

        if (hero.GetAbilityFromSlot(1) != null)
        {
            ability2Text.text = hero.GetAbilityFromSlot(1).abilityBase.idName;


            if (!hero.GetAbilityFromSlot(1).IsUsable)
                ability2Text.text = "<color=#b00000>" + ability2Text.text + " (Unusable)</color>";
        } else
        {
            ability2Text.text = "";
        }

        if (hero.assignedTeam != -1)
        {
            teamText.text = "Team " + (hero.assignedTeam+1);
        } else
        {
            teamText.text = "";
        }

        if (hero.ArchetypePoints > 0)
        {
            apText.text = "AP: " + hero.ArchetypePoints;
        } else
        {
            apText.text = "";
        }

        levelText.text = hero.Level.ToString();

        if (hero.Level < 30)
        {
            levelImage.color = Helpers.NORMAL_COLOR;
        } else if (hero.Level < 60)
        {
            levelImage.color = Helpers.UNCOMMON_COLOR;
        }
        else if (hero.Level < 90)
        {
            levelImage.color = Helpers.RARE_COLOR;
        } else
        {
            levelImage.color = Helpers.EPIC_COLOR;
        }

        UpdateSlot();
    }

    public void OnHeroSlotClick()
    {
        if (callback != null)
        {
            callback.Invoke(hero);
            return;
        }
        HeroDetailWindow detailWindow = UIManager.Instance.HeroDetailWindow;
        HeroDetailWindow.hero = hero;
        UIManager.Instance.OpenWindow(detailWindow.gameObject);
        detailWindow.SetCategorySelected(0);
        detailWindow.SetArchetypeCategoryNames(hero.PrimaryArchetype.Base.idName, hero.SecondaryArchetype?.Base.idName);
    }
}
