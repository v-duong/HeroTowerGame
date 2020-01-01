using System;
using TMPro;
using UnityEngine;

public class HeroUIAbilitySlot : MonoBehaviour
{
    public AbilityBase ability;
    public IAbilitySource source;
    public int abilityLevel;

    [SerializeField]
    public TextMeshProUGUI nameText;

    [SerializeField]
    public TextMeshProUGUI sourceText;

    [SerializeField]
    public TextMeshProUGUI equippedText;

    [SerializeField]
    public TextMeshProUGUI abilityText;

    [SerializeField]
    public TextMeshProUGUI targetText;

    public TextMeshProUGUI infoText;

    public void ClearSlot()
    {
        nameText.text = "";
        sourceText.text = "";
        equippedText.text = "";
        abilityText.text = "";
        targetText.text = "";
        infoText.text = "";
    }

    public void UpdateSlot()
    {
        ClearSlot();
        if (ability == null)
        {
            nameText.text = "Remove Ability";
            return;
        }

        if (source != null)
        {
            sourceText.text = "From " + LocalizationManager.Instance.GetLocalizationText(source.AbilitySourceType) + " " + source.SourceName;

            Tuple<HeroData, int> equippedInfo = source.GetEquippedHeroAndSlot(ability);
            if (equippedInfo == null)
                equippedText.text = "";
            else if (equippedInfo.Item1 != null)
            {
                equippedText.text = "Currently Equipped to ";
                if (HeroDetailWindow.hero != equippedInfo.Item1)
                {
                    equippedText.text += equippedInfo.Item1.Name + " ";
                }
                equippedText.text += "Slot " + (equippedInfo.Item2 + 1);
            }
        }
        string restrictionString = "Requires ";
        bool hasRestriction = false;
        foreach (GroupType groupType in ability.requiredRestrictions)
        {
            hasRestriction = true;
            restrictionString += LocalizationManager.Instance.GetLocalizationText(groupType) + " + ";
        }
        foreach (GroupType groupType in ability.singleRequireRestrictions)
        {
            hasRestriction = true;
            restrictionString += LocalizationManager.Instance.GetLocalizationText(groupType) + " / ";
        }
        if (hasRestriction)
        {
            restrictionString = restrictionString.Trim(' ', '/', '+');
            infoText.text = restrictionString + '\n';
        }
        infoText.text += ability.LocalizationStrings[1] + "\n";
        infoText.text += LocalizationManager.Instance.GetLocalizationText_AbilityBaseDamage(abilityLevel, ability) + "\n";

        CommonUpdate(true);
    }

    public void CommonUpdate(bool getBonusTexts)
    {
        nameText.text = "Lv" + abilityLevel + " " + ability.LocalizedName;
        abilityText.text = LocalizationManager.Instance.GetLocalizationText("abilityType." + ability.abilityType);

        if (ability.abilityType != AbilityType.AURA && ability.abilityType != AbilityType.SELF_BUFF)
            abilityText.text = LocalizationManager.Instance.GetLocalizationText("abilityShotType." + ability.abilityShotType) + " " + abilityText.text;

        targetText.text = LocalizationManager.Instance.GetLocalizationText("targetType." + ability.targetType);

        if (getBonusTexts)
            infoText.text += ability.GetAbilityBonusTexts(abilityLevel);
    }

    public void SetSlot(AbilityBase ability, IAbilitySource source, int level)
    {
        this.ability = ability;
        this.source = source;
        abilityLevel = level;
        UpdateSlot();
    }

    public void OnSlotClick()
    {
        HeroData hero = HeroDetailWindow.hero;
        int slot = HeroAbilityScrollWindow.slot;
        if (ability == null)
        {
            hero.UnequipAbility(slot);
        }
        else
        {
            hero.EquipAbility(ability, slot, source);
        }

        UIManager.Instance.HeroDetailWindow.UpdateCurrentPanel();

        UIManager.Instance.CloseCurrentWindow();
    }
}