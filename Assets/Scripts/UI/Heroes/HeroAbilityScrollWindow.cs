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

        foreach (AbilityBase ability in hero.PrimaryArchetype.AvailableAbilityList)
        {
            string str = LocalizationManager.Instance.GetLocalizationText_AbilityBaseDamage(hero.PrimaryArchetype.GetAbilityLevel(), ability);
            AddAbilitySlot(ability, hero.PrimaryArchetype, str);
        }
        if (hero.SecondaryArchetype != null)
        {
            foreach (AbilityBase ability in hero.SecondaryArchetype.AvailableAbilityList)
            {
                string str = LocalizationManager.Instance.GetLocalizationText_AbilityBaseDamage(hero.SecondaryArchetype.GetAbilityLevel(), ability);
                AddAbilitySlot(ability, hero.SecondaryArchetype, str);
            }
        }
    }

    public void AddAbilitySlot(AbilityBase ability, HeroArchetypeData archetypeData, string info)
    {
        HeroUIAbilitySlot slot;
        if (AvailableSlots.Count > 0)
        {
            slot = AvailableSlots.Pop();
        }
        else
        {
            slot = Instantiate(SlotPrefab, this.transform);
        }
        slot.gameObject.SetActive(true);
        SlotsInUse.Push(slot);
        slot.infoText.text = info;
        slot.SetSlot(ability, HeroDetailWindow.hero, archetypeData);
    }
}