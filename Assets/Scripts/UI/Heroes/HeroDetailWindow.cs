using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroDetailWindow : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI infoText;

    [SerializeField]
    private ArchetypeUITreeWindow treeWindow;

    [SerializeField]
    private ScrollRect abilityScrollRect;

    [SerializeField]
    private HeroAbilityScrollWindow abilityWindow;

    public static HeroData hero;

    public List<HeroEquipmentSlot> equipSlots;
    public HeroEquipmentSlot offHandSlot;
    public Button lockButton;

    public void OnEnable()
    {
        if (hero != null)
            UpdateWindow();
    }

    public void UpdateWindow()
    {
        nameText.text = hero.Name;

        if (hero.IsLocked)
        {
            lockButton.GetComponentInChildren<Text>().text = "Locked";
        }
        else
        {
            lockButton.GetComponentInChildren<Text>().text = "Unlocked";
        }

        infoText.text = "";
        infoText.text += "Archetype: " + hero.PrimaryArchetype.Base.idName;
        if (hero.SecondaryArchetype != null)
        {
            infoText.text += "/" + hero.SecondaryArchetype.Base.idName + "\n";
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

        foreach (HeroEquipmentSlot slot in equipSlots)
        {
            Equipment e = hero.GetEquipmentInSlot(slot.EquipSlot);
            if (e == null)
            {
                slot.slotText.text = slot.EquipSlot.ToString();
            }
            else
            {
                slot.slotText.text = e.Name;
            }
            if (slot.EquipSlot == EquipSlotType.WEAPON)
                if (e is Weapon && e.GetGroupTypes().Contains(GroupType.TWO_HANDED_WEAPON))
                {
                    offHandSlot.GetComponent<Button>().interactable = false;
                }
                else
                {
                    offHandSlot.GetComponent<Button>().interactable = true;
                }
        }
    }

    private string GetAbilityDetailString(ActorAbility ability)
    {
        string s = "";
        if (ability.IsUsable)
        {
            float dps;
            if (ability.DualWielding && ability.AlternatesAttacks)
                dps = (ability.GetApproxDPS(false) + ability.GetApproxDPS(true)) / 2f;
            else
                dps = ability.GetApproxDPS(false);

            s += string.Format("Approx. DPS: {0:F1}\n", dps);

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
        else
        {
            s += "Ability unusable with current weapon type\n";
        }
        return s;
    }

    public void SetActiveToggle()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }

    public void ClickPrimaryTree()
    {
        treeWindow.OpenArchetypeTree(hero, treeWindow.hero != hero, 0);
    }

    public void ClickSecondaryTree()
    {
        treeWindow.OpenArchetypeTree(hero, treeWindow.hero != hero, 1);
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
            lockButton.GetComponentInChildren<Text>().text = "Locked";
        }
        else
        {
            hero.IsLocked = false;
            lockButton.GetComponentInChildren<Text>().text = "Unlocked";
        }
    }

    public void DebugLevelUp()
    {
        if (hero != null)
            hero.AddExperience(50000);
        UpdateWindow();
    }

    public void ItemEquip(Item item, EquipSlotType equipSlot)
    {
        if (item == null)
            hero.UnequipFromSlot(equipSlot);
        else
            hero.EquipToSlot(item as Equipment, equipSlot);
        UpdateWindow();
    }
}