using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class ActorAbility
{
    public readonly AbilityBase abilityBase;
    public int abilityLevel;
    public AbilityColliderContainer abilityCollider;
    public Dictionary<ElementType, MinMaxRange> damageBase = new Dictionary<ElementType, MinMaxRange>();
    public AbilityTargetType TargetType { get; private set; }
    public int targetLayer;
    public int targetMask;
    public bool isSecondaryAbility;

    public Actor AbilityOwner { get; private set; }
    public float AreaLength { get; private set; }
    public float AreaRadius { get; private set; }
    public float Cooldown { get; private set; }
    public float HitscanDelay { get; private set; }
    public float ProjectileSize { get; private set; }
    public float ProjectileSpeed { get; private set; }
    public int ProjectilePierce { get; private set; }
    public float CriticalChance { get; private set; }
    public int CriticalDamage { get; private set; }
    public float TargetRange { get; private set; }
    public int ProjectileCount { get; private set; }

    private ActorData actorData;
    private float AreaScaling;
    private float ProjectileScaling;
    private Dictionary<BonusType, StatBonus> abilityBonuses;

    public AbilityOnHitDataContainer abilityOnHitData;

    public LinkedActorAbility LinkedAbility { get; private set; }

    public List<Actor> targetList = new List<Actor>();
    public Actor CurrentTarget { get; private set; }

    private bool firingRoutineRunning;
    private IEnumerator firingRoutine;
    private ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();

    public ActorAbility(AbilityBase ability, int layer)
    {
        if (ability == null)
            return;
        abilityBase = ability;
        abilityOnHitData = new AbilityOnHitDataContainer
        {
            Type = abilityBase.abilityType
        };
        abilityBonuses = new Dictionary<BonusType, StatBonus>();

        AreaLength = abilityBase.areaLength;
        AreaRadius = abilityBase.areaRadius;
        Cooldown = 1 / abilityBase.attacksPerSec;
        HitscanDelay = abilityBase.hitscanDelay;
        ProjectileSize = abilityBase.projectileSize;
        ProjectileSpeed = abilityBase.projectileSpeed;

        CriticalChance = abilityBase.baseCritical;
        CriticalDamage = 50;
        ProjectileCount = abilityBase.projectileCount;
        TargetRange = abilityBase.targetRange;
        targetLayer = layer;

        if (LayerMask.LayerToName(targetLayer) == "EnemyDetect")
            targetMask = LayerMask.GetMask("Enemy");
        else if (LayerMask.LayerToName(targetLayer) == "AllyDetect")
            targetMask = LayerMask.GetMask("Hero");

        if (abilityBase.hasLinkedAbility)
        {
            if (abilityBase.linkedAbility.inheritsDamage)
                LinkedAbility = new LinkedActorAbility(ability.linkedAbility, targetLayer, ability.damageLevels);
            else
                LinkedAbility = new LinkedActorAbility(ability.linkedAbility, targetLayer);
            LinkedAbility.abilityLevel = abilityLevel;
        }
    }

    public void SetAsSecondaryAbility()
    {
        isSecondaryAbility = true;
        StatBonus speedPenalty = new StatBonus();
        speedPenalty.AddBonus(ModifyType.MULTIPLY, -25);
        StatBonus damagePenalty = new StatBonus();
        damagePenalty.AddBonus(ModifyType.MULTIPLY, -50);
        abilityBonuses.Add(BonusType.GLOBAL_ABILITY_SPEED, speedPenalty);
        abilityBonuses.Add(BonusType.GLOBAL_DAMAGE, damagePenalty);
    }

    public void SetDamageAndSpeedModifier(float damage, float speed)
    {
        StatBonus damageMod = new StatBonus();
        StatBonus speedMod = new StatBonus();
        damageMod.AddBonus(ModifyType.MULTIPLY, damage);
        speedMod.AddBonus(ModifyType.MULTIPLY, speed);
        abilityBonuses.Add(BonusType.GLOBAL_DAMAGE, damageMod);
        abilityBonuses.Add(BonusType.GLOBAL_ABILITY_SPEED, speedMod);
    }

    public void SetAbilityOwner(Actor actor)
    {
        AbilityOwner = actor;
        abilityOnHitData.sourceActor = actor;
        if (AbilityOwner is EnemyActor)
            abilityOnHitData.accuracy = (float)Helpers.GetEnemyAccuracyScaling(AbilityOwner.Data.Level);
        if (LinkedAbility != null)
            LinkedAbility.SetAbilityOwner(AbilityOwner);
    }

    protected void UpdateCurrentTarget(Actor actor)
    {
        if (targetList.Count > 0)
        {
            CurrentTarget = targetList[0];
        }
        else
        {
            CurrentTarget = null;
        }
    }

    public void AddToTargetList(Actor actor)
    {
        targetList.Add(actor);
        UpdateCurrentTarget(actor);
    }

    public void RemoveFromTargetList(Actor actor)
    {
        targetList.Remove(actor);
        if (CurrentTarget == actor)
        {
            UpdateCurrentTarget(actor);
        }
    }

    public void UpdateAbilityLevel(int level)
    {
        abilityLevel = level;
        if (LinkedAbility != null)
            LinkedAbility.abilityLevel = level;
    }

    public virtual void UpdateAbilityStats(HeroData data)
    {
        IEnumerable<GroupType> tags = data.groupTypes;
        UpdateAbilityBonusProperties(tags);
        UpdateDamage(data, abilityBase.damageLevels, tags);
        UpdateTypeParameters(data, tags);
        UpdateShotParameters(data, tags);
        UpdateOnHitDataBonuses(data, tags);
        if (abilityCollider != null)
            abilityCollider.abilityCollider.radius = TargetRange;
        if (LinkedAbility != null)
            LinkedAbility.UpdateAbilityStats(data, tags);
    }

    public virtual void UpdateAbilityStats(EnemyData data)
    {
        IEnumerable<GroupType> tags = data.groupTypes;
        UpdateAbilityBonusProperties(tags);
        UpdateDamage(data, abilityBase.damageLevels, tags);
        UpdateTypeParameters(data, tags);
        UpdateShotParameters(data, tags);
        UpdateOnHitDataBonuses(data, tags);
        if (abilityCollider != null)
            abilityCollider.abilityCollider.radius = TargetRange;
        if (LinkedAbility != null)
            LinkedAbility.UpdateAbilityStats(data, tags);
    }

    protected void UpdateAbilityBonusProperties(IEnumerable<GroupType> tags)
    {
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
            return;
        foreach (AbilityScalingBonusProperty bonusProperty in abilityBase.bonusProperties)
        {
            if (abilityBonuses.TryGetValue(bonusProperty.bonusType, out StatBonus temp))
            {
                temp.ResetBonus();
            }
            else
            {
                temp = new StatBonus();
                abilityBonuses.Add(bonusProperty.bonusType, temp);
            }
            temp.AddBonus(bonusProperty.modifyType, bonusProperty.initialValue + bonusProperty.growthValue * abilityLevel);
        }
    }

    protected void UpdateShotParameters(HeroData data, IEnumerable<GroupType> tags)
    {
        if (abilityBase.abilityType == AbilityType.ATTACK && abilityBase.useWeaponRangeForAOE)
        {
            if (data.GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon weapon)
            {
                AreaRadius = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AREA_RADIUS, BonusType.MELEE_ATTACK_RANGE).CalculateStat(weapon.WeaponRange);
            }
            else
            {
                AreaRadius = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AREA_RADIUS, BonusType.MELEE_ATTACK_RANGE).CalculateStat(1f);
            }
        }
        else
        {
            AreaRadius = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AREA_RADIUS).CalculateStat(abilityBase.areaRadius);
        }

        AreaScaling = AreaRadius / abilityBase.areaRadius;

        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
        {
            TargetRange = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AURA_RANGE).CalculateStat(abilityBase.targetRange);
            return;
        }

        ProjectileSpeed = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_SPEED).CalculateStat(abilityBase.projectileSpeed);
        ProjectilePierce = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_PIERCE).CalculateStat(0);
        ProjectileCount = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_COUNT).CalculateStat(abilityBase.projectileCount);
    }

    protected void UpdateShotParameters(EnemyData data, IEnumerable<GroupType> tags)
    {
        AreaRadius = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AREA_RADIUS).CalculateStat(abilityBase.areaRadius);
        AreaScaling = (AreaRadius) / abilityBase.areaRadius;
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
        {
            TargetRange = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AURA_RANGE).CalculateStat(abilityBase.targetRange);
            return;
        }

        ProjectileSpeed = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_SPEED).CalculateStat(abilityBase.projectileSpeed);
        ProjectilePierce = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_PIERCE).CalculateStat(0);
        ProjectileCount = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_COUNT).CalculateStat(abilityBase.projectileCount);
    }

    protected void UpdateTypeParameters(HeroData data, IEnumerable<GroupType> tags)
    {
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
            return;

        StatBonus critDamageBonus = new StatBonus();

        if (abilityBase.abilityType == AbilityType.SPELL)
        {
            StatBonus critBonus = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.SPELL_CRITICAL_CHANCE, BonusType.GLOBAL_CRITICAL_CHANCE);
            data.GetMultiStatBonus(critDamageBonus, abilityBonuses, tags, BonusType.SPELL_CRITICAL_DAMAGE, BonusType.GLOBAL_CRITICAL_DAMAGE);
            StatBonus speedBonus = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.CAST_SPEED, BonusType.GLOBAL_ABILITY_SPEED);
            StatBonus rangeBonus = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.SPELL_RANGE);

            CriticalChance = critBonus.CalculateStat(abilityBase.baseCritical);
            Cooldown = 1f / speedBonus.CalculateStat(abilityBase.attacksPerSec);
            TargetRange = rangeBonus.CalculateStat(abilityBase.targetRange);
        }
        else if (abilityBase.abilityType == AbilityType.ATTACK)
        {
            StatBonus critBonus = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.GLOBAL_CRITICAL_CHANCE, BonusType.ATTACK_CRITICAL_CHANCE);
            data.GetMultiStatBonus(critDamageBonus, abilityBonuses, tags, BonusType.GLOBAL_CRITICAL_DAMAGE, BonusType.ATTACK_CRITICAL_DAMAGE);
            StatBonus speedBonus = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.GLOBAL_ATTACK_SPEED, BonusType.GLOBAL_ABILITY_SPEED);
            StatBonus rangeBonus = new StatBonus();
            if (abilityBase.GetGroupTypes().Contains(GroupType.MELEE_ATTACK))
                data.GetMultiStatBonus(rangeBonus, abilityBonuses, tags, BonusType.MELEE_ATTACK_RANGE);
            if (abilityBase.GetGroupTypes().Contains(GroupType.RANGED_ATTACK))
                data.GetMultiStatBonus(rangeBonus, abilityBonuses, tags, BonusType.RANGED_ATTACK_RANGE);

            if (data.GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon weapon)
            {
                Cooldown = 1f / speedBonus.CalculateStat(weapon.AttackSpeed);
                if (abilityBase.useWeaponRangeForTargeting)
                {
                    TargetRange = rangeBonus.CalculateStat(weapon.WeaponRange);
                }
                else
                    TargetRange = rangeBonus.CalculateStat(abilityBase.targetRange);
                CriticalChance = critBonus.CalculateStat(weapon.CriticalChance);
            }
            else
            {
                //Unarmed default values
                if (abilityBase.useWeaponRangeForTargeting)
                {
                    TargetRange = rangeBonus.CalculateStat(1f);
                }
                else
                    TargetRange = rangeBonus.CalculateStat(abilityBase.targetRange);
                Cooldown = 1f / speedBonus.CalculateStat(1f);
                CriticalChance = critBonus.CalculateStat(3.5f);
            }
        }

        CriticalDamage = critDamageBonus.CalculateStat(50);

        if (float.IsInfinity(Cooldown))
            Cooldown = 0.001f;
    }

    protected void UpdateTypeParameters(EnemyData data, IEnumerable<GroupType> tags)
    {
        StatBonus critBonus = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.GLOBAL_CRITICAL_CHANCE);
        StatBonus critDamageBonus = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.GLOBAL_CRITICAL_DAMAGE);

        if (abilityBase.abilityType == AbilityType.SPELL)
        {
            StatBonus speedBonus = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.CAST_SPEED, BonusType.GLOBAL_ABILITY_SPEED);
            StatBonus rangeBonus = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.SPELL_RANGE);

            CriticalChance = critBonus.CalculateStat(abilityBase.baseCritical);
            Cooldown = (1 / speedBonus.CalculateStat(abilityBase.attacksPerSec));
            TargetRange = rangeBonus.CalculateStat(abilityBase.targetRange);
        }
        else if (abilityBase.abilityType == AbilityType.ATTACK)
        {
            StatBonus speedBonus = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.GLOBAL_ATTACK_SPEED, BonusType.GLOBAL_ABILITY_SPEED);
            StatBonus rangeBonus = new StatBonus();
            if (abilityBase.GetGroupTypes().Contains(GroupType.MELEE_ATTACK))
            {
                data.GetMultiStatBonus(rangeBonus, abilityBonuses, tags, BonusType.MELEE_ATTACK_RANGE);
            }
            if (abilityBase.GetGroupTypes().Contains(GroupType.RANGED_ATTACK))
            {
                data.GetMultiStatBonus(rangeBonus, abilityBonuses, tags, BonusType.RANGED_ATTACK_RANGE);
            }

            CriticalChance = critBonus.CalculateStat(data.BaseData.attackCriticalChance);
            Cooldown = (1 / speedBonus.CalculateStat(data.BaseData.attackSpeed));
            TargetRange = rangeBonus.CalculateStat(data.BaseData.attackTargetRange);
        }
        CriticalDamage = critDamageBonus.CalculateStat(50);

        if (float.IsInfinity(Cooldown))
            Cooldown = 0.001f;
    }

    protected void UpdateDamage(HeroData data, Dictionary<ElementType, AbilityDamageBase> damageLevels, IEnumerable<GroupType> tags, float flatDamageModifier = 1.0f)
    {
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
            return;

        StatBonus minBonus, maxBonus, multiBonus;
        AbilityType abilityType = abilityBase.abilityType;
        List<GroupType> damageTags = new List<GroupType>(abilityBase.GetGroupTypes());
        Weapon weapon = null;

        if (data.GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon)
        {
            weapon = data.GetEquipmentInSlot(EquipSlotType.WEAPON) as Weapon;
        }

        for (int i = 0; i < (int)ElementType.COUNT; i++)
        {
            ElementType element = (ElementType)i;
            float minDamage = 0, maxDamage = 0;

            if (damageLevels.ContainsKey(element))
            {
                MinMaxRange abilityBaseDamage = damageLevels[element].damage[abilityLevel];
                minDamage = abilityBaseDamage.min;
                maxDamage = abilityBaseDamage.max;
            }
            if (abilityBase.abilityType == AbilityType.ATTACK)
            {
                float weaponMulti = abilityBase.weaponMultiplier + abilityBase.weaponMultiplierScaling * abilityLevel;
                if (weapon != null)
                {
                    MinMaxRange weaponDamage = weapon.GetWeaponDamage(element);
                    minDamage = weaponDamage.min * weaponMulti;
                    maxDamage = weaponDamage.max * weaponMulti;
                }
                else if (element == ElementType.PHYSICAL)
                {
                    minDamage = 3 * weaponMulti;
                    maxDamage = 6 * weaponMulti;
                }
            }

            HashSet<BonusType> min = new HashSet<BonusType>();
            HashSet<BonusType> max = new HashSet<BonusType>();
            HashSet<BonusType> multi = new HashSet<BonusType>();

            Helpers.GetDamageTypes(element, abilityType, abilityBase.abilityShotType, damageTags, min, max, multi);

            minBonus = data.GetMultiStatBonus(abilityBonuses, tags, min.ToArray());
            maxBonus = data.GetMultiStatBonus(abilityBonuses, tags, max.ToArray());
            multiBonus = data.GetMultiStatBonus(abilityBonuses, tags, multi.ToArray());

            minDamage += minBonus.CalculateStat(0) * abilityBase.flatDamageMultiplier;
            maxDamage += maxBonus.CalculateStat(0) * abilityBase.flatDamageMultiplier;

            minDamage = multiBonus.CalculateStat(minDamage);
            maxDamage = multiBonus.CalculateStat(maxDamage);

            MinMaxRange newDamageRange = new MinMaxRange
            {
                min = (int)(minDamage * flatDamageModifier),
                max = (int)(maxDamage * flatDamageModifier)
            };

            if (newDamageRange.IsZero())
                damageBase.Remove(element);
            else
            {
                damageBase[element] = newDamageRange;
            }
        }
    }

    protected void UpdateDamage(EnemyData data, Dictionary<ElementType, AbilityDamageBase> damageLevels, IEnumerable<GroupType> tags, float damageModifier = 1.0f)
    {
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
            return;

        StatBonus minBonus, maxBonus, multiBonus;
        AbilityType abilityType = abilityBase.abilityType;

        for (int i = 0; i < (int)ElementType.COUNT; i++)
        {
            float minDamage = 0, maxDamage = 0;
            ElementType element = (ElementType)i;
            if (damageLevels.ContainsKey(element))
            {
                MinMaxRange abilityBaseDamage = damageLevels[element].damage[abilityLevel];
                minDamage = abilityBaseDamage.min;
                maxDamage = abilityBaseDamage.max;
            }
            else if (abilityBase.abilityType == AbilityType.ATTACK)
            {
                float weaponMulti = abilityBase.weaponMultiplier + abilityBase.weaponMultiplierScaling * abilityLevel;
                minDamage = data.minAttackDamage;
                maxDamage = data.maxAttackDamage;
            }

            HashSet<BonusType> min = new HashSet<BonusType>();
            HashSet<BonusType> max = new HashSet<BonusType>();
            HashSet<BonusType> multi = new HashSet<BonusType>();

            Helpers.GetDamageTypes(element, abilityType, abilityBase.abilityShotType, abilityBase.GetGroupTypes(), min, max, multi);

            minBonus = data.GetMultiStatBonus(abilityBonuses, tags, min.ToArray());
            maxBonus = data.GetMultiStatBonus(abilityBonuses, tags, max.ToArray());
            multiBonus = data.GetMultiStatBonus(abilityBonuses, tags, multi.ToArray());

            minDamage += minBonus.CalculateStat(0) * abilityBase.flatDamageMultiplier;
            maxDamage += maxBonus.CalculateStat(0) * abilityBase.flatDamageMultiplier;

            minDamage = multiBonus.CalculateStat(minDamage);
            maxDamage = multiBonus.CalculateStat(maxDamage);

            MinMaxRange newDamageRange = new MinMaxRange
            {
                min = (int)(minDamage * damageModifier),
                max = (int)(maxDamage * damageModifier)
            };

            if (newDamageRange.IsZero())
                damageBase.Remove(element);
            else
            {
                damageBase[element] = newDamageRange;
            }
        }
    }

    protected void UpdateOnHitDataBonuses(ActorData data, IEnumerable<GroupType> tags)
    {
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
            return;

        StatBonus bleedChance = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.BLEED_CHANCE, BonusType.STATUS_EFFECT_CHANCE);
        StatBonus bleedEffectiveness = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.BLEED_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME);
        StatBonus bleedDuration = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.BLEED_DURATION, BonusType.STATUS_EFFECT_DURATION);
        float bleedSpeed = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.BLEED_SPEED).CalculateStat(100f) / 100f;
        abilityOnHitData.SetEffectChance(EffectType.BLEED, bleedChance.CalculateStat(0f));
        abilityOnHitData.SetEffectEffectiveness(EffectType.BLEED, bleedEffectiveness.CalculateStat(100f) / 100f * bleedSpeed);
        abilityOnHitData.SetEffectDuration(EffectType.BLEED, bleedDuration.CalculateStat(4f) / bleedSpeed);

        StatBonus burnChance = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.BURN_CHANCE, BonusType.STATUS_EFFECT_CHANCE);
        StatBonus burnEffectiveness = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.BURN_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME);
        StatBonus burnDuration = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.BURN_DURATION, BonusType.STATUS_EFFECT_DURATION);
        float burnSpeed = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.BURN_SPEED).CalculateStat(100f) / 100f;
        abilityOnHitData.SetEffectChance(EffectType.BURN, burnChance.CalculateStat(0f));
        abilityOnHitData.SetEffectEffectiveness(EffectType.BURN, burnEffectiveness.CalculateStat(100f) / 100f * burnSpeed);
        abilityOnHitData.SetEffectDuration(EffectType.BURN, burnDuration.CalculateStat(2f) / burnSpeed);

        StatBonus chillChance = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.CHILL_CHANCE, BonusType.STATUS_EFFECT_CHANCE);
        StatBonus chillEffectiveness = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.CHILL_EFFECTIVENESS, BonusType.NONDAMAGE_STATUS_EFFECTIVENESS);
        StatBonus chillDuration = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.CHILL_DURATION, BonusType.STATUS_EFFECT_DURATION);
        abilityOnHitData.SetEffectChance(EffectType.CHILL, chillChance.CalculateStat(0f));
        abilityOnHitData.SetEffectEffectiveness(EffectType.CHILL, chillEffectiveness.CalculateStat(100f) / 100f);
        abilityOnHitData.SetEffectDuration(EffectType.CHILL, chillDuration.CalculateStat(2f));

        StatBonus electrocuteChance = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.ELECTROCUTE_CHANCE, BonusType.STATUS_EFFECT_CHANCE);
        StatBonus electrocuteEffectiveness = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.ELECTROCUTE_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE);
        StatBonus electrocuteDuration = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.ELECTROCUTE_DURATION, BonusType.STATUS_EFFECT_DURATION);
        float electrocuteSpeed = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.ELECTROCUTE_SPEED).CalculateStat(100f) / 100f;
        abilityOnHitData.SetEffectChance(EffectType.ELECTROCUTE, electrocuteChance.CalculateStat(0f));
        abilityOnHitData.SetEffectEffectiveness(EffectType.ELECTROCUTE, electrocuteEffectiveness.CalculateStat(100f) / 100f * electrocuteSpeed);
        abilityOnHitData.SetEffectDuration(EffectType.ELECTROCUTE, electrocuteDuration.CalculateStat(2f) / electrocuteSpeed);

        StatBonus fractureChance = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.FRACTURE_CHANCE, BonusType.STATUS_EFFECT_CHANCE);
        StatBonus fractureEffectiveness = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.FRACTURE_EFFECTIVENESS, BonusType.NONDAMAGE_STATUS_EFFECTIVENESS);
        StatBonus fractureDuration = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.FRACTURE_DURATION, BonusType.STATUS_EFFECT_DURATION);
        abilityOnHitData.SetEffectChance(EffectType.FRACTURE, fractureChance.CalculateStat(0f));
        abilityOnHitData.SetEffectEffectiveness(EffectType.FRACTURE, fractureEffectiveness.CalculateStat(100f) / 100f);
        abilityOnHitData.SetEffectDuration(EffectType.FRACTURE, fractureDuration.CalculateStat(3f));

        StatBonus pacifyChance = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PACIFY_CHANCE, BonusType.STATUS_EFFECT_CHANCE);
        StatBonus pacifyEffectiveness = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PACIFY_EFFECTIVENESS, BonusType.NONDAMAGE_STATUS_EFFECTIVENESS);
        StatBonus pacifyDuration = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PACIFY_DURATION, BonusType.STATUS_EFFECT_DURATION);
        abilityOnHitData.SetEffectChance(EffectType.PACIFY, pacifyChance.CalculateStat(0f));
        abilityOnHitData.SetEffectEffectiveness(EffectType.PACIFY, pacifyEffectiveness.CalculateStat(100f) / 100f);
        abilityOnHitData.SetEffectDuration(EffectType.PACIFY, pacifyDuration.CalculateStat(3f));

        StatBonus radiationChance = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.RADIATION_CHANCE, BonusType.STATUS_EFFECT_CHANCE);
        StatBonus radiationEffectiveness = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.RADIATION_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME);
        StatBonus radiationDuration = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.RADIATION_DURATION, BonusType.STATUS_EFFECT_DURATION);
        float radiationSpeed = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.RADIATION_SPEED).CalculateStat(100f) / 100f;
        abilityOnHitData.SetEffectChance(EffectType.RADIATION, radiationChance.CalculateStat(0f));
        abilityOnHitData.SetEffectEffectiveness(EffectType.RADIATION, radiationEffectiveness.CalculateStat(100f) / 100f * radiationSpeed);
        abilityOnHitData.SetEffectDuration(EffectType.RADIATION, radiationDuration.CalculateStat(5f) / radiationSpeed);

        StatBonus vsBossDamage = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.DAMAGE_VS_BOSS);
        abilityOnHitData.vsBossDamage = 1f + (vsBossDamage.CalculateStat(0f) / 100f);

        StatBonus physicalNegate = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PHYSICAL_RESISTANCE_NEGATION);
        abilityOnHitData.physicalNegation = physicalNegate.CalculateStat(0);

        StatBonus fireNegate = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.FIRE_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION);
        abilityOnHitData.fireNegation = fireNegate.CalculateStat(0);

        StatBonus coldNegate = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.COLD_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION);
        abilityOnHitData.coldNegation = coldNegate.CalculateStat(0);

        StatBonus lightningNegate = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.LIGHTNING_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION);
        abilityOnHitData.lightningNegation = lightningNegate.CalculateStat(0);

        StatBonus earthNegate = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.EARTH_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION);
        abilityOnHitData.earthNegation = earthNegate.CalculateStat(0);

        StatBonus divineNegate = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.DIVINE_RESISTANCE_NEGATION, BonusType.PRIMORDIAL_RESISTANCE_NEGATION);
        abilityOnHitData.divineNegation = divineNegate.CalculateStat(0);

        StatBonus voidNegate = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.VOID_RESISTANCE_NEGATION, BonusType.PRIMORDIAL_RESISTANCE_NEGATION);
        abilityOnHitData.voidNegation = voidNegate.CalculateStat(0);
    }

    public Dictionary<ElementType, float> CalculateDamageDict()
    {
        var values = Enum.GetValues(typeof(ElementType));
        float damage = 0;
        bool isCrit = false;
        Dictionary<ElementType, float> dict = new Dictionary<ElementType, float>();
        if (UnityEngine.Random.Range(0f, 100f) < CriticalChance)
        {
            isCrit = true;
        }
        foreach (ElementType elementType in values)
        {
            if (damageBase.ContainsKey(elementType))
            {
                damage = UnityEngine.Random.Range(damageBase[elementType].min, damageBase[elementType].max + 1);
                if (isCrit)
                    damage = damage * (1f + (CriticalDamage / 100f));
                dict.Add(elementType, damage);
            }
            else
            {
                dict.Add(elementType, 0);
            }
        }
        return dict;
    }

    public void StartFiring(Actor parent)
    {
        if (firingRoutine == null)
        {
            if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
                firingRoutine = FiringRoutine_Aura();
            else
                firingRoutine = FiringRoutine_Attack();
            parent.StartCoroutine(firingRoutine);
            firingRoutineRunning = true;
        }
    }

    public void StopFiring(Actor parent)
    {
        targetList.Clear();
        CurrentTarget = null;
        if (firingRoutine != null)
        {
            parent.StopCoroutine(firingRoutine);
            firingRoutine = null;
            firingRoutineRunning = false;
        }
    }

    private IEnumerator FiringRoutine_Attack()
    {
        bool fired = false;
        while (true)
        {
            if (CurrentTarget != null)
            {
                switch (abilityBase.abilityShotType)
                {
                    case AbilityShotType.PROJECTILE:
                        FireProjectile();
                        break;

                    case AbilityShotType.ARC_AOE:
                        FireArcAoe();
                        break;

                    case AbilityShotType.RADIAL_AOE:
                        FireRadialAoe();
                        break;

                    case AbilityShotType.NOVA_AOE:
                        FireRadialAoe();
                        break;
                }
                fired = true;
            }
            if (fired)
            {
                fired = false;
                yield return new WaitForSeconds(Cooldown);
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator FiringRoutine_Aura()
    {
        while (true)
        {
            switch (abilityBase.abilityType)
            {
                case AbilityType.AURA:
                    ApplyStatBonusBuff(AbilityOwner, EffectType.AURA_BUFF);
                    foreach (Actor target in targetList)
                        ApplyStatBonusBuff(target, EffectType.AURA_BUFF);
                    break;

                case AbilityType.SELF_BUFF:
                    ApplyStatBonusBuff(AbilityOwner, EffectType.SELF_BUFF);
                    break;
            }

            yield return new WaitForSeconds(0.25f);
        }
    }

    protected void ApplyStatBonusBuff(Actor target, EffectType effectType)
    {
        StatBonusBuffEffect buff = target.GetBuffStatusEffect(abilityBase.idName);
        List<Tuple<BonusType, ModifyType, int>> bonuses = new List<Tuple<BonusType, ModifyType, int>>();
        float strength = 0;
        foreach (AbilityScalingBonusProperty bonusProperty in abilityBase.bonusProperties)
        {
            int buffValue = (int)(bonusProperty.initialValue + bonusProperty.growthValue * abilityLevel);
            bonuses.Add(new Tuple<BonusType, ModifyType, int>(bonusProperty.bonusType, bonusProperty.modifyType, buffValue));
            strength += buffValue;
        }

        if (buff != null)
        {
            buff.RefreshDuration(0.5f);
            return;
        }
        else
        {
            target.AddStatusEffect(new StatBonusBuffEffect(target, AbilityOwner, bonuses, 0.5f, abilityBase.idName, effectType));
        }
    }

    protected void FireRadialAoe()
    {
        FireRadialAoe(abilityCollider.transform.position, CurrentTarget.transform.position);
    }

    protected void FireRadialAoe(Vector3 origin, Vector3 target)
    {
        Collider2D[] hits;
        ContactFilter2D filter = new ContactFilter2D
        {
            useTriggers = true
        };
        filter.SetLayerMask(LayerMask.GetMask("Enemy"));

        Vector2 forward = target - origin;
        if (abilityBase.abilityShotType == AbilityShotType.RADIAL_AOE)
        {
            hits = Physics2D.OverlapCircleAll(target, AreaRadius, targetMask);
            emitParams.position = target;
        }
        else
        {
            hits = Physics2D.OverlapCircleAll(origin, AreaRadius, targetMask);
            emitParams.position = origin;
        }

        ParticleManager.Instance.EmitAbilityParticle(abilityBase.idName, emitParams, AreaScaling);

        Dictionary<ElementType, float> damageDict = CalculateDamageDict();
        foreach (Collider2D hit in hits)
        {
            Actor actor = hit.gameObject.GetComponent<Actor>();
            if (actor != null)
            {
                actor.ApplyDamage(damageDict, abilityOnHitData, true);
            }
        }
    }

    protected void FireArcAoe()
    {
        FireArcAoe(abilityCollider.transform.position, CurrentTarget.transform.position);
    }

    protected void FireArcAoe(Vector3 origin, Vector3 target)
    {
        Collider2D[] hits;

        Vector2 forward = target - origin;
        float arc = (abilityBase.projectileSpread / 2f);
        hits = Physics2D.OverlapCircleAll(origin, AreaRadius, targetMask);

        emitParams.position = origin;
        float angle = Vector2.SignedAngle(Vector2.up, forward) + (180f - abilityBase.projectileSpread) / 2;
        ParticleManager.Instance.EmitAbilityParticleArc(abilityBase.idName, emitParams, AreaScaling, angle, AbilityOwner.transform);

        Dictionary<ElementType, float> damageDict = CalculateDamageDict();
        foreach (Collider2D hit in hits)
        {
            Actor actor = hit.gameObject.GetComponent<Actor>();
            if (actor != null)
            {
                Vector2 toActor = (actor.transform.position - origin);
                if (Vector2.Angle(toActor, forward) < arc)
                    actor.ApplyDamage(damageDict, abilityOnHitData, true);
            }
        }
    }

    protected void FireProjectile()
    {
        if (CurrentTarget.GetActorType() == ActorType.ENEMY)
        {
            EnemyActor enemy = CurrentTarget as EnemyActor;
            float enemySpeed = enemy.Data.movementSpeed;
            Tilemap tilemap = StageManager.Instance.PathTilemap;
            Vector3? nextNode;
            Vector2 currentPosition = abilityCollider.transform.position;
            float threshold = 2 + UnityEngine.Random.Range(0f, 1f);

            //Grab next movement steps for enemy then calculate enemy move time to
            //that node. If travel time of projectile and enemy of node are within
            //error then shoot toward that node.
            //Only calculate few nodes ahead for performance and game balance.
            for (int i = 0; i < 5; i++)
            {
                nextNode = enemy.GetMovementNode(i);
                if (nextNode != null)
                {
                    float enemyMoveTime = i / enemySpeed;
                    Vector2 projDirectionToNode = ((Vector2)nextNode - currentPosition).normalized * ProjectileSpeed * enemyMoveTime;
                    float distance = ((Vector2)nextNode - (currentPosition + projDirectionToNode)).sqrMagnitude;
                    if (distance < threshold)
                    {
                        FireProjectile(abilityCollider.transform.position, (Vector3)nextNode);
                        return;
                    }
                }
                else
                {
                    break;
                }
            }
            FireProjectile(abilityCollider.transform.position, CurrentTarget.transform.position);
        }
        else
        {
            FireProjectile(abilityCollider.transform.position, CurrentTarget.transform.position);
        }
    }

    protected void FireProjectile(Vector3 origin, Vector3 target)
    {
        Vector3 heading = (target - origin).normalized;
        heading.z = 0;

        bool isSpread = abilityBase.doesProjectileSpread;
        int angleMultiplier = 0;
        float spreadAngle = 17.5f;

        if (isSpread)
        {
            if (spreadAngle * ProjectileCount / 2f > abilityBase.projectileSpread)
            {
                spreadAngle = abilityBase.projectileSpread / ProjectileCount / 2f;
            }
        }
        else
        {
            spreadAngle = abilityBase.projectileSpread;
        }

        Dictionary<ElementType, float> damageDict = CalculateDamageDict();

        AbilityOnHitDataContainer OnHitCopy = abilityOnHitData.DeepCopy();

        for (int i = 0; i < ProjectileCount; i++)
        {
            Projectile pooledProjectile = GameManager.Instance.ProjectilePool.GetProjectile();
            pooledProjectile.transform.position = origin;
            pooledProjectile.timeToLive = 5f;
            pooledProjectile.currentSpeed = abilityBase.projectileSpeed;

            if (isSpread)
            {
                angleMultiplier = (int)Math.Round((i / 2f), MidpointRounding.AwayFromZero);
                if (i % 2 == 0)
                {
                    angleMultiplier *= -1;
                }

                pooledProjectile.currentHeading = Quaternion.Euler(0, 0, spreadAngle * angleMultiplier) * heading;
            }
            else
            {
                pooledProjectile.currentHeading = Quaternion.Euler(0, 0, spreadAngle * UnityEngine.Random.Range(-1f, 1f)) * heading;
            }

            if (abilityBase.hasLinkedAbility)
                pooledProjectile.linkedAbility = LinkedAbility;

            pooledProjectile.projectileDamage = damageDict;
            pooledProjectile.statusData = OnHitCopy;
            pooledProjectile.gameObject.layer = targetLayer;
            pooledProjectile.transform.up = (pooledProjectile.transform.position + pooledProjectile.currentHeading) - pooledProjectile.transform.position;
            pooledProjectile.abilityBase = abilityBase;
            pooledProjectile.pierceCount = ProjectilePierce;
            pooledProjectile.GetComponent<SpriteRenderer>().sprite = ResourceManager.Instance.GetSprite(abilityBase.idName);
        }
    }
}

public class AbilityOnHitDataContainer
{
    public class EffectData
    {
        public float chance = 0;
        public float effectiveness = 0;
        public float duration = 0;
    }

    public AbilityType Type;
    public Actor sourceActor;
    public float accuracy;
    public int physicalNegation;
    public int fireNegation;
    public int coldNegation;
    public int lightningNegation;
    public int earthNegation;
    public int divineNegation;
    public int voidNegation;

    public float vsBossDamage;

    private readonly Dictionary<EffectType, EffectData> effectData;

    public float GetEffectChance(EffectType type) => effectData[type].chance;

    public void SetEffectChance(EffectType type, float value) => effectData[type].chance = value;

    public float GetEffectEffectiveness(EffectType type) => effectData[type].effectiveness;

    public void SetEffectEffectiveness(EffectType type, float value) => effectData[type].effectiveness = value;

    public float GetEffectDuration(EffectType type) => effectData[type].duration;

    public void SetEffectDuration(EffectType type, float value) => effectData[type].duration = value;

    public EffectData GetEffectData(EffectType type) => effectData[type];

    public AbilityOnHitDataContainer()
    {
        effectData = new Dictionary<EffectType, EffectData>
        {
            { EffectType.BLEED, new EffectData() },
            { EffectType.BURN, new EffectData() },
            { EffectType.CHILL, new EffectData() },
            { EffectType.ELECTROCUTE, new EffectData() },
            { EffectType.FRACTURE, new EffectData() },
            { EffectType.PACIFY, new EffectData() },
            { EffectType.RADIATION, new EffectData() }
        };
    }

    public AbilityOnHitDataContainer DeepCopy()
    {
        return (AbilityOnHitDataContainer)MemberwiseClone();
    }

    public bool DidEffectProc(EffectType effectType)
    {
        float chance = GetEffectChance(effectType);
        if (chance <= 0)
        {
            return false;
        }

        if (chance >= 100)
        {
            return true;
        }
        return UnityEngine.Random.Range(0f, 100f) < chance ? true : false;
    }
}