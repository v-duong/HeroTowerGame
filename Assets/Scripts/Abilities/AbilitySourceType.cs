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
    int GetAbilityLevel();
}