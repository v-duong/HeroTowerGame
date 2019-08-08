using System;
using System.Collections;
using System.Collections.Generic;

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

    public AbilityStatusEffectDataContainer abilityStatusEffectData;

    public LinkedActorAbility LinkedAbility { get; private set; }
    private Dictionary<BonusType, StatBonus> abilityBonusProperties;

    public List<Actor> targetList = new List<Actor>();
    public Actor CurrentTarget { get; private set; }

    private bool firingRoutineRunning;
    private IEnumerator firingRoutine;

    public ActorAbility(AbilityBase ability, int layer)
    {
        if (ability == null)
            return;
        abilityBase = ability;
        abilityStatusEffectData = new AbilityStatusEffectDataContainer();
        abilityBonusProperties = new Dictionary<BonusType, StatBonus>();

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
        StatBonus penalty = new StatBonus();
        penalty.AddBonus(ModifyType.MULTIPLY, -30);
        abilityBonusProperties.Add(BonusType.GLOBAL_ABILITY_SPEED, penalty);
    }

    public void SetDamageAndSpeedModifier(float damage, float speed)
    {
        Debug.Log(speed);
        StatBonus damageMod = new StatBonus();
        StatBonus speedMod = new StatBonus();
        damageMod.AddBonus(ModifyType.MULTIPLY, damage);
        speedMod.AddBonus(ModifyType.MULTIPLY, speed);
        abilityBonusProperties.Add(BonusType.GLOBAL_DAMAGE, damageMod);
        abilityBonusProperties.Add(BonusType.GLOBAL_ABILITY_SPEED, speedMod);
    }

    public void SetAbilityOwner(Actor actor)
    {
        AbilityOwner = actor;
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
        UpdateAbilityBonusProperties();
        UpdateAbility_Damage(data, abilityBase.damageLevels);
        UpdateAbility_AbilityType(data);
        UpdateAbility_ShotType(data);
        UpdateAbility_StatusBonuses(data);
        if (abilityCollider != null)
            abilityCollider.abilityCollider.radius = TargetRange;
        if (LinkedAbility != null)
            LinkedAbility.UpdateAbilityStats(data);
    }

    public virtual void UpdateAbilityStats(EnemyData data)
    {
        UpdateAbilityBonusProperties();
        UpdateAbility_Damage(data, abilityBase.damageLevels);
        UpdateAbility_AbilityType(data);
        UpdateAbility_ShotType(data);
        UpdateAbility_StatusBonuses(data);
        if (abilityCollider != null)
            abilityCollider.abilityCollider.radius = TargetRange;
        if (LinkedAbility != null)
            LinkedAbility.UpdateAbilityStats(data);
    }

    protected void UpdateAbilityBonusProperties()
    {
        foreach (AbilityScalingBonusProperty bonusProperty in abilityBase.bonusProperties)
        {
            if (abilityBonusProperties.TryGetValue(bonusProperty.bonusType, out StatBonus temp))
            {
                temp.ResetBonus();
            }
            else
            {
                temp = new StatBonus();
                abilityBonusProperties.Add(bonusProperty.bonusType, temp);
            }
            temp.AddBonus(bonusProperty.modifyType, bonusProperty.initialValue + bonusProperty.growthValue * abilityLevel);
        }
    }

    protected void UpdateAbility_ShotType(ActorData data)
    {
        ProjectileSpeed = (float)GetTotalStatBonus(data, BonusType.PROJECTILE_SPEED).CalculateStat(abilityBase.projectileSpeed);
        ProjectilePierce = GetTotalStatBonus(data, BonusType.PROJECTILE_PIERCE).CalculateStat(0);
        ProjectileCount = GetTotalStatBonus(data, BonusType.PROJECTILE_COUNT).CalculateStat(abilityBase.projectileCount);

        AreaRadius = (float)GetTotalStatBonus(data, BonusType.AREA_RADIUS).CalculateStat(abilityBase.areaRadius) + 0.5f;
    }

    protected void UpdateAbility_AbilityType(HeroData data)
    {
        StatBonus critBonus = new StatBonus();
        StatBonus critDamageBonus = new StatBonus();

        if (abilityBase.abilityType == AbilityType.SPELL)
        {
            GetTotalStatBonus(data, critBonus, BonusType.SPELL_CRITICAL_CHANCE, BonusType.GLOBAL_CRITICAL_CHANCE);
            GetTotalStatBonus(data, critDamageBonus, BonusType.SPELL_CRITICAL_DAMAGE, BonusType.GLOBAL_CRITICAL_DAMAGE);

            StatBonus speedBonus = GetTotalStatBonus(data, BonusType.CAST_SPEED, BonusType.GLOBAL_ABILITY_SPEED);
            StatBonus rangeBonus = GetTotalStatBonus(data, BonusType.SPELL_RANGE);

            CriticalChance = (float)critBonus.CalculateStat(abilityBase.baseCritical);
            Cooldown = (float)(1 / speedBonus.CalculateStat(abilityBase.attacksPerSec));
            TargetRange = (float)rangeBonus.CalculateStat(abilityBase.targetRange) + 0.5f;
        }
        else if (abilityBase.abilityType == AbilityType.ATTACK)
        {
            StatBonus speedBonus = GetTotalStatBonus(data, BonusType.GLOBAL_ATTACK_SPEED, BonusType.GLOBAL_ABILITY_SPEED);
            StatBonus rangeBonus = new StatBonus();

            if (abilityBase.groupTypes.Contains(GroupType.MELEE_ATTACK))
            {
                GetTotalStatBonus(data, critBonus, BonusType.MELEE_WEAPON_CRITICAL_CHANCE, BonusType.ATTACK_CRITICAL_CHANCE);
                GetTotalStatBonus(data, critDamageBonus, BonusType.MELEE_WEAPON_CRITICAL_DAMAGE, BonusType.ATTACK_CRITICAL_DAMAGE);

                GetTotalStatBonus(data, rangeBonus, BonusType.MELEE_ATTACK_RANGE);
            }
            else if (abilityBase.groupTypes.Contains(GroupType.RANGED_ATTACK))
            {
                GetTotalStatBonus(data, critBonus, BonusType.RANGED_WEAPON_CRITICAL_CHANCE, BonusType.ATTACK_CRITICAL_CHANCE);
                GetTotalStatBonus(data, critBonus, BonusType.RANGED_WEAPON_CRITICAL_DAMAGE, BonusType.ATTACK_CRITICAL_DAMAGE);

                GetTotalStatBonus(data, rangeBonus, BonusType.RANGED_ATTACK_RANGE);
            }

            if (data.GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon weapon)
            {
                Cooldown = (float)(1 / speedBonus.CalculateStat(weapon.AttackSpeed));
                if (abilityBase.useWeaponRange)
                    TargetRange = (float)(rangeBonus.CalculateStat(weapon.WeaponRange)) + 0.5f;
                else
                    TargetRange = (float)(rangeBonus.CalculateStat(abilityBase.targetRange)) + 0.5f;
                CriticalChance = (float)critBonus.CalculateStat(weapon.CriticalChance);
            }
            else
            {
                //Unarmed default values
                if (abilityBase.useWeaponRange)
                    TargetRange = (float)rangeBonus.CalculateStat(0.5f) + 0.5f;
                else
                    TargetRange = (float)(rangeBonus.CalculateStat(abilityBase.targetRange)) + 0.5f;
                Cooldown = (float)speedBonus.CalculateStat(1f);
                CriticalChance = (float)critBonus.CalculateStat(3.5f);
            }
        }

        CriticalDamage = critDamageBonus.CalculateStat(50);
        
        if (float.IsInfinity(Cooldown))
            Cooldown = 0.001f;
    }

    protected void UpdateAbility_AbilityType(EnemyData data)
    {
        StatBonus critBonus = new StatBonus();
        StatBonus critDamageBonus = new StatBonus();

        if (abilityBase.abilityType == AbilityType.SPELL)
        {
            GetTotalStatBonus(data, critBonus, BonusType.GLOBAL_CRITICAL_CHANCE);
            GetTotalStatBonus(data, critDamageBonus, BonusType.GLOBAL_CRITICAL_DAMAGE);

            StatBonus speedBonus = GetTotalStatBonus(data, BonusType.CAST_SPEED, BonusType.GLOBAL_ABILITY_SPEED);
            StatBonus rangeBonus = GetTotalStatBonus(data, BonusType.SPELL_RANGE);

            CriticalChance = (float)critBonus.CalculateStat(abilityBase.baseCritical);
            Cooldown = (float)(1 / speedBonus.CalculateStat(abilityBase.attacksPerSec));
            TargetRange = (float)rangeBonus.CalculateStat(abilityBase.targetRange) + 0.5f;
        }
        else if (abilityBase.abilityType == AbilityType.ATTACK)
        {
            StatBonus speedBonus = GetTotalStatBonus(data, BonusType.GLOBAL_ATTACK_SPEED, BonusType.GLOBAL_ABILITY_SPEED);
            StatBonus rangeBonus = new StatBonus();

            GetTotalStatBonus(data, critBonus, BonusType.GLOBAL_CRITICAL_CHANCE);
            GetTotalStatBonus(data, critDamageBonus, BonusType.GLOBAL_CRITICAL_DAMAGE);

            CriticalChance = (float)critBonus.CalculateStat(data.BaseData.attackCriticalChance);
            Cooldown = (float)(1 / speedBonus.CalculateStat(data.BaseData.attackSpeed));
            TargetRange = (float)rangeBonus.CalculateStat(data.BaseData.attackTargetRange) + 0.5f;
        }
        CriticalDamage = critDamageBonus.CalculateStat(50);

        if (float.IsInfinity(Cooldown))
            Cooldown = 0.001f;
    }

    protected void UpdateAbility_Damage(HeroData data, Dictionary<ElementType, AbilityDamageBase> damageLevels, float damageModifier = 1.0f)
    {
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
            return;

        StatBonus minBonus, maxBonus, multiBonus;
        AbilityType abilityType = abilityBase.abilityType;
        var elementValues = Enum.GetValues(typeof(ElementType));

        foreach (ElementType e in elementValues)
        {
            float minDamage = 0, maxDamage = 0;

            if (damageLevels.ContainsKey(e))
            {
                MinMaxRange abilityBaseDamage = damageLevels[e].damage[abilityLevel];
                minDamage = abilityBaseDamage.min;
                maxDamage = abilityBaseDamage.max;
            }
            if (abilityBase.abilityType == AbilityType.ATTACK)
            {
                float weaponMulti = abilityBase.weaponMultiplier + abilityBase.weaponMultiplierScaling * abilityLevel;
                if (data.GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon weapon)
                {
                    MinMaxRange weaponDamage = weapon.GetWeaponDamage(e);
                    minDamage = weaponDamage.min * weaponMulti;
                    maxDamage = weaponDamage.max * weaponMulti;
                }
            }

            List<BonusType> min = new List<BonusType>();
            List<BonusType> max = new List<BonusType>();
            List<BonusType> multi = new List<BonusType>();

            Helpers.GetDamageTypes(e, abilityType, abilityBase.abilityShotType, abilityBase.groupTypes, min, max, multi);

            minBonus = GetTotalStatBonus(data, min.ToArray());
            maxBonus = GetTotalStatBonus(data, max.ToArray());
            multiBonus = GetTotalStatBonus(data, multi.ToArray());

            minBonus = GetTotalStatBonus(data, min.ToArray());
            maxBonus = GetTotalStatBonus(data, max.ToArray());
            multiBonus = GetTotalStatBonus(data, multi.ToArray());

            minDamage += (minBonus.CalculateStat(0) * abilityBase.flatDamageMultiplier);
            maxDamage += (maxBonus.CalculateStat(0) * abilityBase.flatDamageMultiplier);

            minDamage = (float)multiBonus.CalculateStat(minDamage);
            maxDamage = (float)multiBonus.CalculateStat(maxDamage);

            MinMaxRange newDamageRange = new MinMaxRange
            {
                min = (int)(minDamage * damageModifier),
                max = (int)(maxDamage * damageModifier)
            };

            if (newDamageRange.IsZero())
                damageBase.Remove(e);
            else
            {
                damageBase[e] = newDamageRange;
            }
        }
    }

    protected void UpdateAbility_Damage(EnemyData data, Dictionary<ElementType, AbilityDamageBase> damageLevels, float damageModifier = 1.0f)
    {
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
            return;

        StatBonus minBonus, maxBonus, multiBonus;
        AbilityType abilityType = abilityBase.abilityType;
        var elementValues = Enum.GetValues(typeof(ElementType));

        foreach (ElementType e in elementValues)
        {
            float minDamage = 0, maxDamage = 0;

            if (damageLevels.ContainsKey(e))
            {
                MinMaxRange abilityBaseDamage = damageLevels[e].damage[abilityLevel];
                minDamage = abilityBaseDamage.min;
                maxDamage = abilityBaseDamage.max;
            }
            else if (abilityBase.abilityType == AbilityType.ATTACK)
            {
                float weaponMulti = abilityBase.weaponMultiplier + abilityBase.weaponMultiplierScaling * abilityLevel;
                minDamage = data.minAttackDamage;
                maxDamage = data.maxAttackDamage;
            }

            List<BonusType> min = new List<BonusType>();
            List<BonusType> max = new List<BonusType>();
            List<BonusType> multi = new List<BonusType>();

            Helpers.GetDamageTypes(e, abilityType, abilityBase.abilityShotType, abilityBase.groupTypes, min, max, multi);

            minBonus = GetTotalStatBonus(data, min.ToArray());
            maxBonus = GetTotalStatBonus(data, max.ToArray());
            multiBonus = GetTotalStatBonus(data, multi.ToArray());

            minDamage += (minBonus.CalculateStat(0) * abilityBase.flatDamageMultiplier);
            maxDamage += (maxBonus.CalculateStat(0) * abilityBase.flatDamageMultiplier);

            minDamage = (float)multiBonus.CalculateStat(minDamage);
            maxDamage = (float)multiBonus.CalculateStat(maxDamage);

            MinMaxRange newDamageRange = new MinMaxRange
            {
                min = (int)(minDamage * damageModifier),
                max = (int)(maxDamage * damageModifier)
            };

            if (newDamageRange.IsZero())
                damageBase.Remove(e);
            else
            {
                damageBase[e] = newDamageRange;
            }
        }
    }

    protected void UpdateAbility_StatusBonuses(ActorData data)
    {
        StatBonus bleedChance = GetTotalStatBonus(data, BonusType.BLEED_CHANCE, BonusType.STATUS_EFFECT_CHANCE);
        abilityStatusEffectData.bleedChance = (float)bleedChance.CalculateStat(0f);
        StatBonus bleedEffectiveness = GetTotalStatBonus(data, BonusType.BLEED_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME);
        abilityStatusEffectData.bleedEffectiveness = (float)(bleedEffectiveness.CalculateStat(100f) / 100d);
        StatBonus bleedDuration = GetTotalStatBonus(data, BonusType.BLEED_DURATION, BonusType.STATUS_EFFECT_DURATION);
        abilityStatusEffectData.bleedDuration = (float)bleedDuration.CalculateStat(4f);

        StatBonus burnChance = GetTotalStatBonus(data, BonusType.BURN_CHANCE, BonusType.STATUS_EFFECT_CHANCE);
        abilityStatusEffectData.burnChance = (float)burnChance.CalculateStat(0f);
        StatBonus burnEffectiveness = GetTotalStatBonus(data, BonusType.BURN_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME);
        abilityStatusEffectData.burnEffectiveness = (float)(burnEffectiveness.CalculateStat(100f) / 100d);
        StatBonus burnDuration = GetTotalStatBonus(data, BonusType.BURN_DURATION, BonusType.STATUS_EFFECT_DURATION);
        abilityStatusEffectData.burnDuration = (float)burnDuration.CalculateStat(2f);

        StatBonus chillChance = GetTotalStatBonus(data, BonusType.CHILL_CHANCE, BonusType.STATUS_EFFECT_CHANCE);
        abilityStatusEffectData.chillChance = (float)chillChance.CalculateStat(0f);
        StatBonus chillEffectiveness = GetTotalStatBonus(data, BonusType.CHILL_EFFECTIVENESS, BonusType.NONDAMAGE_STATUS_EFFECTIVENESS);
        abilityStatusEffectData.chillEffectiveness = (float)(chillEffectiveness.CalculateStat(100f) / 100d);
        StatBonus chillDuration = GetTotalStatBonus(data, BonusType.CHILL_DURATION, BonusType.STATUS_EFFECT_DURATION);
        abilityStatusEffectData.chillDuration = (float)chillDuration.CalculateStat(2f);

        StatBonus electrocuteChance = GetTotalStatBonus(data, BonusType.ELECTROCUTE_CHANCE, BonusType.STATUS_EFFECT_CHANCE);
        abilityStatusEffectData.electrocuteChance = (float)electrocuteChance.CalculateStat(0f);
        StatBonus electrocuteEffectiveness = GetTotalStatBonus(data, BonusType.ELECTROCUTE_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE);
        abilityStatusEffectData.electrocuteEffectiveness = (float)(electrocuteEffectiveness.CalculateStat(100f) / 100d);
        StatBonus electrocuteDuration = GetTotalStatBonus(data, BonusType.ELECTROCUTE_DURATION, BonusType.STATUS_EFFECT_DURATION);
        abilityStatusEffectData.electrocuteDuration = (float)electrocuteDuration.CalculateStat(2f);

        StatBonus fractureChance = GetTotalStatBonus(data, BonusType.FRACTURE_CHANCE, BonusType.STATUS_EFFECT_CHANCE);
        abilityStatusEffectData.fractureChance = (float)fractureChance.CalculateStat(0f);
        StatBonus fractureEffectiveness = GetTotalStatBonus(data, BonusType.FRACTURE_EFFECTIVENESS, BonusType.NONDAMAGE_STATUS_EFFECTIVENESS);
        abilityStatusEffectData.fractureEffectiveness = (float)(fractureEffectiveness.CalculateStat(100f) / 100d);
        StatBonus fractureDuration = GetTotalStatBonus(data, BonusType.FRACTURE_DURATION, BonusType.STATUS_EFFECT_DURATION);
        abilityStatusEffectData.fractureDuration = (float)fractureDuration.CalculateStat(3f);

        StatBonus pacifyChance = GetTotalStatBonus(data, BonusType.PACIFY_CHANCE, BonusType.STATUS_EFFECT_CHANCE);
        abilityStatusEffectData.pacifyChance = (float)pacifyChance.CalculateStat(0f);
        StatBonus pacifyEffectiveness = GetTotalStatBonus(data, BonusType.PACIFY_EFFECTIVENESS, BonusType.NONDAMAGE_STATUS_EFFECTIVENESS);
        abilityStatusEffectData.pacifyEffectiveness = (float)(pacifyEffectiveness.CalculateStat(100f) / 100d);
        StatBonus pacifyDuration = GetTotalStatBonus(data, BonusType.PACIFY_DURATION, BonusType.STATUS_EFFECT_DURATION);
        abilityStatusEffectData.pacifyDuration = (float)pacifyDuration.CalculateStat(3f);

        StatBonus radiationChance = GetTotalStatBonus(data, BonusType.RADIATION_CHANCE, BonusType.STATUS_EFFECT_CHANCE);
        abilityStatusEffectData.radiationChance = (float)radiationChance.CalculateStat(0f);
        StatBonus radiationEffectiveness = GetTotalStatBonus(data, BonusType.RADIATION_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME);
        abilityStatusEffectData.radiationEffectiveness = (float)(radiationEffectiveness.CalculateStat(100f) / 100d);
        StatBonus radiationDuration = GetTotalStatBonus(data, BonusType.RADIATION_DURATION, BonusType.STATUS_EFFECT_DURATION);
        abilityStatusEffectData.radiationDuration = (float)radiationDuration.CalculateStat(5f);

        StatBonus vsBossDamage = GetTotalStatBonus(data, BonusType.DAMAGE_VS_BOSS);
        abilityStatusEffectData.vsBossDamage = 1f + ((float)vsBossDamage.CalculateStat(0f) / 100f);
    }

    protected void GetTotalStatBonus(ActorData data, StatBonus existingBonus, params BonusType[] types)
    {
        foreach (BonusType bonusType in types)
        {
            data.GetTotalStatBonus(bonusType, abilityBonusProperties, existingBonus);
        }
    }

    protected StatBonus GetTotalStatBonus(ActorData data, params BonusType[] types)
    {
        StatBonus bonus = new StatBonus();
        foreach (BonusType bonusType in types)
        {
            data.GetTotalStatBonus(bonusType, abilityBonusProperties, bonus);
        }
        return bonus;
    }

    public Dictionary<ElementType, double> CalculateDamageDict()
    {
        var values = Enum.GetValues(typeof(ElementType));
        double damage = 0;
        bool isCrit = false;
        Dictionary<ElementType, double> dict = new Dictionary<ElementType, double>();
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
                    damage = damage * (1d + (CriticalDamage / 100d));
                dict.Add(elementType, damage);
            }
        }
        return dict;
    }

    public void StartFiring(Actor parent)
    {
        if (firingRoutine == null)
        {
            firingRoutine = FiringRoutine();
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

    private IEnumerator FiringRoutine()
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
        }
        else
        {
            hits = Physics2D.OverlapCircleAll(origin, AreaRadius, targetMask);
        }

        Dictionary<ElementType, double> damageDict = CalculateDamageDict();
        foreach (Collider2D hit in hits)
        {
            Actor actor = hit.gameObject.GetComponent<Actor>();
            if (actor != null)
            {
                actor.ApplyDamage(damageDict, abilityStatusEffectData);
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
        double arc = (abilityBase.projectileSpread / 2d);
        hits = Physics2D.OverlapCircleAll(origin, AreaRadius, targetMask);

        Dictionary<ElementType, double> damageDict = CalculateDamageDict();
        foreach (Collider2D hit in hits)
        {
            Actor actor = hit.gameObject.GetComponent<Actor>();
            if (actor != null)
            {
                Vector2 toActor = (actor.transform.position - origin);
                if (Vector2.Angle(toActor, forward) < arc)
                    actor.ApplyDamage(damageDict, abilityStatusEffectData);
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

        Dictionary<ElementType, double> damageDict = CalculateDamageDict();
        for (int i = 0; i < ProjectileCount; i++)
        {
            Projectile pooledProjectile = GameManager.Instance.ProjectilePool.GetProjectile();
            pooledProjectile.transform.position = origin;
            pooledProjectile.timeToLive = 2.5f;
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
            pooledProjectile.statusData = abilityStatusEffectData.DeepCopy();
            pooledProjectile.gameObject.layer = targetLayer;
        }
    }
}

public class AbilityStatusEffectDataContainer
{
    public float bleedChance;
    public float bleedEffectiveness;
    public float bleedDuration;
    public float burnChance;
    public float burnEffectiveness;
    public float burnDuration;
    public float chillChance;
    public float chillEffectiveness;
    public float chillDuration;
    public float electrocuteChance;
    public float electrocuteEffectiveness;
    public float electrocuteDuration;
    public float fractureChance;
    public float fractureEffectiveness;
    public float fractureDuration;
    public float pacifyChance;
    public float pacifyEffectiveness;
    public float pacifyDuration;
    public float radiationChance;
    public float radiationEffectiveness;
    public float radiationDuration;
    public float vsBossDamage;

    public AbilityStatusEffectDataContainer DeepCopy()
    {
        return (AbilityStatusEffectDataContainer)this.MemberwiseClone();
    }

    public bool DidBleedProc()
    {
        if (bleedChance <= 0)
        {
            return false;
        }

        if (bleedChance >= 100)
        {
            return true;
        }
        return UnityEngine.Random.Range(0f, 100f) <= bleedChance ? true : false;
    }

    public bool DidBurnProc()
    {
        if (burnChance <= 0)
        {
            return false;
        }

        if (burnChance >= 100)
        {
            return true;
        }
        return UnityEngine.Random.Range(0f, 100f) <= burnChance ? true : false;
    }

    public bool DidChillProc()
    {
        if (chillChance <= 0)
        {
            return false;
        }

        if (chillChance >= 100)
        {
            return true;
        }
        return UnityEngine.Random.Range(0f, 100f) <= chillChance ? true : false;
    }

    public bool DidElectrocuteProc()
    {
        if (electrocuteChance <= 0)
        {
            return false;
        }

        if (electrocuteChance >= 100)
        {
            return true;
        }
        return UnityEngine.Random.Range(0f, 100f) <= electrocuteChance ? true : false;
    }

    public bool DidFractureProc()
    {
        if (fractureChance <= 0)
        {
            return false;
        }
        if (fractureChance >= 100)
        {
            return true;
        }
        return UnityEngine.Random.Range(0f, 100f) <= fractureChance ? true : false;
    }

    public bool DidPacifyProc()
    {
        if (pacifyChance <= 0)
        {
            return false;
        }
        if (pacifyChance >= 100)
        {
            return true;
        }
        return UnityEngine.Random.Range(0f, 100f) <= pacifyChance ? true : false;
    }

    public bool DidRadiationProc()
    {
        if (radiationChance <= 0)
        {
            return false;
        }
        if (radiationChance >= 100)
        {
            return true;
        }
        return UnityEngine.Random.Range(0f, 100f) <= radiationChance ? true : false;
    }
}