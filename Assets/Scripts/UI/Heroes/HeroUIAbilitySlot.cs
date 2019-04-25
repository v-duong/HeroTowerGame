using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HeroUIAbilitySlot : MonoBehaviour
{
    public AbilityBase ability;
    public IAbilitySource source;
    [SerializeField]
    private TextMeshProUGUI nameText;
    public TextMeshProUGUI infoText;

    public void UpdateSlot()
    {
        nameText.text = LocalizationManager.Instance.GetLocalizationText_Ability(ability.idName)[0];
    }

    public void SetSlot(AbilityBase ability, HeroData hero, IAbilitySource source)
    {
        this.ability = ability;
        this.source = source;
        UpdateSlot();
    }

    public void OnSlotClick()
    {
        HeroData hero = HeroDetailWindow.hero;
        int slot = HeroAbilityScrollWindow.slot;

        hero.EquipAbility(ability, slot, source);
        UIManager.Instance.CloseCurrentWindow();
    }
}
