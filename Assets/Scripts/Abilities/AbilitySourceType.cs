using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilitySourceType
{
    ARCHETYPE,
    EQUIPMENT,
    UNIQUE,
    ABILITY_CORE
}

public interface IAbilitySource
{
    string SourceName { get; }
    AbilitySourceType AbilitySourceType { get; }
    void OnEquip(AbilityBase ability, HeroData hero, int slot);
    void OnUnequip(AbilityBase ability, HeroData hero, int slot);
    Tuple<HeroData, int> GetEquippedHeroAndSlot(AbilityBase ability);
}