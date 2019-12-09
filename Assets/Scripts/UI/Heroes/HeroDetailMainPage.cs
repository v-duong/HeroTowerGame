using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroDetailMainPage : MonoBehaviour, IUpdatablePanel
{
    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI levelText;

    [SerializeField]
    private TextMeshProUGUI expText;

    [SerializeField]
    private TextMeshProUGUI apText;

    [SerializeField]
    private Image primaryArchetypeHeader;

    [SerializeField]
    private Image secondaryArchetypeHeader;

    [SerializeField]
    private Image xpBarFill;

    [SerializeField]
    private ScrollRect abilityScrollRect;

    [SerializeField]
    private HeroAbilityScrollWindow abilityWindow;

    [SerializeField]
    private HeroStatBox healthBox;

    [SerializeField]
    private List<HeroStatBox> attributeBoxes;

    [SerializeField]
    private List<HeroStatBox> defenseBoxes;

    [SerializeField]
    private List<HeroStatBox> resistanceBoxes;

    [SerializeField]
    private HeroUIAbilitySlot primaryAbility;

    [SerializeField]
    private HeroUIAbilitySlot secondaryAbility;

    private HeroData hero;
    public Button lockButton;

    private void OnEnable()
    {
        UpdateWindow();
    }

    public void UpdateWindow()
    {
        hero = HeroDetailWindow.hero;
        nameText.text = hero.Name;

        if (hero.IsLocked)
        {
            lockButton.GetComponentInChildren<TextMeshProUGUI>().text = "Locked";
        }
        else
        {
            lockButton.GetComponentInChildren<TextMeshProUGUI>().text = "Unlocked";
        }

        primaryArchetypeHeader.GetComponentInChildren<TextMeshProUGUI>().text =hero.PrimaryArchetype.Base.LocalizedName;
        primaryArchetypeHeader.color = GetArchetypeStatColor(hero.PrimaryArchetype.Base);
        if (hero.SecondaryArchetype != null)
        {
            secondaryArchetypeHeader.gameObject.SetActive(true);
            secondaryArchetypeHeader.GetComponentInChildren<TextMeshProUGUI>().text = hero.SecondaryArchetype.Base.LocalizedName;
            secondaryArchetypeHeader.color = GetArchetypeStatColor(hero.SecondaryArchetype.Base);
        }
        else
        {
            secondaryArchetypeHeader.gameObject.SetActive(false);
        }
        levelText.text = "Level <b>" + hero.Level + "</b>";
        float requiredExp = Helpers.GetRequiredExperience(hero.Level + 1);
        float currentLevelExp = Helpers.GetRequiredExperience(hero.Level);
        expText.text = "Exp: " + hero.Experience.ToString("N0");
        if (hero.Level < 100)
        {
            expText.text += "/" + requiredExp.ToString("N0") + "\n";
            xpBarFill.fillAmount = (hero.Experience - currentLevelExp) / (requiredExp - currentLevelExp);
        }
        else
        {
            expText.text += " (MAX)";
            xpBarFill.fillAmount = 1f;
        }
        apText.text = "AP <b>" + hero.ArchetypePoints + "</b>";

        healthBox.statText.text = hero.MaximumHealth.ToString("N0");

        //infoText.text += "Soul Points: " + hero.MaximumSoulPoints + "\n\n";

        attributeBoxes[0].statText.text = hero.Strength.ToString("N0");
        attributeBoxes[1].statText.text = hero.Intelligence.ToString("N0");
        attributeBoxes[2].statText.text = hero.Agility.ToString("N0");
        attributeBoxes[3].statText.text = hero.Will.ToString("N0");
        defenseBoxes[0].statText.text = hero.Armor.ToString("N0");
        defenseBoxes[1].statText.text = hero.MaximumManaShield.ToString("N0");
        defenseBoxes[2].statText.text = hero.DodgeRating.ToString("N0");
        defenseBoxes[3].statText.text = hero.ResolveRating.ToString("N0");

        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            float resistance = hero.GetResistance(element);
            float uncapResistance = hero.GetUncapResistance(element);

            resistanceBoxes[(int)element].statText.text = resistance.ToString() + "%";

            if (uncapResistance > resistance)
            {
                resistanceBoxes[(int)element].statText.text += " (" + uncapResistance + ")";
            }
        }

        if (hero.GetAbilityFromSlot(0) != null)
        {
            ActorAbility firstSlotAbility = hero.GetAbilityFromSlot(0);
            primaryAbility.infoText.text = GetAbilityDetailString(firstSlotAbility, false);
            primaryAbility.ability = firstSlotAbility.abilityBase;
            primaryAbility.CommonUpdate();
        }
        else
        {
            primaryAbility.nameText.text = "No Ability";
            primaryAbility.infoText.text = "";
            primaryAbility.targetText.text = "";
            primaryAbility.abilityText.text = "";
        }
        if (hero.GetAbilityFromSlot(1) != null)
        {
            ActorAbility secondSlotAbility = hero.GetAbilityFromSlot(1);
            secondaryAbility.infoText.text = "";
            if (secondSlotAbility.abilityBase.abilityType != AbilityType.AURA && secondSlotAbility.abilityBase.abilityType != AbilityType.SELF_BUFF)
                secondaryAbility.infoText.text += "2nd Slot Penalty\n<size=90%>x0.75 Ability Speed, x0.66 Damage</size>\n";
            secondaryAbility.infoText.text += GetAbilityDetailString(secondSlotAbility, false);
            secondaryAbility.ability = secondSlotAbility.abilityBase;
            secondaryAbility.CommonUpdate();
        }
        else
        {
            secondaryAbility.nameText.text = "No Ability";
            secondaryAbility.infoText.text = "";
            secondaryAbility.targetText.text = "";
            secondaryAbility.abilityText.text = "";
        }
    }

    private string GetAbilityDetailString(ActorAbility ability, bool shortForm)
    {
        string s = "";
        if (ability.IsUsable)
        {
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

                    if (!shortForm)
                    {
                        if (ability.abilityBase.abilityType == AbilityType.ATTACK)
                            s += "Main Hand\n";
                        s += string.Format("Crit. Chance: <b>{0:F1}%</b>\nCrit. Damage: <b>x{1:F2}</b>\n", ability.MainCriticalChance, ability.MainCriticalDamage);
                        s += LocalizationManager.Instance.GetLocalizationText_AbilityCalculatedDamage(ability.mainDamageBase);
                        if (ability.DualWielding && ability.AlternatesAttacks)
                        {
                            s += string.Format("Off Hand\nCrit. Chance: <b>{0:F1}%</b>\nCrit. Damage: <b>x{1:F2}</b>\n", ability.OffhandCriticalChance, ability.OffhandCriticalDamage);
                            s += LocalizationManager.Instance.GetLocalizationText_AbilityCalculatedDamage(ability.offhandDamageBase);
                        }
                    }
                }
            }
        }
        else
        {
            s += "Ability unusable with current weapon type\n";
        }
        return s;
    }

    public Color GetArchetypeStatColor(ArchetypeBase archetype)
    {
        List<float> growths = new List<float>() { archetype.strengthGrowth, archetype.intelligenceGrowth, archetype.agilityGrowth, archetype.willGrowth };
        int sameCount = 0, sameGrowthIndex = 0, highestIndex = 0, secondHighestIndex = 0;
        float highest = 0, secondHighest = 0, sum = 0;
        for (int i = 0; i < growths.Count; i++)
        {
            if (growths[i] > highest)
            {
                highest = growths[i];
                highestIndex = i;
            }
            sum += growths[i];
        }

        for (int j = 0; j < growths.Count; j++)
        {
            if (j == highestIndex)
            {
                continue;
            }
            else if (growths[j] == highest)
            {
                sameCount++;
                sameGrowthIndex = j;
            }
            else if (growths[j] > secondHighest)
            {
                secondHighest = growths[j];
                secondHighestIndex = j;
            }
        }

        if (sameCount >= 2)
            return Helpers.NORMAL_COLOR;
        else if (sameCount == 1)
            return Color.Lerp(GetColorFromStatIndex(highestIndex), GetColorFromStatIndex(sameGrowthIndex), 0.5f);
        else
            return Color.Lerp(GetColorFromStatIndex(highestIndex), Color.Lerp(GetColorFromStatIndex(highestIndex), GetColorFromStatIndex(secondHighestIndex), 0.5f), 0.05f / (highest - growths[secondHighestIndex]));
    }

    public Color GetColorFromStatIndex(int index)
    {
        switch (index)
        {
            case 0:
                return Helpers.STR_ARCHETYPE_COLOR;

            case 1:
                return Helpers.INT_ARCHETYPE_COLOR;

            case 2:
                return Helpers.AGI_ARCHETYPE_COLOR;

            case 3:
                return Helpers.WILL_ARCHETYPE_COLOR;

            default:
                return Helpers.NORMAL_COLOR;
        }
    }

    public void DebugLevelUp()
    {
        if (hero != null)
            hero.AddExperience(500020);
        UpdateWindow();
    }

    public void OpenNameEdit()
    {
        UIManager.Instance.PopUpWindow.OpenTextInput(hero.Name);
        UIManager.Instance.PopUpWindow.textInput.characterLimit = 20;
        UIManager.Instance.PopUpWindow.textInput.contentType = TMP_InputField.ContentType.Alphanumeric;
        UIManager.Instance.PopUpWindow.textInput.lineType = TMP_InputField.LineType.SingleLine;
        UIManager.Instance.PopUpWindow.SetButtonValues("Confirm", delegate
        {
            UIManager.Instance.CloseCurrentWindow();
            if (!string.IsNullOrWhiteSpace(UIManager.Instance.PopUpWindow.textInput.text))
                hero.Name = UIManager.Instance.PopUpWindow.textInput.text;
            UpdateWindow();
        }, null, null);
    }

    public void ClickAbilitySlot(int slot)
    {
        HeroAbilityScrollWindow.slot = slot;
        UIManager.Instance.OpenWindow(abilityScrollRect.gameObject, false);
    }

    public void SetHeroLocked()
    {
        if (!hero.IsLocked)
        {
            hero.IsLocked = true;
            lockButton.GetComponentInChildren<TextMeshProUGUI>().text = "Locked";
        }
        else
        {
            hero.IsLocked = false;
            lockButton.GetComponentInChildren<TextMeshProUGUI>().text = "Unlocked";
        }
    }
}