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
    MANASHIELD_REGEN,
    SOULPOINT_REGEN,
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
    AREA_RADIUS,
    AREA_DAMAGE,
    DAMAGE_OVER_TIME,
    DAMAGE_OVER_TIME_SPEED,
    NONDAMAGE_STATUS_EFFECTIVENESS,
    STATUS_EFFECT_DAMAGE,
    STATUS_EFFECT_RESISTANCE,
    STATUS_EFFECT_AVOIDANCE,
    STATUS_EFFECT_CHANCE,
    STATUS_EFFECT_DURATION,
    BLEED_CHANCE,
    BURN_CHANCE,
    CHILL_CHANCE,
    ELECTROCUTE_CHANCE,
    FRACTURE_CHANCE,
    PACIFY_CHANCE,
    RADIATION_CHANCE,
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
    SPELL_PHYSICAL_DAMAGE_MIN = 0x300,
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
    ATTACK_CRITICAL_CHANCE,
    ATTACK_CRITICAL_DAMAGE,
    DAMAGE_VS_BOSS,
    LOCAL_MAX_SHIELD = Armor.LocalBonusStart, // local armor mods
    LOCAL_ARMOR,
    LOCAL_DODGE_RATING,
    LOCAL_RESOLVE_RATING,
    LOCAL_ATTACK_SPEED = Weapon.LocalBonusStart, // local weapon mods
    LOCAL_PHYSICAL_DAMAGE,
    LOCAL_PHYSICAL_DAMAGE_MIN,
    LOCAL_PHYSICAL_DAMAGE_MAX,
    LOCAL_NONPHYSICAL_DAMAGE,
    LOCAL_FIRE_DAMAGE_MIN,
    LOCAL_FIRE_DAMAGE_MAX,
    LOCAL_COLD_DAMAGE_MIN,
    LOCAL_COLD_DAMAGE_MAX,
    LOCAL_LIGHTNING_DAMAGE_MIN,
    LOCAL_LIGHTNING_DAMAGE_MAX,
    LOCAL_EARTH_DAMAGE_MIN,
    LOCAL_EARTH_DAMAGE_MAX,
    LOCAL_DIVINE_DAMAGE_MIN,
    LOCAL_DIVINE_DAMAGE_MAX,
    LOCAL_VOID_DAMAGE_MIN,
    LOCAL_VOID_DAMAGE_MAX,
    LOCAL_CRITICAL_CHANCE,
    LOCAL_WEAPON_RANGE,
    SWORD_PHYSICAL_DAMAGE = HeroArchetypeData.LocalBonusStart, //start of archetype node mods
    SWORD_ELEMENTAL_DAMAGE,
    SWORD_PRIMORIDAL_DAMAGE,
    SWORD_ATTACK_SPEED,
    SWORD_CRITICAL_CHANCE,
    SWORD_CRITICAL_DAMAGE,
    AXE_PHYSICAL_DAMAGE,
    AXE_ELEMENTAL_DAMAGE,
    AXE_PRIMORIDAL_DAMAGE,
    AXE_ATTACK_SPEED,
    AXE_CRITICAL_CHANCE,
    AXE_CRITICAL_DAMAGE,
    MACE_PHYSICAL_DAMAGE,
    MACE_ELEMENTAL_DAMAGE,
    MACE_PRIMORIDAL_DAMAGE,
    MACE_ATTACK_SPEED,
    MACE_CRITICAL_CHANCE,
    MACE_CRITICAL_DAMAGE,
    WAND_PHYSICAL_DAMAGE,
    WAND_ELEMENTAL_DAMAGE,
    WAND_PRIMORIDAL_DAMAGE,
    WAND_ATTACK_SPEED,
    WAND_CRITICAL_CHANCE,
    WAND_CRITICAL_DAMAGE,
    WAND_SPELL_DAMAGE,
    WAND_CAST_SPEED,
    STAFF_PHYSICAL_DAMAGE,
    STAFF_ELEMENTAL_DAMAGE,
    STAFF_PRIMORIDAL_DAMAGE,
    STAFF_ATTACK_SPEED,
    STAFF_CRITICAL_CHANCE,
    STAFF_CRITICAL_DAMAGE,
    STAFF_SPELL_DAMAGE,
    STAFF_CAST_SPEED,
    GRIMOIRE_PHYSICAL_DAMAGE,
    GRIMOIRE_ELEMENTAL_DAMAGE,
    GRIMOIRE_PRIMORIDAL_DAMAGE,
    GRIMOIRE_ATTACK_SPEED,
    GRIMOIRE_CRITICAL_CHANCE,
    GRIMOIRE_CRITICAL_DAMAGE,
    GRIMOIRE_SPELL_DAMAGE,
    GRIMOIRE_CAST_SPEED,
    SPEAR_PHYSICAL_DAMAGE,
    SPEAR_ELEMENTAL_DAMAGE,
    SPEAR_PRIMORIDAL_DAMAGE,
    SPEAR_ATTACK_SPEED,
    SPEAR_CRITICAL_CHANCE,
    SPEAR_CRITICAL_DAMAGE,
    BOW_PHYSICAL_DAMAGE,
    BOW_ELEMENTAL_DAMAGE,
    BOW_PRIMORIDAL_DAMAGE,
    BOW_ATTACK_SPEED,
    BOW_CRITICAL_CHANCE,
    BOW_CRITICAL_DAMAGE,
    CROSSBOW_PHYSICAL_DAMAGE,
    CROSSBOW_ELEMENTAL_DAMAGE,
    CROSSBOW_PRIMORIDAL_DAMAGE,
    CROSSBOW_ATTACK_SPEED,
    CROSSBOW_CRITICAL_CHANCE,
    CROSSBOW_CRITICAL_DAMAGE,
    GUN_PHYSICAL_DAMAGE,
    GUN_ELEMENTAL_DAMAGE,
    GUN_PRIMORIDAL_DAMAGE,
    GUN_ATTACK_SPEED,
    GUN_CRITICAL_CHANCE,
    GUN_CRITICAL_DAMAGE,
    SHIELD_PHYSICAL_DAMAGE,
    SHIELD_ELEMENTAL_DAMAGE,
    SHIELD_PRIMORIDAL_DAMAGE,
    SHIELD_ATTACK_SPEED,
    SHIELD_CRITICAL_CHANCE,
    SHIELD_CRITICAL_DAMAGE,
    SHIELD_BLOCK_CHANCE,
    ONE_HANDED_PHYSICAL_DAMAGE,
    ONE_HANDED_ELEMENTAL_DAMAGE,
    ONE_HANDED_PRIMORIDAL_DAMAGE,
    ONE_HANDED_ATTACK_SPEED,
    ONE_HANDED_CRITICAL_CHANCE,
    ONE_HANDED_CRITICAL_DAMAGE,
    TWO_HANDED_PHYSICAL_DAMAGE,
    TWO_HANDED_ELEMENTAL_DAMAGE,
    TWO_HANDED_PRIMORIDAL_DAMAGE,
    TWO_HANDED_ATTACK_SPEED,
    TWO_HANDED_CRITICAL_CHANCE,
    TWO_HANDED_CRITICAL_DAMAGE,
    DUAL_WIELD_PHYSICAL_DAMAGE,
    DUAL_WIELD_ELEMENTAL_DAMAGE,
    DUAL_WIELD_PRIMORIDAL_DAMAGE,
    DUAL_WIELD_ATTACK_SPEED,
    DUAL_WIELD_CRITICAL_CHANCE,
    DUAL_WIELD_CRITICAL_DAMAGE,
    MELEE_WEAPON_PHYSICAL_DAMAGE,
    MELEE_WEAPON_ELEMENTAL_DAMAGE,
    MELEE_WEAPON_PRIMORIDAL_DAMAGE,
    MELEE_WEAPON_ATTACK_SPEED,
    MELEE_WEAPON_CRITICAL_CHANCE,
    MELEE_WEAPON_CRITICAL_DAMAGE,
    RANGED_WEAPON_PHYSICAL_DAMAGE,
    RANGED_WEAPON_ELEMENTAL_DAMAGE,
    RANGED_WEAPON_PRIMORIDAL_DAMAGE,
    RANGED_WEAPON_ATTACK_SPEED,
    RANGED_WEAPON_CRITICAL_CHANCE,
    RANGED_WEAPON_CRITICAL_DAMAGE,
}