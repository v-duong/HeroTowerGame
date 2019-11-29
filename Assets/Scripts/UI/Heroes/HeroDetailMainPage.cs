using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroDetailMainPage : MonoBehaviour, IUpdatablePanel
{
    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI infoText;

    [SerializeField]
    private ScrollRect abilityScrollRect;

    [SerializeField]
    private HeroAbilityScrollWindow abilityWindow;

    private HeroData hero;
    public Button lockButton;

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

        infoText.text = "";
        infoText.text += "Archetype: " + LocalizationManager.Instance.GetLocalizationText_ArchetypeName(hero.PrimaryArchetype.Base.idName);
        if (hero.SecondaryArchetype != null)
        {
            infoText.text += "/" + LocalizationManager.Instance.GetLocalizationText_ArchetypeName(hero.SecondaryArchetype.Base.idName) + "\n";
        }
        else
        {
            infoText.text += "\n";
        }
        infoText.text += "Level: " + hero.Level + "\n";
        infoText.text += "Experience: " + hero.Experience + "\n";
        infoText.text += "AP: " + hero.ArchetypePoints + "\n\n";

        infoText.text += "Health: " + hero.MaximumHealth + "\n";
        infoText.text += "Shield: " + hero.MaximumManaShield + "\n";
        infoText.text += "Soul Points: " + hero.MaximumSoulPoints + "\n\n";

        infoText.text += "Strength: " + hero.Strength + "\n";
        infoText.text += "Intelligence: " + hero.Intelligence + "\n";
        infoText.text += "Agility: " + hero.Agility + "\n";
        infoText.text += "Will: " + hero.Will + "\n\n";
        infoText.text += "Armor: " + hero.Armor + "\n";
        infoText.text += "Dodge Rating: " + hero.DodgeRating + "\n";
        infoText.text += "Resolve: " + hero.ResolveRating + "\n\n";

        if (hero.GetAbilityFromSlot(0) != null)
        {
            ActorAbility firstSlotAbility = hero.GetAbilityFromSlot(0);
            infoText.text += "Ability 1: " + firstSlotAbility.abilityBase.idName + "\n";
            infoText.text += GetAbilityDetailString(firstSlotAbility);
        }
        if (hero.GetAbilityFromSlot(1) != null)
        {
            ActorAbility secondSlotAbility = hero.GetAbilityFromSlot(1);
            infoText.text += "Ability 2: " + secondSlotAbility.abilityBase.idName + "\n";
            infoText.text += GetAbilityDetailString(secondSlotAbility);
        }

    }

    private string GetAbilityDetailString(ActorAbility ability)
    {
        string s = "";
        if (ability.IsUsable)
        {
            if (ability.abilityBase.abilityType == AbilityType.AURA || ability.abilityBase.abilityType == AbilityType.SELF_BUFF)
            {
            }
            else
            {
                float dps;
                if (ability.DualWielding && ability.AlternatesAttacks)
                    dps = (ability.GetApproxDPS(false) + ability.GetApproxDPS(true)) / 2f;
                else
                    dps = ability.GetApproxDPS(false);

                s += string.Format("Approx. DPS: {0:n1}\n", dps);

                if (ability.abilityBase.abilityType != AbilityType.AURA && ability.abilityBase.abilityType != AbilityType.SELF_BUFF)
                {
                    if (ability.abilityBase.abilityType == AbilityType.ATTACK)
                        s += string.Format("Attack Rate: {0:F2}/s\n", 1f / ability.Cooldown);
                    else
                        s += string.Format("Cast Rate: {0:F2}/s\n", 1f / ability.Cooldown);

                    s += string.Format("{0:F1}%, x{1:F2}\n", ability.MainCriticalChance, ability.MainCriticalDamage);
                    s += LocalizationManager.Instance.GetLocalizationText_AbilityCalculatedDamage(ability.mainDamageBase);
                    if (ability.DualWielding && ability.AlternatesAttacks)
                    {
                        s += string.Format("{0:F1}%, x{1:F2}\n", ability.OffhandCriticalChance, ability.OffhandCriticalDamage);
                        s += LocalizationManager.Instance.GetLocalizationText_AbilityCalculatedDamage(ability.offhandDamageBase);
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
        UIManager.Instance.OpenWindow(abilityScrollRect.gameObject);
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
