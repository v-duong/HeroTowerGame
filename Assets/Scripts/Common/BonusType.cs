﻿public enum BonusType
{
    MAX_HEALTH,
    MAX_SOULPOINTS,
    GLOBAL_MAX_SHIELD,
    GLOBAL_ARMOR,
    GLOBAL_DODGE_RATING,
    GLOBAL_RESOLVE_RATING,
    ATTACK_PHASING,
    MAGIC_PHASING,
    HEALTH_REGEN,
    PERCENT_HEALTH_REGEN,
    SHIELD_REGEN,
    PERCENT_SHIELD_REGEN,
    SHIELD_RESTORE_SPEED,
    SHIELD_RESTORE_DELAY,
    MAX_HEALTH_TO_SHIELD,
    HEALTH_AS_EXTRA_SHIELD,
    SOULPOINT_REGEN,
    SOULPOINT_COST,
    STRENGTH,
    INTELLIGENCE,
    AGILITY,
    WILL,
    MAX_PHYSICAL_RESISTANCE,
    PHYSICAL_RESISTANCE,
    MAX_FIRE_RESISTANCE,
    FIRE_RESISTANCE,
    MAX_COLD_RESISTANCE,
    COLD_RESISTANCE,
    MAX_LIGHTNING_RESISTANCE,
    LIGHTNING_RESISTANCE,
    MAX_EARTH_RESISTANCE,
    EARTH_RESISTANCE,
    MAX_DIVINE_RESISTANCE,
    DIVINE_RESISTANCE,
    MAX_VOID_RESISTANCE,
    VOID_RESISTANCE,
    MAX_ELEMENTAL_RESISTANCES,
    ELEMENTAL_RESISTANCES,
    MAX_PRIMORDIAL_RESISTANCES,
    PRIMORDIAL_RESISTANCES,
    MAX_ALL_NONPHYSICAL_RESISTANCES,
    ALL_NONPHYSICAL_RESISTANCES,
    DAMAGE_TAKEN,
    DAMAGE_TAKEN_BY_SHIELD,
    ATTACK_DAMAGE = 0x100,
    SPELL_DAMAGE,
    RANGED_ATTACK_DAMAGE,
    MELEE_ATTACK_DAMAGE,
    GLOBAL_DAMAGE,
    GLOBAL_ELEMENTAL_DAMAGE,
    GLOBAL_PRIMORDIAL_DAMAGE,
    GLOBAL_PHYSICAL_DAMAGE,
    GLOBAL_PHYSICAL_DAMAGE_MIN,
    GLOBAL_PHYSICAL_DAMAGE_MAX,
    GLOBAL_FIRE_DAMAGE,
    GLOBAL_FIRE_DAMAGE_MIN,
    GLOBAL_FIRE_DAMAGE_MAX,
    GLOBAL_COLD_DAMAGE,
    GLOBAL_COLD_DAMAGE_MIN,
    GLOBAL_COLD_DAMAGE_MAX,
    GLOBAL_LIGHTNING_DAMAGE,
    GLOBAL_LIGHTNING_DAMAGE_MIN,
    GLOBAL_LIGHTNING_DAMAGE_MAX,
    GLOBAL_EARTH_DAMAGE,
    GLOBAL_EARTH_DAMAGE_MIN,
    GLOBAL_EARTH_DAMAGE_MAX,
    GLOBAL_DIVINE_DAMAGE,
    GLOBAL_DIVINE_DAMAGE_MIN,
    GLOBAL_DIVINE_DAMAGE_MAX,
    GLOBAL_VOID_DAMAGE,
    GLOBAL_VOID_DAMAGE_MIN,
    GLOBAL_VOID_DAMAGE_MAX,
    GLOBAL_ATTACK_SPEED,
    CAST_SPEED,
    GLOBAL_ABILITY_SPEED,
    GLOBAL_CRITICAL_CHANCE,
    GLOBAL_CRITICAL_DAMAGE,
    CRITICAL_DEFENSE,
    AURA_EFFECT,
    AURA_RANGE,
    AURA_RESERVATION,
    PROJECTILE_SPEED,
    PROJECTILE_DAMAGE,
    PROJECTILE_COUNT,
    PROJECTILE_PIERCE,
    PROJECTILE_CHAIN,
    PROJECTILE_SPLIT,
    PROJECTILE_SIZE,
    PROJECTILE_SIZE_OVER_TIME,
    PROJECTILE_HOMING,
    HITSCAN_MULTI_TARGET_COUNT,
    AREA_RADIUS,
    AREA_DAMAGE,
    DAMAGE_OVER_TIME,
    DAMAGE_OVER_TIME_SPEED,
    NONDAMAGE_STATUS_EFFECTIVENESS,
    AFFLICTED_STATUS_AVOIDANCE,
    AFFLICTED_STATUS_DURATION,
    AFFLICTED_STATUS_DAMAGE_RESISTANCE,
    AFFLICTED_STATUS_THRESHOLD,
    STATUS_EFFECT_DAMAGE,
    STATUS_EFFECT_CHANCE,
    STATUS_EFFECT_DURATION,
    ELEMENTAL_STATUS_EFFECT_CHANCE,
    ELEMENTAL_STATUS_EFFECT_DURATION,
    ELEMENTAL_STATUS_EFFECT_EFFECTIVENESS,
    PRIMORDIAL_STATUS_EFFECT_CHANCE,
    PRIMORDIAL_STATUS_EFFECT_DURATION,
    PRIMORDIAL_STATUS_EFFECT_EFFECTIVENESS,
    BLEED_CHANCE,
    BURN_CHANCE,
    CHILL_CHANCE,
    ELECTROCUTE_CHANCE,
    FRACTURE_CHANCE,
    PACIFY_CHANCE,
    RADIATION_CHANCE,
    BLEED_CHANCE_WHEN_HIT,
    BURN_CHANCE_WHEN_HIT,
    CHILL_CHANCE_WHEN_HIT,
    ELECTROCUTE_CHANCE_WHEN_HIT,
    FRACTURE_CHANCE_WHEN_HIT,
    PACIFY_CHANCE_WHEN_HIT,
    RADIATION_CHANCE_WHEN_HIT,
    SELF_BLEED_CHANCE_ON_HIT,
    SELF_BURN_CHANCE_ON_HIT,
    SELF_CHILL_CHANCE_ON_HIT,
    SELF_ELECTROCUTE_CHANCE_ON_HIT,
    SELF_FRACTURE_CHANCE_ON_HIT,
    SELF_PACIFY_CHANCE_ON_HIT,
    SELF_RADIATION_CHANCE_ON_HIT,
    BLEED_DURATION,
    BURN_DURATION,
    CHILL_DURATION,
    ELECTROCUTE_DURATION,
    FRACTURE_DURATION,
    PACIFY_DURATION,
    RADIATION_DURATION,
    BLEED_EFFECTIVENESS,
    BURN_EFFECTIVENESS,
    CHILL_EFFECTIVENESS,
    ELECTROCUTE_EFFECTIVENESS,
    FRACTURE_EFFECTIVENESS,
    PACIFY_EFFECTIVENESS,
    RADIATION_EFFECTIVENESS,
    BLEED_SPEED,
    BURN_SPEED,
    ELECTROCUTE_SPEED,
    RADIATION_SPEED,
    POISON_CHANCE,
    POISON_EFFECTIVENESS,
    POISON_DURATION,
    POISON_SPEED,
    POISON_SHIELD_RESISTANCE,
    SPELL_RANGE,
    RANGED_ATTACK_RANGE,
    MELEE_ATTACK_RANGE,
    MOVEMENT_SPEED,
    PHYSICAL_RESISTANCE_NEGATION,
    FIRE_RESISTANCE_NEGATION,
    COLD_RESISTANCE_NEGATION,
    LIGHTNING_RESISTANCE_NEGATION,
    EARTH_RESISTANCE_NEGATION,
    DIVINE_RESISTANCE_NEGATION,
    VOID_RESISTANCE_NEGATION,
    ELEMENTAL_RESISTANCE_NEGATION,
    PRIMORDIAL_RESISTANCE_NEGATION,
    ALL_RESISTANCE_NEGATION,
    ALL_ABILITY_LEVEL,
    PHYSICAL_ABILITY_LEVEL,
    FIRE_ABILITY_LEVEL,
    COLD_ABILITY_LEVEL,
    LIGHTNING_ABILITY_LEVEL,
    EARTH_ABILITY_LEVEL,
    DIVINE_ABILITY_LEVEL,
    VOID_ABILITY_LEVEL,
    ATTACK_ABILITY_LEVEL,
    SPELL_ABILITY_LEVEL,
    SLOT_1_ABILITY_LEVEL,
    SLOT_2_ABILITY_LEVEL,
    SHIELD_BLOCK_CHANCE,
    MAX_SHIELD_BLOCK_CHANCE,
    SHIELD_BLOCK_PROTECTION,
    MAX_SHIELD_BLOCK_PROTECTION,
    WEAPON_ATTACK_MAX_PARRY_CHANCE,
    WEAPON_ATTACK_PARRY_CHANCE,
    WEAPON_ATTACK_PARRY_REFLECTION,
    WEAPON_SPELL_PARRY_CHANCE,
    WEAPON_SPELL_MAX_PARRY_CHANCE,
    WEAPON_SPELL_PARRY_REFLECTION,
    PHYSICAL_CONVERT_FIRE,
    PHYSICAL_CONVERT_COLD,
    PHYSICAL_CONVERT_EARTH,
    PHYSICAL_CONVERT_LIGHTNING,
    PHYSICAL_CONVERT_DIVINE,
    PHYSICAL_CONVERT_VOID,
    PHYSICAL_CONVERT_ELEMENTAL,
    SPELL_PHYSICAL_DAMAGE_MIN,
    SPELL_PHYSICAL_DAMAGE_MAX,
    SPELL_FIRE_DAMAGE_MIN,
    SPELL_FIRE_DAMAGE_MAX,
    SPELL_COLD_DAMAGE_MIN,
    SPELL_COLD_DAMAGE_MAX,
    SPELL_LIGHTNING_DAMAGE_MIN,
    SPELL_LIGHTNING_DAMAGE_MAX,
    SPELL_EARTH_DAMAGE_MIN,
    SPELL_EARTH_DAMAGE_MAX,
    SPELL_DIVINE_DAMAGE_MIN,
    SPELL_DIVINE_DAMAGE_MAX,
    SPELL_VOID_DAMAGE_MIN,
    SPELL_VOID_DAMAGE_MAX,
    SPELL_ELEMENTAL_DAMAGE,
    SPELL_PRIMORDIAL_DAMAGE,
    SPELL_PHYSICAL_DAMAGE,
    SPELL_FIRE_DAMAGE,
    SPELL_COLD_DAMAGE,
    SPELL_LIGHTNING_DAMAGE,
    SPELL_EARTH_DAMAGE,
    SPELL_DIVINE_DAMAGE,
    SPELL_VOID_DAMAGE,
    SPELL_CRITICAL_CHANCE,
    SPELL_CRITICAL_DAMAGE,
    ATTACK_PHYSICAL_DAMAGE_MIN,
    ATTACK_PHYSICAL_DAMAGE_MAX,
    ATTACK_FIRE_DAMAGE_MIN,
    ATTACK_FIRE_DAMAGE_MAX,
    ATTACK_COLD_DAMAGE_MIN,
    ATTACK_COLD_DAMAGE_MAX,
    ATTACK_LIGHTNING_DAMAGE_MIN,
    ATTACK_LIGHTNING_DAMAGE_MAX,
    ATTACK_EARTH_DAMAGE_MIN,
    ATTACK_EARTH_DAMAGE_MAX,
    ATTACK_DIVINE_DAMAGE_MIN,
    ATTACK_DIVINE_DAMAGE_MAX,
    ATTACK_VOID_DAMAGE_MIN,
    ATTACK_VOID_DAMAGE_MAX,
    ATTACK_ELEMENTAL_DAMAGE,
    ATTACK_PRIMORDIAL_DAMAGE,
    ATTACK_PHYSICAL_DAMAGE,
    ATTACK_FIRE_DAMAGE,
    ATTACK_COLD_DAMAGE,
    ATTACK_LIGHTNING_DAMAGE,
    ATTACK_EARTH_DAMAGE,
    ATTACK_DIVINE_DAMAGE,
    ATTACK_VOID_DAMAGE,
    ATTACK_CRITICAL_CHANCE,
    ATTACK_CRITICAL_DAMAGE,
    DAMAGE_VS_BOSS,
    AGGRO_PRIORITY_RATE,
    DIRECT_HIT_DAMAGE,
    LOCAL_MAX_SHIELD = Armor.LocalBonusStart, // local armor mods
    LOCAL_ARMOR,
    LOCAL_DODGE_RATING,
    LOCAL_RESOLVE_RATING,
    LOCAL_ATTACK_SPEED = Weapon.LocalBonusStart, // local weapon mods
    LOCAL_PHYSICAL_DAMAGE,
    LOCAL_PHYSICAL_DAMAGE_MIN,
    LOCAL_PHYSICAL_DAMAGE_MAX,
    LOCAL_NONPHYSICAL_DAMAGE,
    LOCAL_FIRE_DAMAGE,
    LOCAL_FIRE_DAMAGE_MIN,
    LOCAL_FIRE_DAMAGE_MAX,
    LOCAL_COLD_DAMAGE,
    LOCAL_COLD_DAMAGE_MIN,
    LOCAL_COLD_DAMAGE_MAX,
    LOCAL_LIGHTNING_DAMAGE,
    LOCAL_LIGHTNING_DAMAGE_MIN,
    LOCAL_LIGHTNING_DAMAGE_MAX,
    LOCAL_EARTH_DAMAGE,
    LOCAL_EARTH_DAMAGE_MIN,
    LOCAL_EARTH_DAMAGE_MAX,
    LOCAL_DIVINE_DAMAGE,
    LOCAL_DIVINE_DAMAGE_MIN,
    LOCAL_DIVINE_DAMAGE_MAX,
    LOCAL_VOID_DAMAGE,
    LOCAL_VOID_DAMAGE_MIN,
    LOCAL_VOID_DAMAGE_MAX,
    LOCAL_ELEMENTAL_DAMAGE,
    LOCAL_PRIMORDIAL_DAMAGE,
    LOCAL_CRITICAL_CHANCE,
    LOCAL_WEAPON_RANGE,
    CAN_USE_SPEARS_WITH_SHIELDS = HeroArchetypeData.SpecialBonusStart, //start of archetype node mods
    TWO_HANDED_WEAPONS_ARE_ONE_HANDED,
    ONE_HANDED_WEAPONS_ARE_TWO_HANDED,
    SHIELDS_DRAINED_FOR_DAMAGE,
    PLAGUE_SPREAD_DURATION_INCREASE,
    SHIELD_RESTORE_CANNOT_BE_INTERRUPTED,
    SHIELD_RESTORE_APPLIES_TO_REGEN,
}