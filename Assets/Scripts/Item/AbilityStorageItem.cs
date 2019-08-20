using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityStorageItem : Item
{
    public AbilityBase Base { get; private set; }

    protected AbilityStorageItem(AbilityBase b, string name)
    {
        Base = b;
        Name = name;
    }

    public static AbilityStorageItem CreateAbilityItemFromArchetype(ArchetypeItem archetypeItem, AbilityBase abilityBase)
    {
        if (!archetypeItem.Base.GetArchetypeAbilities().Contains(abilityBase))
            return null;
        else if (!GameManager.Instance.PlayerStats.ArchetypeInventory.Contains(archetypeItem))
            return null;
        else
        {
            GameManager.Instance.PlayerStats.RemoveArchetypeFromInventory(archetypeItem);
            string name = archetypeItem.Name + "'s " + LocalizationManager.Instance.GetLocalizationText_Ability(abilityBase.idName)[0];
            return new AbilityStorageItem(abilityBase, name);
        }
    }

    public override ItemType GetItemType()
    {
        return ItemType.ABILITY;
    }
}
