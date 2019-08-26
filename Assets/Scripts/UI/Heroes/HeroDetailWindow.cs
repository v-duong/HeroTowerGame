﻿using System.Collections.Generic;
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
        infoText.text += "Experience: " + hero.Experience + "\n\n";

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
            infoText.text += string.Format("{0:F2}/s, {1:F1}%, x{2:F2}", 1f / firstSlotAbility.Cooldown, firstSlotAbility.CriticalChance, 1f + firstSlotAbility.CriticalDamage / 100f) + "\n";
            infoText.text += LocalizationManager.Instance.GetLocalizationText_AbilityCalculatedDamage(firstSlotAbility.damageBase);
        }
        if (hero.GetAbilityFromSlot(1) != null)
        {
            ActorAbility secondSlotAbility = hero.GetAbilityFromSlot(1);
            infoText.text += "Ability 2: " + secondSlotAbility.abilityBase.idName + "\n";
            infoText.text += string.Format("{0:F2}/s, {1:F1}%, x{2:F2}", 1f / secondSlotAbility.Cooldown, secondSlotAbility.CriticalChance, 1f + secondSlotAbility.CriticalDamage / 100f) + "\n";
            infoText.text += LocalizationManager.Instance.GetLocalizationText_AbilityCalculatedDamage(secondSlotAbility.damageBase);
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
        }
    }

    public void SetActiveToggle()
    {
        if (!this.gameObject.activeSelf)
            this.gameObject.SetActive(true);
        else
            this.gameObject.SetActive(false);
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
            hero.AddExperience(5000);
        UpdateWindow();
    }
}