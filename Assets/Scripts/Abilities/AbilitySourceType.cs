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
    Guid SourceId { get; }
    AbilitySourceType AbilitySourceType { get; }
    void OnAbilityEquip(AbilityBase ability, HeroData hero, int slot);
    void OnAbilityUnequip(AbilityBase ability, HeroData hero, int slot);
    Tuple<HeroData, int> GetEquippedHeroAndSlot(AbilityBase ability);
}