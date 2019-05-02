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
    public float AreaLength { get; private set; }
    public float AreaRadius { get; private set; }
    public float Cooldown { get; private set; }
    public float HitscanDelay { get; private set; }
    public float ProjectileSize { get; private set; }
    public float ProjectileSpeed { get; private set; }
    public int ProjectilePierce { get; private set; }
    public float CriticalChance { get; private set; }
    public float TargetRange { get; private set; }
    public int ProjectileCount { get; private set; }
    public LinkedActorAbility LinkedAbility { get; private set; }

    public List<Actor> targetList = new List<Actor>();
    public Actor CurrentTarget { get; private set; }

    private bool firingRoutineRunning;
    private IEnumerator firingRoutine;

    public ActorAbility(AbilityBase ability)
    {
        if (ability == null)
            return;
        abilityBase = ability;

        AreaLength = abilityBase.areaLength;
        AreaRadius = abilityBase.areaRadius;
        Cooldown = 1 / abilityBase.attacksPerSec;
        HitscanDelay = abilityBase.hitscanDelay;
        ProjectileSize = abilityBase.projectileSize;
        ProjectileSpeed = abilityBase.projectileSpeed;

        CriticalChance = abilityBase.baseCritical;
        ProjectileCount = abilityBase.projectileCount;
        TargetRange = abilityBase.targetRange;

        if (abilityBase.hasLinkedAbility)
        {
            if (abilityBase.linkedAbility.inheritsDamage)
                LinkedAbility = new LinkedActorAbility(ability.linkedAbility, ability.damageLevels);
            else
                LinkedAbility = new LinkedActorAbility(ability.linkedAbility);
            LinkedAbility.abilityLevel = abilityLevel;
        }
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
        UpdateAbilityDamage(data, abilityBase.damageLevels);
        UpdateAbility_AbilityType(data);
        UpdateAbility_ShotType(data);
        if (LinkedAbility != null)
            LinkedAbility.UpdateAbilityStats(data);
    }

    protected void UpdateAbility_ShotType(HeroData data)
    {
        if (abilityBase.abilityShotType == AbilityShotType.PROJECTILE)
        {
            StatBonus projSpeedBonus = data.GetTotalStatBonus(BonusType.PROJECTILE_SPEED);
            StatBonus projCountBonus = data.GetTotalStatBonus(BonusType.PROJECTILE_COUNT);
            StatBonus projPierceBonus = data.GetTotalStatBonus(BonusType.PROJECTILE_PIERCE);

            ProjectileSpeed = (float)projSpeedBonus.CalculateStat(abilityBase.projectileSpeed);
            ProjectilePierce = projPierceBonus.CalculateStat(0);
            ProjectileCount = projSpeedBonus.CalculateStat(abilityBase.projectileCount);
        }
    }

    protected void UpdateAbility_AbilityType(HeroData data)
    {
        if (abilityBase.abilityType == AbilityType.SPELL)
        {
            StatBonus critBonus = data.GetTotalStatBonus(BonusType.GLOBAL_CRITICAL_CHANCE);
            data.GetTotalStatBonus(BonusType.SPELL_CRITICAL_CHANCE, critBonus);

            StatBonus speedBonus = data.GetTotalStatBonus(BonusType.CAST_SPEED);
            StatBonus rangeBonus = data.GetTotalStatBonus(BonusType.SPELL_RANGE);

            CriticalChance = (float)critBonus.CalculateStat(abilityBase.baseCritical);
            Cooldown = (float)(1 / speedBonus.CalculateStat(abilityBase.attacksPerSec));
            TargetRange = (float)rangeBonus.CalculateStat(abilityBase.targetRange);
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
                Cooldown = (float)(1 / speedBonus.CalculateStat(weapon.AttackSpeed));
                TargetRange = (float)(rangeBonus.CalculateStat(weapon.WeaponRange));
                CriticalChance = (float)critBonus.CalculateStat(weapon.CriticalChance);
            }
            else
            {
                TargetRange = (float)rangeBonus.CalculateStat(0.5f);
                Cooldown = (float)speedBonus.CalculateStat(1f);
                CriticalChance = (float)critBonus.CalculateStat(3.5f);
            }
        }

        if (float.IsInfinity(Cooldown))
            Cooldown = 0.001f;
    }

    protected void UpdateAbilityDamage(HeroData data, Dictionary<ElementType, AbilityDamageBase> damageLevels, float damageModifier = 1.0f)
    {
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
            return;

        StatBonus minBonus, maxBonus, multiBonus;
        AbilityType abilityType = abilityBase.abilityType;
        var elementValues = Enum.GetValues(typeof(ElementType));

        foreach (ElementType e in elementValues)
        {
            MinMaxRange newDamageRange = new MinMaxRange();

            if (damageLevels.ContainsKey(e))
            {
                MinMaxRange abilityBaseDamage = damageLevels[e].damage[abilityLevel];
                newDamageRange.min = abilityBaseDamage.min;
                newDamageRange.max = abilityBaseDamage.max;
            }
            if (abilityBase.abilityType == AbilityType.ATTACK)
            {
                float weaponMulti = abilityBase.weaponMultiplier + abilityBase.weaponMultiplierScaling * abilityLevel;
                if (data.GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon weapon)
                {
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

            newDamageRange.min = (int)(minBonus.CalculateStat(newDamageRange.min) * abilityBase.flatDamageMultiplier);
            newDamageRange.max = (int)(maxBonus.CalculateStat(newDamageRange.max) * abilityBase.flatDamageMultiplier);

            newDamageRange.min = multiBonus.CalculateStat(newDamageRange.min);
            newDamageRange.max = multiBonus.CalculateStat(newDamageRange.max);

            newDamageRange.min = (int)(newDamageRange.min * damageModifier);
            newDamageRange.max = (int)(newDamageRange.max * damageModifier);

            if (newDamageRange.IsZero())
                damageBase.Remove(e);
            else
            {
                damageBase[e] = newDamageRange;
            }
        }
    }

    public Dictionary<ElementType, int> CalculateDamageDict()
    {
        var values = Enum.GetValues(typeof(ElementType));
        int d = 0;
        Dictionary<ElementType, int> dict = new Dictionary<ElementType, int>();
        foreach (ElementType e in values)
        {
            if (damageBase.ContainsKey(e))
            {
                d = UnityEngine.Random.Range(damageBase[e].min, damageBase[e].max + 1);
                dict.Add(e, d);
            }
        }
        return dict;
    }

    public int CalculateDamageTotalValue()
    {
        int total = 0;
        for (int i = 0; i < (int)ElementType.COUNT; i++)
        {
            if (damageBase.ContainsKey((ElementType)i))
            {
                total += UnityEngine.Random.Range(damageBase[(ElementType)i].min, damageBase[(ElementType)i].max + 1);
            }
        }
        return total;
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
        FireRadialAoe(abilityCollider.transform.position, CurrentTarget.transform.position, CalculateDamageDict());
    }

    protected void FireRadialAoe(Vector3 origin, Vector3 target, Dictionary<ElementType, int> damageDict)
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        ContactFilter2D filter = new ContactFilter2D
        {
            useTriggers = true
        };
        filter.SetLayerMask(LayerMask.GetMask("Enemy"));

        Vector2 forward = target - origin;

        if (abilityBase.abilityShotType == AbilityShotType.RADIAL_AOE)
        {
            Physics2D.CircleCast(target, AreaRadius, -forward, filter, hits);
        }
        else
        {
            Physics2D.CircleCast(origin, AreaRadius, -forward, filter, hits);
        }

        foreach (RaycastHit2D hit in hits)
        {
            Actor actor = hit.collider.gameObject.GetComponent<Actor>();
            if (actor != null)
            {
                actor.ApplyDamage(CalculateDamageTotalValue());
            }
        }
    }

    protected void FireArcAoe()
    {
        FireArcAoe(abilityCollider.transform.position, CurrentTarget.transform.position, CalculateDamageDict());
    }

    protected void FireArcAoe(Vector3 origin, Vector3 target, Dictionary<ElementType, int> damageDict)
    {
        List<RaycastHit2D> hits = new List<RaycastHit2D>();
        ContactFilter2D filter = new ContactFilter2D
        {
            useTriggers = true
        };
        filter.SetLayerMask(LayerMask.GetMask("Enemy"));

        Vector2 forward = target - origin;
        double arc = (-abilityBase.projectileSpread / 180) + 1;

        Physics2D.CircleCast(origin, AreaRadius, forward, filter, hits);

        foreach (RaycastHit2D hit in hits)
        {
            Actor actor = hit.collider.gameObject.GetComponent<Actor>();
            if (actor != null)
            {
                Vector2 toActor = (actor.transform.position - origin);
                if (Vector2.Dot(toActor, forward) > arc)
                    actor.ApplyDamage(CalculateDamageTotalValue());
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
                        FireProjectile(abilityCollider.transform.position, (Vector3)nextNode, CalculateDamageDict());
                        return;
                    }
                }
                else
                {
                    break;
                }
            }
            FireProjectile(abilityCollider.transform.position, CurrentTarget.transform.position, CalculateDamageDict());
        }
        else
        {
            FireProjectile(abilityCollider.transform.position, CurrentTarget.transform.position, CalculateDamageDict());
        }
    }

    protected void FireProjectile(Vector3 origin, Vector3 target, Dictionary<ElementType, int> damageDict)
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

        for (int i = 0; i < ProjectileCount; i++)
        {
            var p = GameManager.Instance.ProjectilePool.GetProjectile();
            p.transform.position = origin;
            p.timeToLive = 2.5f;
            p.currentSpeed = abilityBase.projectileSpeed;

            if (isSpread)
            {
                angleMultiplier = (int)Math.Round((i / 2f), MidpointRounding.AwayFromZero);
                if (i % 2 == 0)
                {
                    angleMultiplier *= -1;
                }

                p.currentHeading = Quaternion.Euler(0, 0, spreadAngle * angleMultiplier) * heading;
            }
            else
            {
                p.currentHeading = Quaternion.Euler(0, 0, spreadAngle * UnityEngine.Random.Range(-1f, 1f)) * heading;
            }

            if (abilityBase.hasLinkedAbility)
                p.linkedAbility = LinkedAbility;

            p.projectileDamage = damageDict;
        }
    }
}