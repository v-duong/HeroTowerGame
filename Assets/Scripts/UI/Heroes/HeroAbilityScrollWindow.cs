using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroAbilityScrollWindow : MonoBehaviour
{
    [SerializeField]
    private HeroUIAbilitySlot SlotPrefab;

    private Stack<HeroUIAbilitySlot> SlotsInUse = new Stack<HeroUIAbilitySlot>();
    private Stack<HeroUIAbilitySlot> AvailableSlots = new Stack<HeroUIAbilitySlot>();

    [NonSerialized]
    public static int slot;

    public void OnEnable()
    {
        InitializeAbilitySlots();
    }

    public void InitializeAbilitySlots()
    {
        HeroData hero = HeroDetailWindow.hero;

        foreach (HeroUIAbilitySlot slot in SlotsInUse)
        {
            slot.gameObject.SetActive(false);
            AvailableSlots.Push(slot);
        }
        SlotsInUse.Clear();
        AddAbilitySlot(null,null,null);


        foreach (var ability in hero.PrimaryArchetype.AvailableAbilityList)
        {
            string str = LocalizationManager.Instance.GetLocalizationText_AbilityBaseDamage(hero.GetAbilityLevel(ability.abilityBase), ability.abilityBase);
            AddAbilitySlot(ability.abilityBase, hero.PrimaryArchetype, str);
        }
        if (hero.SecondaryArchetype != null)
        {
            foreach (var ability in hero.SecondaryArchetype.AvailableAbilityList)
            {
                string str = LocalizationManager.Instance.GetLocalizationText_AbilityBaseDamage(hero.GetAbilityLevel(ability.abilityBase), ability.abilityBase);
                AddAbilitySlot(ability.abilityBase, hero.SecondaryArchetype, str);
            }
        }
        foreach(AbilityCoreItem abilityItem in GameManager.Instance.PlayerStats.AbilityInventory)
        {
            string str = LocalizationManager.Instance.GetLocalizationText_AbilityBaseDamage(hero.GetAbilityLevel(abilityItem.Base), abilityItem.Base);
            AddAbilitySlot(abilityItem.Base, abilityItem, str);
        }
    }

    public void AddAbilitySlot(AbilityBase ability, IAbilitySource abilitySource, string info)
    {
        HeroUIAbilitySlot slot;
        if (AvailableSlots.Count > 0)
        {
            slot = AvailableSlots.Pop();
        }
        else
        {
            slot = Instantiate(SlotPrefab, transform);
        }
        slot.gameObject.SetActive(true);
        SlotsInUse.Push(slot);

        slot.SetSlot(ability, abilitySource);

        if (ability == null)
        {
            slot.infoText.text = "REMOVE";
            return;
        }

        slot.infoText.text = info;
        
    }
}