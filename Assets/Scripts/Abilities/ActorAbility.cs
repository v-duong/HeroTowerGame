using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActorAbility
{
    public readonly AbilityBase abilityBase;
    public int abilityLevel;
    public AbilityColliderContainer abilityCollider;
    public Dictionary<ElementType, AbilityDamageContainer> mainDamageBase = new Dictionary<ElementType, AbilityDamageContainer>();
    public Dictionary<ElementType, AbilityDamageContainer> offhandDamageBase = new Dictionary<ElementType, AbilityDamageContainer>();
    public int targetLayer;
    public int targetMask;
    public bool isSecondaryAbility;
    protected float finalDamageModifier = 1.0f;

    public Actor AbilityOwner { get; private set; }
    public float AreaLength { get; private set; }
    public float AreaRadius { get; private set; }
    public float Cooldown { get; private set; }
    public float HitscanDelay { get; private set; }
    public float ProjectileSize { get; private set; }
    public float ProjectileSpeed { get; private set; }
    public int ProjectilePierce { get; private set; }
    public int ProjectileChain { get; private set; }
    public float MainCriticalChance { get; private set; }
    public float OffhandCriticalChance { get; private set; }
    public float MainCriticalDamage { get; private set; }
    public float OffhandCriticalDamage { get; private set; }
    public float TargetRange { get; private set; }
    public int ProjectileCount { get; private set; }
    public bool IsUsable { get; private set; }
    private bool attackWithMainHand;
    public bool AlternatesAttacks { get; private set; }
    public bool DualWielding { get; private set; }
    public EffectType BuffType { get; private set; }
    private AuraBuffBonusContainer auraBuffBonus;

    private float AreaScaling;
    private float ProjectileScaling;
    private readonly Dictionary<BonusType, StatBonus> abilityBonuses;

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
            sourceAbility = abilityBase,
            Type = abilityBase.abilityType
        };
        abilityBonuses = new Dictionary<BonusType, StatBonus>();
        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            mainDamageBase[element] = new AbilityDamageContainer();
            offhandDamageBase[element] = new AbilityDamageContainer();
        }

        AreaLength = abilityBase.areaLength;
        AreaRadius = abilityBase.areaRadius;
        Cooldown = 1f / abilityBase.attacksPerSec;
        HitscanDelay = abilityBase.hitscanDelay;
        ProjectileSize = abilityBase.projectileSize;
        ProjectileSpeed = abilityBase.projectileSpeed;

        MainCriticalChance = abilityBase.baseCritical;
        MainCriticalDamage = 1.50f;
        ProjectileCount = abilityBase.projectileCount;
        TargetRange = abilityBase.targetRange;
        targetLayer = layer;
        attackWithMainHand = true;
        if (abilityBase.abilityType == AbilityType.ATTACK)
            AlternatesAttacks = !abilityBase.useBothWeaponsForDual;
        else
            AlternatesAttacks = false;

        auraBuffBonus = new AuraBuffBonusContainer
        {
            cachedAuraBonuses = new List<Tuple<BonusType, ModifyType, float>>()
        };

        if (abilityBase.targetType == AbilityTargetType.ENEMY)
        {
            BuffType = EffectType.DEBUFF;
        }
        else
            BuffType = EffectType.BUFF;

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
        if (actor == AbilityOwner)
            return;
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
            LinkedAbility.UpdateAbilityLevel(level);
    }

    public virtual void UpdateAbilityStats(HeroData data)
    {
        IEnumerable<GroupType> tags = data.GroupTypes.Union(abilityBase.GetGroupTypes());
        if (abilityBase.weaponRestrictions.Count > 0)
        {
            if (!tags.Intersect(abilityBase.weaponRestrictions).Any())
            {
                IsUsable = false;
                return;
            }
        }
        IsUsable = true;

        UpdateAbilityBonusProperties(tags);
        UpdateDamage(data, abilityBase.damageLevels, tags);
        UpdateTypeParameters(data, tags);
        UpdateShotParameters(data, tags);
        UpdateOnHitDataBonuses(data, tags);
        UpdateAbilityBuffData(data, tags);

        if (abilityCollider != null)
            abilityCollider.abilityCollider.radius = TargetRange;
        if (LinkedAbility != null)
            LinkedAbility.UpdateAbilityStats(data, tags);
    }

    public virtual void UpdateAbilityStats(EnemyData data)
    {
        IsUsable = true;
        IEnumerable<GroupType> tags = data.GroupTypes.Union(abilityBase.GetGroupTypes());

        UpdateAbilityBonusProperties(tags);
        UpdateDamage(data, abilityBase.damageLevels, tags);
        UpdateTypeParameters(data, tags);
        UpdateShotParameters(data, tags);
        UpdateOnHitDataBonuses(data, tags);
        UpdateAbilityBuffData(data, tags);

        if (abilityCollider != null)
            abilityCollider.abilityCollider.radius = TargetRange;
        if (LinkedAbility != null)
            LinkedAbility.UpdateAbilityStats(data, tags);
    }

    private void UpdateAbilityBuffData(ActorData data, IEnumerable<GroupType> tags)
    {
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
        {
            auraBuffBonus.cachedAuraBonuses = new List<Tuple<BonusType, ModifyType, float>>();
            auraBuffBonus.auraStrength = 0;
            foreach (AbilityScalingAddedEffect effect in abilityBase.appliedEffects)
            {
                float buffValue = effect.initialValue + effect.growthValue * abilityLevel;
                auraBuffBonus.cachedAuraBonuses.Add(new Tuple<BonusType, ModifyType, float>(effect.bonusType, effect.modifyType, buffValue));
                auraBuffBonus.auraStrength += buffValue;

                if (!GameManager.Instance.isInBattle)
                {
                    data.AddTemporaryBonus(buffValue, effect.bonusType, effect.modifyType, true);
                }
            }
        }
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
                AreaRadius = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AREA_RADIUS, BonusType.MELEE_ATTACK_RANGE).CalculateStat(weapon.WeaponRange);
            else
                AreaRadius = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AREA_RADIUS, BonusType.MELEE_ATTACK_RANGE).CalculateStat(1f);
        }
        else
            AreaRadius = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AREA_RADIUS).CalculateStat(abilityBase.areaRadius);

        AreaLength = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AREA_RADIUS).CalculateStat(abilityBase.areaLength);
        AreaScaling = AreaRadius / abilityBase.areaRadius;

        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
        {
            TargetRange = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AURA_RANGE).CalculateStat(abilityBase.targetRange);
            return;
        }

        ProjectileSpeed = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_SPEED).CalculateStat(abilityBase.projectileSpeed);

        if (abilityBase.GetGroupTypes().Contains(GroupType.CANNOT_MODIFY_PROJECTILE_COUNT))
            ProjectileCount = abilityBase.projectileCount;
        else if (abilityBase.abilityShotType == AbilityShotType.HITSCAN_MULTI)
            ProjectileCount = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.HITSCAN_MULTI_TARGET_COUNT).CalculateStat(abilityBase.projectileCount);
        else
            ProjectileCount = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_COUNT).CalculateStat(abilityBase.projectileCount);

        if (abilityBase.abilityShotType == AbilityShotType.HITSCAN_SINGLE || abilityBase.abilityShotType == AbilityShotType.HITSCAN_MULTI)
        {
            if (abilityBase.GetGroupTypes().Contains(GroupType.CAN_PIERCE))
                ProjectilePierce = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_PIERCE).CalculateStat(0);
            else
                ProjectilePierce = 0;

            if (abilityBase.GetGroupTypes().Contains(GroupType.CAN_CHAIN))
                ProjectileChain = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_CHAIN).CalculateStat(0);
            else
                ProjectileChain = 0;
        }
        else
        {
            ProjectilePierce = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_PIERCE).CalculateStat(0);
            ProjectileChain = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_CHAIN).CalculateStat(0);
        }
    }

    protected void UpdateShotParameters(EnemyData data, IEnumerable<GroupType> tags)
    {
        AreaRadius = Math.Max(data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AREA_RADIUS).CalculateStat(abilityBase.areaRadius), 0.1f);
        AreaLength = Math.Max(data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AREA_RADIUS).CalculateStat(abilityBase.areaLength), 1f);
        AreaScaling = AreaRadius / abilityBase.areaRadius;
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
        {
            TargetRange = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AURA_RANGE).CalculateStat(abilityBase.targetRange);
            return;
        }

        ProjectileSpeed = Math.Max(data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_SPEED).CalculateStat(abilityBase.projectileSpeed), 0.5f);
        ProjectilePierce = Math.Max(data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_PIERCE).CalculateStat(0), 0);
        ProjectileCount = Math.Max(data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_COUNT).CalculateStat(abilityBase.projectileCount), 1);
    }

    protected void UpdateTypeParameters(HeroData data, IEnumerable<GroupType> tags)
    {
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
            return;

        if (abilityBase.abilityType == AbilityType.SPELL)
        {
            StatBonus critBonus = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.SPELL_CRITICAL_CHANCE, BonusType.GLOBAL_CRITICAL_CHANCE);
            StatBonus critDamageBonus = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.SPELL_CRITICAL_DAMAGE, BonusType.GLOBAL_CRITICAL_DAMAGE);
            StatBonus speedBonus = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.CAST_SPEED, BonusType.GLOBAL_ABILITY_SPEED);
            StatBonus rangeBonus = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.SPELL_RANGE);

            MainCriticalChance = critBonus.CalculateStat(abilityBase.baseCritical);
            MainCriticalDamage = 1f + (critDamageBonus.CalculateStat(50) / 100f);
            Cooldown = 1f / speedBonus.CalculateStat(abilityBase.attacksPerSec);
            TargetRange = rangeBonus.CalculateStat(abilityBase.targetRange);
        }
        else if (abilityBase.abilityType == AbilityType.ATTACK)
        {
            StatBonus rangeBonus = new StatBonus();
            StatBonus speedBonus = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.GLOBAL_ATTACK_SPEED, BonusType.GLOBAL_ABILITY_SPEED);

            if (abilityBase.GetGroupTypes().Contains(GroupType.MELEE_ATTACK))
                data.GetMultiStatBonus(rangeBonus, abilityBonuses, tags, BonusType.MELEE_ATTACK_RANGE);
            if (abilityBase.GetGroupTypes().Contains(GroupType.RANGED_ATTACK))
                data.GetMultiStatBonus(rangeBonus, abilityBonuses, tags, BonusType.RANGED_ATTACK_RANGE);

            if (data.GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon weapon)
            {
                HashSet<GroupType> nonWeaponTags = data.GetGroupTypes(false);
                HashSet<GroupType> mainTags = new HashSet<GroupType>(nonWeaponTags);
                mainTags.UnionWith(weapon.GetGroupTypes());

                float weaponSpeed = weapon.AttackSpeed;
                float mainCritical = weapon.CriticalChance;
                float range = weapon.WeaponRange;

                if (data.GetEquipmentInSlot(EquipSlotType.OFF_HAND) is Weapon offhand)
                {
                    if (AlternatesAttacks)
                    {
                        HashSet<GroupType> offTags = new HashSet<GroupType>(nonWeaponTags);
                        offTags.UnionWith(offhand.GetGroupTypes());

                        StatBonus offCritBonus = data.GetMultiStatBonus(abilityBonuses, offTags, BonusType.GLOBAL_CRITICAL_CHANCE, BonusType.ATTACK_CRITICAL_CHANCE);
                        OffhandCriticalChance = offCritBonus.CalculateStat(offhand.CriticalChance);

                        StatBonus offCritDamageBonus = data.GetMultiStatBonus(abilityBonuses, offTags, BonusType.GLOBAL_CRITICAL_DAMAGE, BonusType.ATTACK_CRITICAL_DAMAGE);
                        OffhandCriticalDamage = 1f + (offCritDamageBonus.CalculateStat(50) / 100f);
                    }
                    else
                    {
                        mainTags.UnionWith(offhand.GetGroupTypes());
                        mainCritical = (mainCritical + offhand.CriticalChance) / 2f;
                    }
                    range = (range + offhand.WeaponRange) / 2f;
                    weaponSpeed = (weaponSpeed + offhand.AttackSpeed) / 2f;
                }

                Cooldown = 1f / speedBonus.CalculateStat(weaponSpeed);

                StatBonus mainCritBonus = data.GetMultiStatBonus(abilityBonuses, mainTags, BonusType.GLOBAL_CRITICAL_CHANCE, BonusType.ATTACK_CRITICAL_CHANCE);
                MainCriticalChance = mainCritBonus.CalculateStat(mainCritical);
                StatBonus mainCritDamageBonus = data.GetMultiStatBonus(abilityBonuses, mainTags, BonusType.GLOBAL_CRITICAL_DAMAGE, BonusType.ATTACK_CRITICAL_DAMAGE);
                MainCriticalDamage = 1f + (mainCritDamageBonus.CalculateStat(50) / 100f);

                if (abilityBase.useWeaponRangeForTargeting)
                {
                    TargetRange = rangeBonus.CalculateStat(range);
                }
                else
                    TargetRange = rangeBonus.CalculateStat(abilityBase.targetRange);
            }
            else
            {
                StatBonus critBonus = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.GLOBAL_CRITICAL_CHANCE, BonusType.ATTACK_CRITICAL_CHANCE);

                //Unarmed default values
                if (abilityBase.useWeaponRangeForTargeting)
                {
                    TargetRange = rangeBonus.CalculateStat(1f);
                }
                else
                    TargetRange = rangeBonus.CalculateStat(abilityBase.targetRange);
                Cooldown = 1f / speedBonus.CalculateStat(1f);
                MainCriticalChance = critBonus.CalculateStat(3.5f);
            }
        }

        if (float.IsInfinity(Cooldown) || float.IsNaN(Cooldown))
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

            MainCriticalChance = critBonus.CalculateStat(abilityBase.baseCritical);
            Cooldown = 1f / speedBonus.CalculateStat(abilityBase.attacksPerSec);
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

            MainCriticalChance = critBonus.CalculateStat(data.BaseData.attackCriticalChance);
            Cooldown = 1 / speedBonus.CalculateStat(data.BaseData.attackSpeed);
            TargetRange = rangeBonus.CalculateStat(data.BaseData.attackTargetRange);
        }
        MainCriticalDamage = 1f + (critDamageBonus.CalculateStat(50) / 100f);

        if (float.IsInfinity(Cooldown))
            Cooldown = 0.001f;
    }

    protected void UpdateDamage(HeroData data, Dictionary<ElementType, AbilityDamageBase> damageLevels, IEnumerable<GroupType> tags)
    {
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
            return;

        IList<GroupType> damageTags = abilityBase.GetGroupTypes();
        Weapon mainWeapon = null, offWeapon = null;
        float flatDamageMod = abilityBase.flatDamageMultiplier;
        DualWielding = false;
        int[] mainConvertedDamageMin = new int[7];
        int[] mainConvertedDamageMax = new int[7];
        int[] offConvertedDamageMin = new int[7];
        int[] offConvertedDamageMax = new int[7];

        if (data.GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon)
        {
            mainWeapon = data.GetEquipmentInSlot(EquipSlotType.WEAPON) as Weapon;
        }
        if (data.GetEquipmentInSlot(EquipSlotType.OFF_HAND) is Weapon)
        {
            offWeapon = data.GetEquipmentInSlot(EquipSlotType.OFF_HAND) as Weapon;
            DualWielding = true;
        }

        HashSet<GroupType> nonWeaponTags = data.GetGroupTypes(false);

        if (AbilityOwner != null)
            nonWeaponTags.UnionWith(AbilityOwner.GetActorTags());

        HashSet<GroupType> mainHandTags = new HashSet<GroupType>(nonWeaponTags);

        if (mainWeapon != null)
        {
            mainHandTags.UnionWith(mainWeapon.GetGroupTypes());
            if (DualWielding && !AlternatesAttacks || DualWielding && abilityBase.abilityType == AbilityType.SPELL)
                mainHandTags.UnionWith(offWeapon.GetGroupTypes());
        }

        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            float offMinDamage = 0, offMaxDamage = 0, baseMinDamage = 0, baseMaxDamage = 0;

            if (damageLevels.ContainsKey(element))
            {
                MinMaxRange abilityBaseDamage = damageLevels[element].damage[abilityLevel];
                baseMinDamage = abilityBaseDamage.min;
                baseMaxDamage = abilityBaseDamage.max;
            }

            float mainMaxDamage;
            float mainMinDamage;
            if (abilityBase.abilityType == AbilityType.ATTACK)
            {
                float weaponMulti = abilityBase.weaponMultiplier + abilityBase.weaponMultiplierScaling * abilityLevel;
                flatDamageMod = weaponMulti;

                if (mainWeapon != null)
                {
                    MinMaxRange mainWeaponDamage = mainWeapon.GetWeaponDamage(element);

                    mainMinDamage = (mainWeaponDamage.min + baseMinDamage) * weaponMulti;
                    mainMaxDamage = (mainWeaponDamage.max + baseMaxDamage) * weaponMulti;

                    if (DualWielding)
                    {
                        MinMaxRange offWeaponDamage = offWeapon.GetWeaponDamage(element);

                        offMinDamage = (offWeaponDamage.min + baseMinDamage) * weaponMulti;
                        offMaxDamage = (offWeaponDamage.max + baseMaxDamage) * weaponMulti;

                        offhandDamageBase[element].baseMin = offMinDamage;
                        offhandDamageBase[element].baseMax = offMaxDamage;

                        if (!AlternatesAttacks)
                        {
                            mainMinDamage = (mainMinDamage + offMinDamage) * 0.5f;
                            mainMaxDamage = (mainMaxDamage + offMaxDamage) * 0.5f;
                        }
                    }
                }
                else
                {
                    if (element == ElementType.PHYSICAL)
                    {
                        baseMinDamage += 4;
                        baseMaxDamage += 9;
                    }
                    mainMinDamage = baseMinDamage * weaponMulti;
                    mainMaxDamage = baseMaxDamage * weaponMulti;
                }
            }
            else
            {
                mainMinDamage = baseMinDamage;
                mainMaxDamage = baseMaxDamage;
            }

            mainDamageBase[element].baseMin = mainMinDamage;
            mainDamageBase[element].baseMax = mainMaxDamage;

            //Helpers.GetDamageTypes(element, abilityType, abilityBase.abilityShotType, damageTags, min, max, multi);

            HashSet<BonusType> min = new HashSet<BonusType>();
            HashSet<BonusType> max = new HashSet<BonusType>();
            HashSet<BonusType> multi = new HashSet<BonusType>();

            Helpers.GetGlobalAndFlatDamageTypes(element, abilityBase.abilityType, abilityBase.abilityShotType, damageTags, min, max, multi);

            mainDamageBase[element].ClearBonuses();
            data.GetMultiStatBonus(mainDamageBase[element].minBonus, abilityBonuses, mainHandTags, min.ToArray());
            data.GetMultiStatBonus(mainDamageBase[element].maxBonus, abilityBonuses, mainHandTags, max.ToArray());
            multi.UnionWith(Helpers.GetMultiplierTypes(abilityBase.abilityType, element));
            data.GetMultiStatBonus(mainDamageBase[element].multiplierBonus, abilityBonuses, mainHandTags, multi.ToArray());

            HashSet<BonusType> availableConversions = data.BonusesIntersection(abilityBonuses.Keys, Helpers.GetConversionTypes(element));
            if (availableConversions.Count > 0)
            {
                GetElementConversionValues(data, mainHandTags, availableConversions, mainDamageBase[element]);
                MinMaxRange baseRange = CalculateDamageConversion(data, flatDamageMod, mainConvertedDamageMin, mainConvertedDamageMax, mainHandTags, mainDamageBase[element], element, multi);
                mainDamageBase[element].calculatedRange.min = baseRange.min;
                mainDamageBase[element].calculatedRange.max = baseRange.max;
            }
            else
            {
                mainDamageBase[element].CalculateRange(flatDamageMod, finalDamageModifier);
            }

            if (DualWielding && AlternatesAttacks)
            {
                HashSet<GroupType> offHandTags = new HashSet<GroupType>(nonWeaponTags);
                offHandTags.UnionWith(offWeapon.GetGroupTypes());

                offhandDamageBase[element].ClearBonuses();

                data.GetMultiStatBonus(offhandDamageBase[element].minBonus, abilityBonuses, offHandTags, min.ToArray());
                data.GetMultiStatBonus(offhandDamageBase[element].maxBonus, abilityBonuses, offHandTags, max.ToArray());
                multi.UnionWith(Helpers.GetMultiplierTypes(abilityBase.abilityType, element));
                data.GetMultiStatBonus(offhandDamageBase[element].multiplierBonus, abilityBonuses, offHandTags, multi.ToArray());

                availableConversions = data.BonusesIntersection(abilityBonuses.Keys, Helpers.GetConversionTypes(element));
                if (availableConversions.Count > 0)
                {
                    GetElementConversionValues(data, offHandTags, availableConversions, offhandDamageBase[element]);
                    MinMaxRange baseRange = CalculateDamageConversion(data, flatDamageMod, offConvertedDamageMin, offConvertedDamageMax, offHandTags, offhandDamageBase[element], element, multi);
                    offhandDamageBase[element].calculatedRange.min = Math.Max(baseRange.min, 0);
                    offhandDamageBase[element].calculatedRange.max = Math.Max(baseRange.max, 0);
                }
                else
                {
                    offhandDamageBase[element].CalculateRange(flatDamageMod, finalDamageModifier);
                }
            }
        }

        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            mainDamageBase[element].calculatedRange.min += mainConvertedDamageMin[(int)element];
            mainDamageBase[element].calculatedRange.max += mainConvertedDamageMax[(int)element];
            if (DualWielding && AlternatesAttacks)
            {
                offhandDamageBase[element].calculatedRange.min += offConvertedDamageMin[(int)element];
                offhandDamageBase[element].calculatedRange.max += offConvertedDamageMax[(int)element];
            }
        }
    }

    /// <summary>
    /// Calculates Elemental Conversions. Each element is run once so conversion chains cannot happen.
    /// After conversion, the converted damage is calculated with all multiplier bonuses and
    /// is added to an array. Afterwards it should be added to the main damage dictionary/array.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="flatDamageMod"></param>
    /// <param name="convertedDamageMin"></param>
    /// <param name="convertedDamageMax"></param>
    /// <param name="tags"></param>
    /// <param name="damageContainer"></param>
    /// <param name="element"></param>
    /// <param name="multi"></param>
    /// <param name="minBonus"></param>
    /// <param name="maxBonus"></param>
    /// <param name="multiplierBonus"></param>
    /// <returns></returns>
    private MinMaxRange CalculateDamageConversion(ActorData data, float flatDamageMod, int[] convertedDamageMin,
                                           int[] convertedDamageMax, IEnumerable<GroupType> tags,
                                           AbilityDamageContainer damageContainer, ElementType element,
                                           HashSet<BonusType> multi, StatBonus minBonus = null, StatBonus maxBonus = null, StatBonus multiplierBonus = null)
    {
        float baseMin, baseMax;

        if (minBonus != null)
            baseMin = damageContainer.baseMin + minBonus.CalculateStat(0) * flatDamageMod;
        else
            baseMin = damageContainer.baseMin + damageContainer.minBonus.CalculateStat(0) * flatDamageMod;

        if (maxBonus != null)
            baseMax = damageContainer.baseMax + maxBonus.CalculateStat(0) * flatDamageMod;
        else
            baseMax = damageContainer.baseMax + damageContainer.maxBonus.CalculateStat(0) * flatDamageMod;

        float finalBaseMin = baseMin;
        float finalBaseMax = baseMax;

        for (int i = 0; i < 7; i++)
        {
            if (damageContainer.conversions[i] == 0)
                continue;

            float convertedMin = baseMin * damageContainer.conversions[i];
            float convertedMax = baseMax * damageContainer.conversions[i];

            finalBaseMin -= convertedMin;
            finalBaseMax -= convertedMax;

            HashSet<BonusType> bonusesForConverted = new HashSet<BonusType>(multi);
            bonusesForConverted.UnionWith(Helpers.GetMultiplierTypes(abilityBase.abilityType, element, (ElementType)i));
            StatBonus totalMultiplierBonus = data.GetMultiStatBonus(abilityBonuses, tags, bonusesForConverted.ToArray());
            totalMultiplierBonus.AddBonuses(multiplierBonus);

            convertedMin = totalMultiplierBonus.CalculateStat(convertedMin);
            convertedMax = totalMultiplierBonus.CalculateStat(convertedMax);

            convertedDamageMin[i] += (int)Math.Max(convertedMin * finalDamageModifier, 0);
            convertedDamageMax[i] += (int)Math.Max(convertedMax * finalDamageModifier, 0);
        }
        MinMaxRange returnValue = new MinMaxRange();
        if (finalBaseMin < 1)
            finalBaseMin = 0;
        if (finalBaseMax < 1)
            finalBaseMax = 0;
        if (finalBaseMax == 0 && finalBaseMin == 0)
        {
            returnValue.min = 0;
            returnValue.max = 0;
        }
        else
        {
            StatBonus finalMultiplier = data.GetMultiStatBonus(abilityBonuses, tags, multi.ToArray());
            finalMultiplier.AddBonuses(multiplierBonus);
            returnValue.min = (int)Math.Max(finalMultiplier.CalculateStat(finalBaseMin) * finalDamageModifier, 0);
            returnValue.max = (int)Math.Max(finalMultiplier.CalculateStat(finalBaseMax) * finalDamageModifier, 0);
        }

        return returnValue;
    }

    private void GetElementConversionValues(ActorData data, IEnumerable<GroupType> weaponTags, HashSet<BonusType> availableConversions, AbilityDamageContainer damageContainer)
    {
        int conversionSum = 0;
        HashSet<ElementType> elementList = new HashSet<ElementType>();
        foreach (BonusType conversion in availableConversions)
        {
            ElementType convertTo = (ElementType)Enum.Parse(typeof(ElementType), conversion.ToString().Split('_')[2]);
            elementList.Add(convertTo);
            int conversionValue = data.GetMultiStatBonus(abilityBonuses, weaponTags, conversion).CalculateStat(0);
            damageContainer.conversions[(int)convertTo] = conversionValue;
            conversionSum += conversionValue;
        }
        foreach (ElementType convert in elementList)
        {
            if (conversionSum > 100)
            {
                damageContainer.conversions[(int)convert] /= conversionSum;
            }
            else
            {
                damageContainer.conversions[(int)convert] /= 100f;
            }
        }
    }

    protected void UpdateDamage(EnemyData data, Dictionary<ElementType, AbilityDamageBase> damageLevels, IEnumerable<GroupType> tags, float damageModifier = 1.0f)
    {
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
            return;

        StatBonus minBonus, maxBonus, multiBonus;
        float flatDamageMod = abilityBase.flatDamageMultiplier;
        AbilityType abilityType = abilityBase.abilityType;

        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            float minDamage = 0, maxDamage = 0;
            if (damageLevels.ContainsKey(element))
            {
                MinMaxRange abilityBaseDamage = damageLevels[element].damage[abilityLevel];
                minDamage = abilityBaseDamage.min;
                maxDamage = abilityBaseDamage.max;
            }
            else if (abilityBase.abilityType == AbilityType.ATTACK)
            {
                flatDamageMod = abilityBase.weaponMultiplier + abilityBase.weaponMultiplierScaling * 50;
                minDamage = data.minAttackDamage;
                maxDamage = data.maxAttackDamage;
            }

            HashSet<BonusType> min = new HashSet<BonusType>();
            HashSet<BonusType> max = new HashSet<BonusType>();
            HashSet<BonusType> multi = new HashSet<BonusType>();

            Helpers.GetGlobalAndFlatDamageTypes(element, abilityType, abilityBase.abilityShotType, abilityBase.GetGroupTypes(), min, max, multi);

            minBonus = data.GetMultiStatBonus(abilityBonuses, tags, min.ToArray());
            maxBonus = data.GetMultiStatBonus(abilityBonuses, tags, max.ToArray());
            multiBonus = data.GetMultiStatBonus(abilityBonuses, tags, multi.ToArray());

            mainDamageBase[element].CalculateRange(abilityBase.flatDamageMultiplier, finalDamageModifier);
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
        abilityOnHitData.SetEffectDuration(EffectType.BLEED, bleedDuration.CalculateStat(BleedEffect.BASE_DURATION) / bleedSpeed);

        StatBonus burnChance = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.BURN_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.ELEMENTAL_STATUS_EFFECT_CHANCE);
        StatBonus burnEffectiveness = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.BURN_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME, BonusType.ELEMENTAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus burnDuration = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.BURN_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.ELEMENTAL_STATUS_EFFECT_DURATION);
        float burnSpeed = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.BURN_SPEED).CalculateStat(100f) / 100f;
        abilityOnHitData.SetEffectChance(EffectType.BURN, burnChance.CalculateStat(0f));
        abilityOnHitData.SetEffectEffectiveness(EffectType.BURN, burnEffectiveness.CalculateStat(100f) / 100f * burnSpeed);
        abilityOnHitData.SetEffectDuration(EffectType.BURN, burnDuration.CalculateStat(BurnEffect.BASE_DURATION) / burnSpeed);

        StatBonus chillChance = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.CHILL_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.ELEMENTAL_STATUS_EFFECT_CHANCE);
        StatBonus chillEffectiveness = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.CHILL_EFFECTIVENESS, BonusType.NONDAMAGE_STATUS_EFFECTIVENESS, BonusType.ELEMENTAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus chillDuration = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.CHILL_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.ELEMENTAL_STATUS_EFFECT_DURATION);
        abilityOnHitData.SetEffectChance(EffectType.CHILL, chillChance.CalculateStat(0f));
        abilityOnHitData.SetEffectEffectiveness(EffectType.CHILL, chillEffectiveness.CalculateStat(100f) / 100f);
        abilityOnHitData.SetEffectDuration(EffectType.CHILL, chillDuration.CalculateStat(ChillEffect.BASE_DURATION));

        StatBonus electrocuteChance = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.ELECTROCUTE_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.ELEMENTAL_STATUS_EFFECT_CHANCE);
        StatBonus electrocuteEffectiveness = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.ELECTROCUTE_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.ELEMENTAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus electrocuteDuration = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.ELECTROCUTE_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.ELEMENTAL_STATUS_EFFECT_DURATION);
        float electrocuteSpeed = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.ELECTROCUTE_SPEED).CalculateStat(100f) / 100f;
        abilityOnHitData.SetEffectChance(EffectType.ELECTROCUTE, electrocuteChance.CalculateStat(0f));
        abilityOnHitData.SetEffectEffectiveness(EffectType.ELECTROCUTE, electrocuteEffectiveness.CalculateStat(100f) / 100f * electrocuteSpeed);
        abilityOnHitData.SetEffectDuration(EffectType.ELECTROCUTE, electrocuteDuration.CalculateStat(ElectrocuteEffect.BASE_DURATION) / electrocuteSpeed);

        StatBonus fractureChance = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.FRACTURE_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.ELEMENTAL_STATUS_EFFECT_CHANCE);
        StatBonus fractureEffectiveness = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.FRACTURE_EFFECTIVENESS, BonusType.NONDAMAGE_STATUS_EFFECTIVENESS, BonusType.ELEMENTAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus fractureDuration = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.FRACTURE_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.ELEMENTAL_STATUS_EFFECT_DURATION);
        abilityOnHitData.SetEffectChance(EffectType.FRACTURE, fractureChance.CalculateStat(0f));
        abilityOnHitData.SetEffectEffectiveness(EffectType.FRACTURE, fractureEffectiveness.CalculateStat(100f) / 100f);
        abilityOnHitData.SetEffectDuration(EffectType.FRACTURE, fractureDuration.CalculateStat(FractureEffect.BASE_DURATION));

        StatBonus pacifyChance = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PACIFY_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.PRIMORDIAL_STATUS_EFFECT_CHANCE);
        StatBonus pacifyEffectiveness = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PACIFY_EFFECTIVENESS, BonusType.NONDAMAGE_STATUS_EFFECTIVENESS, BonusType.PRIMORDIAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus pacifyDuration = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PACIFY_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.PRIMORDIAL_STATUS_EFFECT_DURATION);
        abilityOnHitData.SetEffectChance(EffectType.PACIFY, pacifyChance.CalculateStat(0f));
        abilityOnHitData.SetEffectEffectiveness(EffectType.PACIFY, pacifyEffectiveness.CalculateStat(100f) / 100f);
        abilityOnHitData.SetEffectDuration(EffectType.PACIFY, pacifyDuration.CalculateStat(PacifyEffect.BASE_DURATION));

        StatBonus radiationChance = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.RADIATION_CHANCE, BonusType.STATUS_EFFECT_CHANCE, BonusType.PRIMORDIAL_STATUS_EFFECT_CHANCE);
        StatBonus radiationEffectiveness = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.RADIATION_EFFECTIVENESS, BonusType.STATUS_EFFECT_DAMAGE, BonusType.DAMAGE_OVER_TIME, BonusType.PRIMORDIAL_STATUS_EFFECT_EFFECTIVENESS);
        StatBonus radiationDuration = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.RADIATION_DURATION, BonusType.STATUS_EFFECT_DURATION, BonusType.PRIMORDIAL_STATUS_EFFECT_DURATION);
        float radiationSpeed = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.RADIATION_SPEED).CalculateStat(100f) / 100f;
        abilityOnHitData.SetEffectChance(EffectType.RADIATION, radiationChance.CalculateStat(0f));
        abilityOnHitData.SetEffectEffectiveness(EffectType.RADIATION, radiationEffectiveness.CalculateStat(100f) / 100f * radiationSpeed);
        abilityOnHitData.SetEffectDuration(EffectType.RADIATION, radiationDuration.CalculateStat(RadiationEffect.BASE_DURATION) / radiationSpeed);

        StatBonus vsBossDamage = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.DAMAGE_VS_BOSS);
        abilityOnHitData.vsBossDamage = vsBossDamage.CalculateStat(1f);

        StatBonus directHitDamage = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.DIRECT_HIT_DAMAGE);
        abilityOnHitData.directHitDamage = directHitDamage.CalculateStat(1f);

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

        abilityOnHitData.onHitEffectsFromAbility.Clear();
        foreach (TriggeredEffectBonusProperty appliedEffect in abilityBase.triggeredEffects)
        {
            abilityOnHitData.onHitEffectsFromAbility.Add(new TriggeredEffect(appliedEffect, abilityLevel));
        }
    }

    public Dictionary<ElementType, float> CalculateDamageValues(Actor target, Actor source)
    {
        var values = Enum.GetValues(typeof(ElementType));
        float critChance, criticalDamage;
        Dictionary<ElementType, float> returnDict = new Dictionary<ElementType, float>();
        IList<GroupType> damageTags = abilityBase.GetGroupTypes();
        HashSet<GroupType> weaponTags = new HashSet<GroupType>();
        HashSet<GroupType> targetTypes = target.GetActorTagsAsTarget();
        Dictionary<ElementType, AbilityDamageContainer> dicToUse;
        Dictionary<BonusType, StatBonus> whenHitBonusDict = new Dictionary<BonusType, StatBonus>();
        int[] minDamage = new int[7];
        int[] maxDamage = new int[7];
        int[] convertedMinDamage = new int[7];
        int[] convertedMaxDamage = new int[7];

        foreach (TriggeredEffect triggeredEffect in source.Data.WhenHittingEffects)
        {
            if (targetTypes.Contains(triggeredEffect.BaseEffect.restriction) && triggeredEffect.RollTriggerChance())
            {
                if (whenHitBonusDict.ContainsKey(triggeredEffect.BaseEffect.statBonusType))
                {
                    whenHitBonusDict[triggeredEffect.BaseEffect.statBonusType].AddBonus(triggeredEffect.BaseEffect.statModifyType, triggeredEffect.Value);
                }
                else
                {
                    StatBonus bonus = new StatBonus();
                    bonus.AddBonus(triggeredEffect.BaseEffect.statModifyType, triggeredEffect.Value);
                    whenHitBonusDict.Add(triggeredEffect.BaseEffect.statBonusType, bonus);
                }
            }
        }

        if (attackWithMainHand)
        {
            dicToUse = mainDamageBase;
            critChance = MainCriticalChance;
            criticalDamage = MainCriticalDamage;
        }
        else
        {
            dicToUse = offhandDamageBase;
            critChance = OffhandCriticalChance;
            criticalDamage = OffhandCriticalDamage;
        }

        if (source.GetActorType() == ActorType.ALLY)
        {
            HeroData hero = (HeroData)source.Data;
            HashSet<GroupType> nonWeaponTags = hero.GetGroupTypes(false);
            nonWeaponTags.UnionWith(AbilityOwner.GetActorTags());
            weaponTags = new HashSet<GroupType>(nonWeaponTags);
            if (attackWithMainHand && hero.GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon weapon)
            {
                weaponTags.UnionWith(weapon.GetGroupTypes());
                if (DualWielding && !AlternatesAttacks || DualWielding && abilityBase.abilityType == AbilityType.SPELL)
                    weaponTags.UnionWith(hero.GetEquipmentInSlot(EquipSlotType.OFF_HAND).GetGroupTypes());
            }
            else if (!attackWithMainHand && hero.GetEquipmentInSlot(EquipSlotType.OFF_HAND) is Weapon offWeapon)
            {
                weaponTags.UnionWith(offWeapon.GetGroupTypes());
            }
        }

        foreach (ElementType elementType in values)
        {
            if (dicToUse.ContainsKey(elementType))
            {
                minDamage[(int)elementType] = dicToUse[elementType].calculatedRange.min;
                maxDamage[(int)elementType] = dicToUse[elementType].calculatedRange.max;
                if (source.Data.WhenHittingEffects.Count > 0)
                {
                    StatBonus minBonus = new StatBonus();
                    StatBonus maxBonus = new StatBonus();
                    StatBonus multiBonus = new StatBonus();
                    HashSet<BonusType> minTypes = new HashSet<BonusType>();
                    HashSet<BonusType> maxTypes = new HashSet<BonusType>();
                    HashSet<BonusType> multiTypes = new HashSet<BonusType>();

                    Helpers.GetGlobalAndFlatDamageTypes(elementType, abilityBase.abilityType, abilityBase.abilityShotType, damageTags, minTypes, maxTypes, multiTypes);
                    multiTypes.UnionWith(Helpers.GetMultiplierTypes(abilityBase.abilityType, elementType));
                    foreach (KeyValuePair<BonusType, StatBonus> keyValue in whenHitBonusDict)
                    {
                        if (minTypes.Contains(keyValue.Key))
                            minBonus.AddBonuses(keyValue.Value);
                        else if (maxTypes.Contains(keyValue.Key))
                            maxBonus.AddBonuses(keyValue.Value);
                        else if (multiTypes.Contains(keyValue.Key))
                            multiBonus.AddBonuses(keyValue.Value);
                    }

                    float flatDamageMod;

                    if (abilityBase.abilityType == AbilityType.ATTACK)
                    {
                        flatDamageMod = abilityBase.weaponMultiplier + abilityBase.weaponMultiplierScaling * abilityLevel;
                    }
                    else
                    {
                        flatDamageMod = abilityBase.flatDamageMultiplier;
                    }

                    minBonus.AddBonuses(dicToUse[elementType].minBonus);
                    maxBonus.AddBonuses(dicToUse[elementType].maxBonus);

                    HashSet<BonusType> availableConversions = source.Data.BonusesIntersection(abilityBonuses.Keys, Helpers.GetConversionTypes(elementType));
                    if (availableConversions.Count > 0)
                    {
                        GetElementConversionValues(source.Data, weaponTags, availableConversions, offhandDamageBase[elementType]);
                        MinMaxRange baseRange = CalculateDamageConversion(source.Data, flatDamageMod, convertedMinDamage, convertedMaxDamage, weaponTags, dicToUse[elementType], elementType, multiTypes, minBonus, maxBonus, multiBonus);
                        minDamage[(int)elementType] = baseRange.min;
                        maxDamage[(int)elementType] = baseRange.max;
                    }
                    else
                    {
                        multiBonus.AddBonuses(dicToUse[elementType].multiplierBonus);
                        minDamage[(int)elementType] = (int)Math.Max(multiBonus.CalculateStat(dicToUse[elementType].baseMin + minBonus.CalculateStat(0f) * flatDamageMod) * finalDamageModifier, 0);
                        maxDamage[(int)elementType] = (int)Math.Max(multiBonus.CalculateStat(dicToUse[elementType].baseMax + maxBonus.CalculateStat(0f) * flatDamageMod) * finalDamageModifier, 0);
                    }
                }
            }
        }

        bool isCrit = UnityEngine.Random.Range(0f, 100f) < critChance;

        foreach (ElementType elementType in values)
        {
            float damage = UnityEngine.Random.Range(minDamage[(int)elementType] + convertedMinDamage[(int)elementType], maxDamage[(int)elementType] + convertedMaxDamage[(int)elementType] + 1);

            if (isCrit)
                damage *= criticalDamage;

            returnDict.Add(elementType, damage * abilityBase.hitDamageModifier);
        }

        if (DualWielding && AlternatesAttacks)
            attackWithMainHand = !attackWithMainHand;

        return returnDict;
    }

    public void StartFiring(Actor parent)
    {
        if (IsUsable)
            if (firingRoutine == null)
            {
                if (abilityBase.abilityType == AbilityType.AURA)
                    firingRoutine = FiringRoutine_Aura();
                else if (abilityBase.abilityType == AbilityType.SELF_BUFF)
                {
                    FiringRoutine_Aura();
                    return;
                }
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
        attackWithMainHand = true;
        while (true)
        {
            if (AbilityOwner.attackLocks > 0)
                yield return null;

            if (CurrentTarget != null)
            {
                switch (abilityBase.abilityShotType)
                {
                    case AbilityShotType.PROJECTILE:
                    case AbilityShotType.PROJECTILE_NOVA:
                        FireProjectile();
                        break;

                    case AbilityShotType.ARC_AOE:
                    case AbilityShotType.NOVA_ARC_AOE:
                        FireArcAoe();
                        break;

                    case AbilityShotType.NOVA_AOE:
                    case AbilityShotType.RADIAL_AOE:
                        FireRadialAoe();
                        break;

                    case AbilityShotType.HITSCAN_SINGLE:
                        FireHitscan();
                        break;

                    case AbilityShotType.HITSCAN_MULTI:
                        FireHitscanMulti();
                        break;

                    case AbilityShotType.LINEAR_AOE:
                        FireLinearAoe();
                        break;

                    case AbilityShotType.FORWARD_MOVING_ARC:
                    case AbilityShotType.FORWARD_MOVING_RADIAL:
                    case AbilityShotType.FORWARD_MOVING_LINEAR:
                        FireMovingAoe();
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
                    ApplyAuraBuff(AbilityOwner);
                    foreach (Actor target in targetList)
                        ApplyAuraBuff(target);
                    break;

                case AbilityType.SELF_BUFF:
                    ApplyAuraBuff(AbilityOwner);
                    break;

                default:
                    break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    protected void ApplyAuraBuff(Actor target)
    {
        StatBonusBuffEffect buff = null;
        List<StatBonusBuffEffect> buffs = target.GetBuffStatusEffect(abilityBase.idName);
        if (buffs.Count > 0)
            buff = buffs[0];

        if (buff != null)
        {
            if (auraBuffBonus.auraStrength == buff.BuffPower)
            {
                buff.RefreshDuration(0.75f);
                return;
            }
            else if (auraBuffBonus.auraStrength < buff.BuffPower)
                return;
            else
                buff.OnExpire();
        }

        target.AddStatusEffect(new StatBonusBuffEffect(target, AbilityOwner, auraBuffBonus.cachedAuraBonuses, 0.75f, abilityBase.idName, BuffType));
    }

    protected void FireRadialAoe(Vector3 origin, Vector3 target)
    {
        Collider2D[] hits;
        ContactFilter2D filter = new ContactFilter2D
        {
            useTriggers = true
        };
        filter.SetLayerMask(LayerMask.GetMask("Enemy"));

        //Vector2 forward = target - origin;

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

        foreach (Collider2D hit in hits)
        {
            Actor actor = hit.gameObject.GetComponent<Actor>();
            if (actor != null)
            {
                Dictionary<ElementType, float> damageDict = CalculateDamageValues(actor, AbilityOwner);
                ApplyDamageToActor(actor, damageDict, abilityOnHitData, true);
            }
        }
    }

    protected IEnumerator FireHitscan(Vector3 origin, Actor target, List<Actor> sharedHitlist)
    {
        Actor lastHitTarget;
        Dictionary<ElementType, float> damageDict = CalculateDamageValues(target, AbilityOwner);
        emitParams.position = target.transform.position;

        yield return new WaitForSeconds(HitscanDelay);

        ParticleManager.Instance.EmitAbilityParticle(abilityBase.idName, emitParams, 1);
        ApplyDamageToActor(target, damageDict, abilityOnHitData, true);

        List<Actor> hitList;
        if (sharedHitlist == null)
        {
            hitList = new List<Actor>
            {
                target
            };
        }
        else
        {
            hitList = sharedHitlist;
        }

        lastHitTarget = target;
        int pierceCount = ProjectilePierce;

        if (pierceCount > 0)
        {
            RaycastHit2D[] raycastHits = Physics2D.RaycastAll(origin, (target.transform.position - origin).normalized, Mathf.Infinity, targetMask);
            foreach (RaycastHit2D hit in raycastHits)
            {
                Actor actor = hit.transform.GetComponent<Actor>();
                if (hitList.Contains(actor))
                    continue;

                damageDict = CalculateDamageValues(actor, AbilityOwner);
                ApplyDamageToActor(actor, damageDict, abilityOnHitData, true);

                emitParams.position = actor.transform.position;
                ParticleManager.Instance.EmitAbilityParticle(abilityBase.idName, emitParams, 1);

                lastHitTarget = actor;
                hitList.Add(actor);

                pierceCount--;
                if (pierceCount <= 0)
                    break;
            }
        }

        if (ProjectileChain > 0 && pierceCount <= 0)
        {
            List<Actor> possibleTargets = new List<Actor>();
            Collider2D[] hits = Physics2D.OverlapCircleAll(lastHitTarget.transform.position, 2, targetMask);
            foreach (Collider2D hit in hits)
            {
                Actor actor = hit.GetComponent<Actor>();
                if (actor == null)
                    continue;
                if (hitList.Contains(actor))
                    continue;
                possibleTargets.Add(actor);
            }

            if (possibleTargets.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, possibleTargets.Count);
                if (possibleTargets[index] != null)
                {
                    AbilityOwner.StartCoroutine(FireHitscan_Chained(origin, possibleTargets[index], ProjectileChain - 1, hitList));
                }
            }
        }

        yield break;
    }

    protected IEnumerator FireHitscan_Chained(Vector3 origin, Actor target, int remainingChainCount, List<Actor> hitList)
    {
        Dictionary<ElementType, float> damageDict = CalculateDamageValues(target, AbilityOwner);
        emitParams.position = target.transform.position;

        yield return new WaitForSeconds(HitscanDelay / 2);

        ParticleManager.Instance.EmitAbilityParticle(abilityBase.idName, emitParams, 1);
        ApplyDamageToActor(target, damageDict, abilityOnHitData, true);
        hitList.Add(target);

        if (remainingChainCount > 0)
        {
            List<Actor> possibleTargets = new List<Actor>();
            Collider2D[] hits = Physics2D.OverlapCircleAll(target.transform.position, 2, targetMask);

            foreach (Collider2D hit in hits)
            {
                Actor actor = hit.GetComponent<Actor>();
                if (hitList.Contains(actor))
                    continue;
                possibleTargets.Add(actor);
            }

            if (possibleTargets.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, possibleTargets.Count);
                if (possibleTargets[index] != null)
                {
                    AbilityOwner.StartCoroutine(FireHitscan_Chained(origin, possibleTargets[index], remainingChainCount - 1, hitList));
                }
            }
        }

        yield break;
    }

    protected void FireHitscanMulti(Vector3 origin)
    {
        if (targetList.Count <= ProjectileCount)
        {
            List<Actor> hitList = new List<Actor>();
            foreach (Actor target in targetList)
            {
                hitList.Add(target);
                AbilityOwner.StartCoroutine(FireHitscan(abilityCollider.transform.position, target, hitList));
            }
        }
        else
        {
            List<Actor> targetListCopy = new List<Actor>(targetList);
            List<Actor> selectedTargetList = new List<Actor>();

            for (int i = 0; i < ProjectileCount; i++)
            {
                int index = UnityEngine.Random.Range(0, targetListCopy.Count);
                selectedTargetList.Add(targetListCopy[index]);
                targetListCopy.RemoveAt(index);
            }

            foreach (Actor target in selectedTargetList)
            {
                AbilityOwner.StartCoroutine(FireHitscan(abilityCollider.transform.position, target, selectedTargetList));
            }
        }
    }

    protected void FireArcAoe(Vector3 origin, Vector3 target)
    {
        Collider2D[] hits;

        Vector2 forward = target - origin;
        float arc = abilityBase.projectileSpread / 2f;
        hits = Physics2D.OverlapCircleAll(origin, AreaRadius, targetMask);

        emitParams.position = origin;
        float angle = Vector2.SignedAngle(Vector2.up, forward) + (180f - abilityBase.projectileSpread) / 2;
        ParticleManager.Instance.EmitAbilityParticle_Rotated(abilityBase.idName, emitParams, AreaScaling, angle, AbilityOwner.transform);

        foreach (Collider2D hit in hits)
        {
            Actor actor = hit.gameObject.GetComponent<Actor>();
            if (actor != null)
            {
                Vector2 toActor = actor.transform.position - origin;
                if (Vector2.Angle(toActor, forward) < arc)
                {
                    Dictionary<ElementType, float> damageDict = CalculateDamageValues(actor, AbilityOwner);
                    ApplyDamageToActor(actor, damageDict, abilityOnHitData, true);
                }
            }
        }
    }

    protected void FireProjectile(Vector3 origin, Vector3 target)
    {
        Vector3 heading = (target - origin).normalized;
        heading.z = 0;

        bool isSpread = abilityBase.doesProjectileSpread;
        float spreadAngle = 17.5f;
        List<Actor> sharedList = null;
        if (abilityBase.abilityShotType == AbilityShotType.PROJECTILE_NOVA)
        {
            spreadAngle = 360f / ProjectileCount;
            sharedList = new List<Actor>();
        }
        else
        {
            if (isSpread)
            {
                if (spreadAngle * ProjectileCount / 2f > abilityBase.projectileSpread)
                {
                    spreadAngle = abilityBase.projectileSpread / ProjectileCount / 2f;
                }
                sharedList = new List<Actor>();
            }
            else
            {
                spreadAngle = abilityBase.projectileSpread;
            }
        }

        for (int i = 0; i < ProjectileCount; i++)
        {
            Projectile pooledProjectile = StageManager.Instance.BattleManager.ProjectilePool.GetProjectile();
            pooledProjectile.transform.position = origin;
            pooledProjectile.transform.up = (pooledProjectile.transform.position + pooledProjectile.currentHeading) - pooledProjectile.transform.position;

            pooledProjectile.timeToLive = 4f * abilityBase.projectileLifespanMulti;
            pooledProjectile.currentSpeed = abilityBase.projectileSpeed;

            if (abilityBase.abilityShotType == AbilityShotType.PROJECTILE_NOVA)
            {
                pooledProjectile.currentHeading = Quaternion.Euler(0, 0, spreadAngle * i) * heading;
                pooledProjectile.sharedHitList = sharedList;
            }
            else
            {
                if (isSpread)
                {
                    int angleMultiplier = (int)Math.Round(i / 2f, MidpointRounding.AwayFromZero);
                    if (i % 2 == 0)
                    {
                        angleMultiplier *= -1;
                    }

                    pooledProjectile.currentHeading = Quaternion.Euler(0, 0, spreadAngle * angleMultiplier) * heading;
                    pooledProjectile.sharedHitList = sharedList;
                }
                else
                {
                    pooledProjectile.currentHeading = Quaternion.Euler(0, 0, spreadAngle * UnityEngine.Random.Range(-1f, 1f)) * heading;
                }
            }

            if (abilityBase.hasLinkedAbility)
                pooledProjectile.linkedAbility = LinkedAbility;

            pooledProjectile.damageCalculationCallback = CalculateDamageValues;
            pooledProjectile.onHitData = abilityOnHitData;
            pooledProjectile.gameObject.layer = targetLayer;
            pooledProjectile.layerMask = targetMask;
            pooledProjectile.abilityBase = abilityBase;
            pooledProjectile.pierceCount = ProjectilePierce;
            pooledProjectile.chainCount = ProjectileChain;
            pooledProjectile.transform.localScale = new Vector2(ProjectileSize, ProjectileSize);

            AbilityParticleSystem particleSystem = ParticleManager.Instance.GetParticleSystem(abilityBase.idName);
            if (particleSystem == null)
            {
                particleSystem = ParticleManager.Instance.GetParticleSystem(abilityBase.effectSprite);
            }

            if (particleSystem != null)
            {
                GameObject particle = GameObject.Instantiate(particleSystem.gameObject, pooledProjectile.transform, false);
                pooledProjectile.particles = particle.GetComponent<ParticleSystem>();
            }

            pooledProjectile.GetComponent<SpriteRenderer>().sprite = ResourceManager.Instance.GetSprite(abilityBase.idName);
        }
    }

    protected void FireMovingAoe(Vector3 origin, Actor target)
    {
        Vector3 heading = (target.transform.position - origin).normalized;
        heading.z = 0;

        Projectile pooledProjectile;

        if (abilityBase.abilityShotType == AbilityShotType.FORWARD_MOVING_LINEAR)
        {
            pooledProjectile = StageManager.Instance.BattleManager.BoxProjectilePool.GetProjectile();
            BoxCollider2D collider = pooledProjectile.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(AreaRadius * 2f, 1f * AreaScaling);
        }
        else
        {
            pooledProjectile = StageManager.Instance.BattleManager.ProjectilePool.GetProjectile();
        }

        pooledProjectile.currentHeading = heading;
        pooledProjectile.transform.position = origin;
        pooledProjectile.transform.up = (pooledProjectile.transform.position + pooledProjectile.currentHeading) - pooledProjectile.transform.position;

        pooledProjectile.timeToLive = abilityBase.projectileLifespanMulti;
        pooledProjectile.currentSpeed = AreaLength / abilityBase.projectileLifespanMulti;

        if (abilityBase.hasLinkedAbility)
            pooledProjectile.linkedAbility = LinkedAbility;

        pooledProjectile.damageCalculationCallback = CalculateDamageValues;
        pooledProjectile.onHitData = abilityOnHitData;
        pooledProjectile.gameObject.layer = targetLayer;
        pooledProjectile.layerMask = targetMask;
        pooledProjectile.abilityBase = abilityBase;
        pooledProjectile.pierceCount = 999;

        AbilityParticleSystem particleSystem = ParticleManager.Instance.GetParticleSystem(abilityBase.idName, abilityBase.effectSprite);

        if (particleSystem != null)
        {
            GameObject particle = GameObject.Instantiate(particleSystem.gameObject, pooledProjectile.transform, false);
            pooledProjectile.particles = particle.GetComponent<ParticleSystem>();
        }

        pooledProjectile.GetComponent<SpriteRenderer>().sprite = ResourceManager.Instance.GetSprite(abilityBase.idName);
    }

    protected void FireLinearAoe(Vector3 origin, Vector3 target)
    {
        Vector3 heading = (target - origin).normalized;
        Vector3 horizontal = Vector3.Cross(heading, Vector3.forward).normalized * AreaRadius;

        /*
        Debug.DrawLine(origin, origin + horizontal, Color.red, 1);
        Debug.DrawLine(origin, origin - horizontal, Color.red, 1);
        Debug.DrawLine(origin - horizontal, origin - horizontal + (heading * AreaLength), Color.red, 1);
        */

        Collider2D[] hits = Physics2D.OverlapAreaAll(origin + horizontal, origin - horizontal + (heading * AreaLength), targetMask);

        foreach (Collider2D hit in hits)
        {
            Actor actor = hit.GetComponent<Actor>();
            if (actor == null)
                continue;
            Dictionary<ElementType, float> damageDict = CalculateDamageValues(actor, AbilityOwner);
            ApplyDamageToActor(actor, damageDict, abilityOnHitData, true);
        }
    }

    protected void FireHitscan()
    {
        AbilityOwner.StartCoroutine(FireHitscan(abilityCollider.transform.position, CurrentTarget, null));
    }

    protected void FireHitscanMulti()
    {
        FireHitscanMulti(abilityCollider.transform.position);
    }

    protected void FireLinearAoe()
    {
        FireLinearAoe(abilityCollider.transform.position, CurrentTarget.transform.position);
    }

    protected void FireRadialAoe()
    {
        FireRadialAoe(abilityCollider.transform.position, CurrentTarget.transform.position);
    }

    protected void FireArcAoe()
    {
        FireArcAoe(abilityCollider.transform.position, CurrentTarget.transform.position);
    }

    protected void FireMovingAoe()
    {
        FireMovingAoe(abilityCollider.transform.position, CurrentTarget);
    }

    protected void FireProjectile()
    {
        if (CurrentTarget.GetActorType() == ActorType.ENEMY)
        {
            EnemyActor enemy = CurrentTarget as EnemyActor;
            float enemySpeed = enemy.Data.movementSpeed;

            //Tilemap tilemap = StageManager.Instance.PathTilemap;

            Vector3? nextNode;
            Vector2 currentPosition = abilityCollider.transform.position;

            //float threshold = 1 + UnityEngine.Random.Range(0f, 1f);

            float threshold = 0.05f + UnityEngine.Random.Range(0f, 0.1f);

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
                    float projectileTimeToNode = Vector2.Distance((Vector2)nextNode, currentPosition) / ProjectileSpeed;
                    //float distance = ((Vector2)nextNode - (currentPosition + projDirectionToNode)).magnitude;
                    float timeDifference = Math.Abs(enemyMoveTime - projectileTimeToNode);

                    if (timeDifference < threshold)
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

    public float GetApproxDPS(bool getOffhand)
    {
        Dictionary<ElementType, AbilityDamageContainer> damage;
        float criticalChance, criticalDamage, total = 0;

        if (!getOffhand)
        {
            damage = mainDamageBase;
            criticalChance = MainCriticalChance / 100f;
            criticalDamage = MainCriticalDamage;
        }
        else
        {
            damage = offhandDamageBase;
            criticalChance = OffhandCriticalChance / 100f;
            criticalDamage = OffhandCriticalDamage;
        }

        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            if (damage.ContainsKey(element))
            {
                total += damage[element].calculatedRange.min + damage[element].calculatedRange.max;
            }
        }

        total /= 2f;
        float dps = ((total * (1 - criticalChance)) + (total * criticalChance * criticalDamage)) * (1f / Cooldown) * abilityBase.hitCount * abilityBase.hitDamageModifier;
        return dps * abilityOnHitData.directHitDamage;
    }

    public void ApplyDamageToActor(Actor target, Dictionary<ElementType, float> damage, AbilityOnHitDataContainer onHitDataContainer, bool isHit)
    {
        if (target == null || target.Data.IsDead)
        {
            return;
        }
        if (abilityBase.hitCount > 1)
        {
            AbilityOwner.StartCoroutine(ApplyMultiHitDamage(target, damage, onHitDataContainer, isHit));
        }
        else
        {
            target.ApplyDamage(damage, onHitDataContainer, isHit);
        }
    }

    private IEnumerator ApplyMultiHitDamage(Actor target, Dictionary<ElementType, float> damage, AbilityOnHitDataContainer onHitDataContainer, bool isHit)
    {
        float delayBetweenHits = abilityBase.delayBetweenHits;
        for (int i = 0; i < abilityBase.hitCount; i++)
        {
            target.ApplyDamage(damage, onHitDataContainer, isHit);
            yield return new WaitForSeconds(delayBetweenHits);
        }
    }

    private class AuraBuffBonusContainer
    {
        public List<Tuple<BonusType, ModifyType, float>> cachedAuraBonuses;
        public float auraStrength = 0;
        public bool isOutdated = true;
    }

    public class AbilityDamageContainer
    {
        public MinMaxRange calculatedRange;
        public float baseMin;
        public float baseMax;
        public StatBonus minBonus;
        public StatBonus maxBonus;
        public StatBonus multiplierBonus;
        public float[] conversions;

        public AbilityDamageContainer()
        {
            calculatedRange = new MinMaxRange();
            minBonus = new StatBonus();
            maxBonus = new StatBonus();
            multiplierBonus = new StatBonus();
            conversions = new float[7];
        }

        public void ClearBonuses()
        {
            minBonus.ResetBonus();
            maxBonus.ResetBonus();
            multiplierBonus.ResetBonus();
            Array.Clear(conversions, 0, 7);
        }

        public void CalculateRange(float flatDamageMod, float finalDamageModifier)
        {
            float mainMinDamage, mainMaxDamage;
            float addedFlatMin = minBonus.CalculateStat(0) * flatDamageMod;
            float addedFlatMax = maxBonus.CalculateStat(0) * flatDamageMod;

            mainMinDamage = baseMin + addedFlatMin;
            mainMaxDamage = baseMax + addedFlatMax;

            mainMinDamage = multiplierBonus.CalculateStat(mainMinDamage);
            mainMaxDamage = multiplierBonus.CalculateStat(mainMaxDamage);

            calculatedRange.min = (int)Math.Max(mainMinDamage * finalDamageModifier, 0);
            calculatedRange.max = (int)Math.Max(mainMaxDamage * finalDamageModifier, 0);
        }
    }
}