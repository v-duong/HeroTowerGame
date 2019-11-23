public enum EffectType
{
    BLEED,          //physical dot, increased damage over distance
    BURN,        //fire dot
    CHILL,          //slow, reduced regen
    ELECTROCUTE,    //delayed burst of aoe damage
    FRACTURE,       //reduced defenses (armor, shield, block/phasing)
    PACIFY,         //reduced damage
    RADIATION,     //dot to target + nearby enemies
    POISON,
    BUFF,
    DEBUFF,
    STUN,
    PLAGUE,
    BERSERK,
    KNOCKBACK,
    BODYGUARD_AURA,
    MASS_SHIELD_AURA,
    REPEAT_OFFENDER_BUFF,
    EXPLODE_MAX_LIFE,
    EXPLODE_OVERKILL,
    EXPLODE_HIT_DAMAGE,
    EXPLODE_BLEED,
    EXPLODE_BURN,
    EXPLODE_ELECTROCUTE,
    EXPLODE_RADIATION,
    SPREAD_STATUSES,
    SPREAD_DAMAGING_STATUSES,
    SPREAD_BLEED,
    SPREAD_BURN,
    SPREAD_RADIATION,
    CLEAR_STATUSES,
    RETALIATION_DAMAGE,
}
[System.Flags]
public enum EffectApplicationFlags
{
    NONE = 0,
    CANNOT_BLEED = 0b1,
    CANNOT_BURN = 0b10,
    CANNOT_CHILL = 0b100,
    CANNOT_ELECTROCUTE = 0b1000,
    CANNOT_FRACTURE = 0b10000,
    CANNOT_PACIFY = 0b100000,
    CANNOT_RADIATION = 0b1000000,
    CANNOT_POISON = 0b10000000,
}