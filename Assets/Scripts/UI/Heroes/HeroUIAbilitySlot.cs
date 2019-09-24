using System;
using TMPro;
using UnityEngine;

public class HeroUIAbilitySlot : MonoBehaviour
{
    public AbilityBase ability;
    public IAbilitySource source;

    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI sourceText;

    [SerializeField]
    private TextMeshProUGUI equippedText;

    public TextMeshProUGUI infoText;

    public void UpdateSlot()
    {
        nameText.text = "";
        sourceText.text = "";
        equippedText.text = "";

        if (ability == null)
            return;

        nameText.text = LocalizationManager.Instance.GetLocalizationText_Ability(ability.idName)[0];
        sourceText.text = source.AbilitySourceType + " " + source.SourceName;

        Tuple<HeroData, int> equippedInfo = source.GetEquippedHeroAndSlot(ability);
        if (equippedInfo == null)
            equippedText.text = "";
        else if (equippedInfo.Item1 != null)
        {
            if (HeroDetailWindow.hero != equippedInfo.Item1)
            {
                equippedText.text += equippedInfo.Item1.Name + " ";
            }
            equippedText.text += "Slot " + (equippedInfo.Item2 + 1);
        }
    }

    public void SetSlot(AbilityBase ability, IAbilitySource source)
    {
        this.ability = ability;
        this.source = source;
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
        UIManager.Instance.CloseCurrentWindow();
    }
}