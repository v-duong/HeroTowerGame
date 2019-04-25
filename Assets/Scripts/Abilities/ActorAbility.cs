using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ActorAbility
{
    public readonly AbilityBase abilityBase;
    public int abilityLevel;
    public Collider2D collider;
    public Dictionary<ElementType, MinMaxRange> damageBase = new Dictionary<ElementType, MinMaxRange>();
    public AbilityTargetType targetType;
    public float areaLength;
    public float areaRadius;
    public float cooldown;
    public float hitscanDelay;
    public float projectileSize;
    public float projectileSpeed;
    public float targetRange;
    public float criticalChance;
    public int projectileCount;

    private bool firingRoutineRunning;
    private IEnumerator firingRoutine;
    private ContactFilter2D contactFilter;

    public ActorAbility(AbilityBase ability)
    {
        if (ability == null)
            return;
        abilityBase = ability;

        areaLength = abilityBase.areaLength;
        areaRadius = abilityBase.areaRadius;
        cooldown = 1 / abilityBase.attacksPerSec;
        hitscanDelay = abilityBase.hitscanDelay;
        projectileSize = abilityBase.projectileSize;
        projectileSpeed = abilityBase.projectileSpeed;
        targetRange = abilityBase.targetRange;
        projectileCount = abilityBase.projectileCount;
        criticalChance = abilityBase.baseCritical;

        contactFilter.useLayerMask = true;
        contactFilter.useTriggers = false;

        if (ability.targetType == AbilityTargetType.ENEMY)
        {
            contactFilter.SetLayerMask(LayerMask.GetMask("Enemy"));
        }
        else if (ability.targetType == AbilityTargetType.ALLY)
        {
            contactFilter.SetLayerMask(LayerMask.GetMask("Hero"));
        }
        else if (ability.targetType == AbilityTargetType.ALLY)
        {
            contactFilter.SetLayerMask(LayerMask.GetMask("Enemy", "Hero"));
        }
    }

    public void UpdateAbilityStats(HeroData data)
    {
        UpdateAbilityDamage(data);
        UpdateAbilityNonDamageStats(data);
    }

    private void UpdateAbilityNonDamageStats(HeroData data)
    {
        StatBonus bonus;
        if (abilityBase.abilityType == AbilityType.SPELL)
        {
            StatBonus critBonus = data.GetTotalStatBonus(BonusType.GLOBAL_CRITICAL_CHANCE);
            data.GetTotalStatBonus(BonusType.SPELL_CRITICAL_CHANCE, critBonus);

            StatBonus speedBonus = data.GetTotalStatBonus(BonusType.CAST_SPEED);
            StatBonus rangeBonus = data.GetTotalStatBonus(BonusType.SPELL_RANGE);

            criticalChance = (float)critBonus.CalculateStat(abilityBase.baseCritical);
            cooldown = (float)(1 / speedBonus.CalculateStat(abilityBase.attacksPerSec));
            targetRange = (float)rangeBonus.CalculateStat(abilityBase.targetRange);
        }
        else if (abilityBase.abilityType == AbilityType.ATTACK)
        {

            StatBonus critBonus = data.GetTotalStatBonus(BonusType.GLOBAL_CRITICAL_CHANCE);
            data.GetTotalStatBonus(BonusType.ATTACK_CRITICAL_CHANCE, critBonus);

            StatBonus speedBonus = data.GetTotalStatBonus(BonusType.GLOBAL_ATTACK_SPEED);

            StatBonus rangeBonus = new StatBonus();
            if (abilityBase.groupTypes.Contains(GroupType.MELEE_ATTACK))
            {
                data.GetTotalStatBonus(BonusType.MELEE_ATTACK_RANGE, rangeBonus);
            }
            else if (abilityBase.groupTypes.Contains(GroupType.RANGED_ATTACK))
            {
                data.GetTotalStatBonus(BonusType.RANGED_ATTACK_RANGE, rangeBonus);
            }

            if (data.GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon weapon)
            {
                cooldown = (float)(1 / speedBonus.CalculateStat(weapon.AttackSpeed));
                targetRange = (float)(rangeBonus.CalculateStat(weapon.WeaponRange));
                criticalChance = (float)critBonus.CalculateStat(weapon.CriticalChance);
            }
            else
            {
                targetRange = (float)rangeBonus.CalculateStat(0.5f);
                cooldown = (float)speedBonus.CalculateStat(1f);
                criticalChance = (float)critBonus.CalculateStat(3.5f);
            }
        }
    }

    private void UpdateAbilityDamage(HeroData data)
    {
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
            return;

        StatBonus minBonus, maxBonus, multiBonus;
        AbilityType abilityType = abilityBase.abilityType;
        var elementValues = Enum.GetValues(typeof(ElementType));

        foreach (ElementType e in elementValues)
        {
            MinMaxRange newDamageRange = new MinMaxRange();

            if (abilityBase.damageLevels.ContainsKey(e))
            {
                MinMaxRange abilityBaseDamage = abilityBase.GetDamageAtLevel(e, abilityLevel);
                newDamageRange.min = abilityBaseDamage.min;
                newDamageRange.max = abilityBaseDamage.max;
            }
            if (abilityBase.abilityType == AbilityType.ATTACK)
            {
                if (data.GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon weapon)
                {
                    float weaponMulti = abilityBase.weaponMultiplier + abilityBase.weaponMultiplierScaling * abilityLevel;
                    MinMaxRange weaponDamage = weapon.GetWeaponDamage(e);
                    newDamageRange.min = (int)(weaponDamage.min * weaponMulti);
                    newDamageRange.max = (int)(weaponDamage.max * weaponMulti);
                }
            }

            List<BonusType> min = new List<BonusType>();
            List<BonusType> max = new List<BonusType>();
            List<BonusType> multi = new List<BonusType>();
            minBonus = new StatBonus();
            maxBonus = new StatBonus();
            multiBonus = new StatBonus();

            Helpers.GetDamageTypes(e, abilityType, abilityBase.abilityShotType, abilityBase.groupTypes, min, max, multi);

            foreach (BonusType bonusType in min)
            {
                data.GetTotalStatBonus(bonusType, minBonus);
            }
            foreach (BonusType bonusType in max)
            {
                data.GetTotalStatBonus(bonusType, maxBonus);
            }
            foreach (BonusType bonusType in multi)
            {
                data.GetTotalStatBonus(bonusType, multiBonus);
            }

            newDamageRange.min = minBonus.CalculateStat(newDamageRange.min);
            newDamageRange.max = minBonus.CalculateStat(newDamageRange.max);

            newDamageRange.min = multiBonus.CalculateStat(newDamageRange.min);
            newDamageRange.max = multiBonus.CalculateStat(newDamageRange.max);

            if (newDamageRange.IsZero())
                damageBase.Remove(e);
            else
            {
                damageBase[e] = newDamageRange;
            }
        }
    }

    public void CalculateDamage(Dictionary<ElementType, int> dict)
    {
        var values = Enum.GetValues(typeof(ElementType));
        int d = 0;
        foreach (ElementType e in values)
        {
            if (damageBase.ContainsKey(e))
            {
                d = UnityEngine.Random.Range(damageBase[e].min, damageBase[e].max + 1);
                dict.Add(e, d);
            }
        }
    }

    public bool StartFiring(Actor parent)
    {
        if (firingRoutineRunning)
            return false;
        else
        {
            firingRoutine = FiringRoutine();
            parent.StartCoroutine(firingRoutine);
            firingRoutineRunning = true;
            return true;
        }
    }

    public bool StopFiring(Actor parent)
    {
        if (firingRoutineRunning)
        {
            parent.StopCoroutine(firingRoutine);
            firingRoutineRunning = false;
            return true;
        }
        else
            return false;
    }

    private IEnumerator FiringRoutine()
    {
        bool fired = false;
        Collider2D[] results = new Collider2D[10];
        while (true)
        {
            if (collider.OverlapCollider(contactFilter, results) != 0)
            {
                var p = GameManager.Instance.ProjectilePool.GetProjectile();
                p.transform.position = this.collider.transform.position;
                p.currentHeading = results[0].transform.position - this.collider.transform.position;
                p.currentHeading.z = 0;
                p.currentSpeed = abilityBase.projectileSpeed;
                CalculateDamage(p.projectileDamage);
                fired = true;
            }
            if (fired)
            {
                fired = false;
                yield return new WaitForSeconds(cooldown);
            }
            else
            {
                yield return null;
            }
        }
    }
}