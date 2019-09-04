using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCoreItem : Item, IAbilitySource
{
    public AbilityBase Base { get; private set; }
    public HeroData EquippedHero { get; private set; }
    public int EquippedSlot { get; private set; }

    public AbilitySourceType AbilitySourceType => AbilitySourceType.ABILITY_CORE;
    public string SourceName => Name;

    public Guid SourceId => Id;

    protected AbilityCoreItem(AbilityBase b, string name)
    {
        Id = Guid.NewGuid();
        Base = b;
        Name = name;
    }

    public AbilityCoreItem(Guid id, string baseName, string name)
    {
        Id = Guid.NewGuid();
        Base = ResourceManager.Instance.GetAbilityBase(baseName);
        Name = name;
    }

    public static AbilityCoreItem CreateAbilityItemFromArchetype(ArchetypeItem archetypeItem, AbilityBase abilityBase)
    {
        if (!archetypeItem.Base.GetArchetypeAbilities().Contains(abilityBase))
            return null;
        else if (!GameManager.Instance.PlayerStats.ArchetypeInventory.Contains(archetypeItem))
            return null;
        else
        {
            GameManager.Instance.PlayerStats.RemoveArchetypeFromInventory(archetypeItem);
            string name = archetypeItem.Name + "'s " + LocalizationManager.Instance.GetLocalizationText_Ability(abilityBase.idName)[0];
            return new AbilityCoreItem(abilityBase, name);
        }
    }

    public override ItemType GetItemType()
    {
        return ItemType.ABILITY;
    }

    public void OnEquip(AbilityBase ability, HeroData hero, int slot)
    {
        EquippedHero = hero;
        EquippedSlot = slot;
    }

    public void OnUnequip(AbilityBase ability, HeroData hero, int slot)
    {
        EquippedHero = null;
    }

    public Tuple<HeroData, int> GetEquippedHeroAndSlot(AbilityBase ability)
    {
        if (EquippedHero == null)
            return null;
        else
            return new Tuple<HeroData, int>(EquippedHero, EquippedSlot);
    }
}
