using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroAbilityPage : MonoBehaviour, IUpdatablePanel
{
    [SerializeField]
    private HeroUIAbilitySlot abilitySlot;

    [SerializeField]
    private TextMeshProUGUI mainText;

    [SerializeField]
    private TextMeshProUGUI offText;

    [SerializeField]
    private List<HeroAbilityPageSelector> buttons;

    [SerializeField]
    private ScrollRect abilityScrollRect;

    [SerializeField]
    private HeroAbilityScrollWindow abilityScrollWindow;

    private int abilitySelected = 0;

    private HeroData hero;

    private void OnEnable()
    {
        abilitySelected = 0;
        UpdateWindow();
    }

    public void UpdateWindow()
    {
        hero = HeroDetailWindow.hero;
        for (int i = 0; i < buttons.Count; i++)
        {
            HeroAbilityPageSelector button = buttons[i];
            if (i == abilitySelected)
            {
                button.GetComponent<Button>().image.color = Helpers.SELECTION_COLOR;
            }
            else
            {
                button.GetComponent<Button>().image.color = Color.white;
            }

            ActorAbility buttonAbility;

            buttonAbility = hero.GetAbilityFromSlot(i);

            if (buttonAbility != null)
            {
                float dps;
                if (buttonAbility.DualWielding && buttonAbility.AlternatesAttacks)
                    dps = (buttonAbility.GetApproxDPS(false) + buttonAbility.GetApproxDPS(true)) / 2f;
                else
                    dps = buttonAbility.GetApproxDPS(false);

                button.infoText.text = buttonAbility.abilityBase.LocalizedName;
                if (buttonAbility.abilityBase.abilityType < AbilityType.AURA && !buttonAbility.abilityBase.isSoulAbility)
                    button.infoText.text += "\nDPS: " + dps.ToString("N1");
            }
            else
            {
                button.infoText.text = "No Ability";
            }
        }

        abilitySlot.nameText.text = "No Ability";
        abilitySlot.infoText.text = "Click to select an ability";
        abilitySlot.targetText.text = "";
        abilitySlot.abilityText.text = "";
        mainText.text = "";
        offText.text = "";

        ActorAbility ability = hero.GetAbilityFromSlot(abilitySelected);

        if (ability == null)
            return;

        abilitySlot.abilityLevel = hero.GetAbilityLevel(ability.abilityBase);

        if (abilitySelected == 0 || abilitySelected == 1)
        {
            abilitySlot.infoText.text = GetAbilityDetailString(ability, false);

            if (ability.abilityBase.abilityType != AbilityType.AURA && ability.abilityBase.abilityType != AbilityType.SELF_BUFF)
            {
                if (abilitySelected == 1)
                    abilitySlot.infoText.text += "<size=90%>2nd Slot Penalty\nx0.75 Ability Speed, x0.66 Damage</size>\n";

                mainText.text = GetMainHandText(ability);

                if (ability.DualWielding && ability.AlternatesAttacks)
                {
                    offText.text = GetOffHandText(ability);
                }
            }

            abilitySlot.ability = ability.abilityBase;

            if (ability.abilityBase.abilityType == AbilityType.AURA || ability.abilityBase.abilityType == AbilityType.SELF_BUFF || ability.abilityBase.abilityType == AbilityType.AREA_BUFF)
            {
                abilitySlot.CommonUpdate(false);
            } else
            {
                abilitySlot.infoText.text += '\n';
                abilitySlot.CommonUpdate(true);
                abilitySlot.infoText.text += '\n';
            }

        }
        else
        {
            abilitySlot.ability = ability.abilityBase;

            abilitySlot.infoText.text = GetAbilityDetailString(ability, false);
            if (ability.abilityBase.abilityType == AbilityType.AURA || ability.abilityBase.abilityType == AbilityType.SELF_BUFF || ability.abilityBase.abilityType == AbilityType.AREA_BUFF)
            {
                abilitySlot.CommonUpdate(false);
            } else
            {
                abilitySlot.infoText.text += '\n';
                abilitySlot.CommonUpdate(true);
                abilitySlot.infoText.text += '\n';
            }

            if (ability.abilityBase.abilityType != AbilityType.AURA && ability.abilityBase.abilityType != AbilityType.SELF_BUFF && ability.abilityBase.abilityType != AbilityType.AREA_BUFF)
            {
                mainText.text = GetMainHandText(ability);

                if (ability.DualWielding && ability.AlternatesAttacks)
                {
                    offText.text = GetOffHandText(ability);
                }
            }
        }
    }

    public void ClickAbilitySlot(int slot)
    {
        HeroAbilityScrollWindow.slot = abilitySelected;
        UIManager.Instance.OpenWindow(abilityScrollRect.gameObject, false);
    }

    public void ClickAbilitySelector(int slot)
    {
        abilitySelected = slot;
        UpdateWindow();
    }

    private string GetAbilityDetailString(ActorAbility ability, bool shortForm)
    {
        string s = "";
        if (ability.IsUsable)
        {
            s += string.Format("Target Range: <b>{0:F2} units</b>\n", ability.TargetRange);

            if (ability.abilityBase.abilityType == AbilityType.AURA || ability.abilityBase.abilityType == AbilityType.SELF_BUFF)
            {
                s += ability.GetAuraBuffString();
            }
            else
            {
                float dps;
                if (ability.DualWielding && ability.AlternatesAttacks)
                    dps = (ability.GetApproxDPS(false) + ability.GetApproxDPS(true)) / 2f;
                else
                    dps = ability.GetApproxDPS(false);

                s += string.Format("Approx. DPS: <b>{0:n1}</b>\n", dps);

                if (ability.abilityBase.abilityType != AbilityType.AURA && ability.abilityBase.abilityType != AbilityType.SELF_BUFF)
                {
                    if (ability.abilityBase.abilityType == AbilityType.ATTACK)
                        s += string.Format("Attack Rate: <b>{0:F2}/s</b>\n", 1f / ability.Cooldown);
                    else
                        s += string.Format("Cast Rate: <b>{0:F2}/s</b>\n", 1f / ability.Cooldown);
                }
            }
        }
        else
        {
            s += "Ability unusable with current weapon type\n";
        }
        return s;
    }

    public string GetMainHandText(ActorAbility ability)
    {
        string s = "";
        if (ability.IsUsable)
        {
            if (ability.abilityBase.abilityType == AbilityType.ATTACK)
                s += "<b>Main Hand</b>\n";
            s += string.Format("Crit. Chance: <b>{0:F1}%</b>\nCrit. Damage: <b>x{1:F2}</b>\n", ability.MainCriticalChance, ability.MainCriticalDamage);
            s += LocalizationManager.Instance.GetLocalizationText_AbilityCalculatedDamage(ability.mainDamageBase);
        }
        return s;
    }

    public string GetOffHandText(ActorAbility ability)
    {
        string s = "";
        if (ability.IsUsable)
        {
            s += string.Format("<b>Off Hand</b>\nCrit. Chance: <b>{0:F1}%</b>\nCrit. Damage: <b>x{1:F2}</b>\n", ability.OffhandCriticalChance, ability.OffhandCriticalDamage);
            s += LocalizationManager.Instance.GetLocalizationText_AbilityCalculatedDamage(ability.offhandDamageBase);
        }
        return s;
    }
}