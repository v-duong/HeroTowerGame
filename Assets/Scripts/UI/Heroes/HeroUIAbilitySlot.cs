using System;
using TMPro;
using UnityEngine;

public class HeroUIAbilitySlot : MonoBehaviour
{
    public AbilityBase ability;
    public IAbilitySource source;
    public int abilityLevel;

    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI sourceText;

    [SerializeField]
    private TextMeshProUGUI equippedText;

    [SerializeField]
    private TextMeshProUGUI abilityText;

    [SerializeField]
    private TextMeshProUGUI targetText;

    public TextMeshProUGUI infoText;

    public void UpdateSlot()
    {
        nameText.text = "";
        sourceText.text = "";
        equippedText.text = "";
        abilityText.text = "";
        targetText.text = "";

        if (ability == null)
        {
            nameText.text = "Remove Ability";
            return;
        }

        nameText.text = LocalizationManager.Instance.GetLocalizationText_Ability(ability.idName)[0];
        sourceText.text = "From " + source.AbilitySourceType + " " + source.SourceName;
        abilityText.text = LocalizationManager.Instance.GetLocalizationText("abilityType." + ability.abilityType);

        if (ability.abilityType != AbilityType.AURA && ability.abilityType != AbilityType.SELF_BUFF)
            abilityText.text = LocalizationManager.Instance.GetLocalizationText("abilityShotType." + ability.abilityShotType) + " " + abilityText.text;

        targetText.text = LocalizationManager.Instance.GetLocalizationText("targetType." + ability.targetType);

        infoText.text = LocalizationManager.Instance.GetLocalizationText_AbilityBaseDamage(abilityLevel, ability) + "\n";

        foreach (AbilityScalingBonusProperty bonusProperty in ability.bonusProperties)
        {
            infoText.text += "○ " + LocalizationManager.Instance.GetLocalizationText_BonusType(bonusProperty.bonusType,
                                                                                        bonusProperty.modifyType,
                                                                                        bonusProperty.initialValue + bonusProperty.growthValue * abilityLevel,
                                                                                        bonusProperty.restriction);
        }

        foreach (AbilityScalingAddedEffect appliedEffect in ability.appliedEffects)
        {
            if (appliedEffect.effectType == EffectType.BUFF || appliedEffect.effectType == EffectType.DEBUFF)
            {
                infoText.text += "○ " + LocalizationManager.Instance.GetLocalizationText_BonusType(appliedEffect.bonusType,
                                                                             appliedEffect.modifyType,
                                                                             appliedEffect.initialValue + appliedEffect.growthValue * abilityLevel,
                                                                             GroupType.NO_GROUP);
            }
        }

        foreach (var x in ability.triggeredEffects)
        {
            infoText.text += LocalizationManager.Instance.GetLocalizationText_TriggeredEffect(x, x.effectMaxValue);
        }

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
            hero.UpdateActorData();
        }
        else
        {
            hero.EquipAbility(ability, slot, source);
        }

        UIManager.Instance.HeroDetailWindow.UpdateCurrentPanel();

        UIManager.Instance.CloseCurrentWindow();
    }
}