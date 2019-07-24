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
            infoText.text += "Ability 1: " + hero.GetAbilityFromSlot(0).abilityBase.idName + "\n";
            infoText.text += LocalizationManager.Instance.GetLocalizationText_AbilityCalculatedDamage(hero.GetAbilityFromSlot(0).damageBase);
        }
        if (hero.GetAbilityFromSlot(1) != null)
        {
            infoText.text += "Ability 2: " + hero.GetAbilityFromSlot(1).abilityBase.idName + "\n";
            infoText.text += LocalizationManager.Instance.GetLocalizationText_AbilityCalculatedDamage(hero.GetAbilityFromSlot(1).damageBase);
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
        treeWindow.OpenPrimaryTree();
        if (treeWindow.hero != hero)
            treeWindow.InitializeTree(hero);
        
    }

    public void ClickSecondaryTree()
    {
        treeWindow.OpenSecondaryTree();
        if (treeWindow.hero != hero)
            treeWindow.InitializeTree(hero);
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