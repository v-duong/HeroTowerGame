using System;
using System.Collections.Generic;
using System.Linq;

public partial class ActorAbility
{
    public void UpdateAbilityLevel(int level)
    {
        abilityLevel = level;
        if (LinkedAbility != null)
            LinkedAbility.UpdateAbilityLevel(level);
    }

    public virtual void UpdateAbilityStats(HeroData data)
    {
        IEnumerable<GroupType> tags = data.GroupTypes.Union(abilityBase.GetGroupTypes());
        if (abilityBase.requiredRestrictions.Count > 0)
        {
            if (tags.Intersect(abilityBase.requiredRestrictions).Count() != abilityBase.requiredRestrictions.Count)
            {
                IsUsable = false;
                return;
            }
        }

        if (abilityBase.singleRequireRestrictions.Count > 0)
        {
            if (!tags.Intersect(abilityBase.singleRequireRestrictions).Any())
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
        if (abilityBase.abilityType == AbilityType.AURA)
        {
            auraBuffBonus.auraEffectMultiplier = Math.Max(data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AURA_EFFECT).CalculateStat(1f), 0f);
            auraBuffBonus.selfAuraEffectMultiplier = Math.Max(data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AURA_EFFECT_ON_SELF).CalculateStat(1f), 0f);
        }
        else
        {
            auraBuffBonus.auraEffectMultiplier = 1f;
            auraBuffBonus.selfAuraEffectMultiplier = 1f;
        }

        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF || abilityBase.isSoulAbility)
        {
            auraBuffBonus.cachedAuraBonuses.Clear();
            auraBuffBonus.cachedAuraSpecialEffects.Clear();
            auraBuffBonus.auraStrength = 0;

            foreach (var damagebase in abilityBase.damageLevels)
            {
                if (!Enum.TryParse("GLOBAL_" + damagebase.Key + "_DAMAGE_MIN", out BonusType damageTypeMin))
                    continue;

                if (!Enum.TryParse("GLOBAL_" + damagebase.Key + "_DAMAGE_MAX", out BonusType damageTypeMax))
                    continue;

                float minVal = damagebase.Value.damage[abilityLevel].min;
                float maxVal = damagebase.Value.damage[abilityLevel].max;

                auraBuffBonus.cachedAuraBonuses.Add(new TempEffectBonusContainer.StatusBonus(damageTypeMin, ModifyType.FLAT_ADDITION, minVal, 0));
                auraBuffBonus.cachedAuraBonuses.Add(new TempEffectBonusContainer.StatusBonus(damageTypeMax, ModifyType.FLAT_ADDITION, maxVal, 0));
                auraBuffBonus.auraStrength += minVal + maxVal;

                if (!GameManager.Instance.isInBattle && !abilityBase.isSoulAbility)
                {
                    if ((abilityBase.abilityType == AbilityType.AURA && abilityBase.targetType == AbilityTargetType.ALLY) || abilityBase.abilityType == AbilityType.SELF_BUFF)
                    {
                        data.AddTemporaryBonus(minVal * auraBuffBonus.auraEffectMultiplier * auraBuffBonus.selfAuraEffectMultiplier, damageTypeMin, ModifyType.FLAT_ADDITION, true);
                        data.AddTemporaryBonus(maxVal * auraBuffBonus.auraEffectMultiplier * auraBuffBonus.selfAuraEffectMultiplier, damageTypeMax, ModifyType.FLAT_ADDITION, true);
                    }
                }
            }

            foreach (AbilityScalingAddedEffect effect in abilityBase.appliedEffects)
            {
                if (effect.effectType == EffectType.BUFF || effect.effectType == EffectType.DEBUFF)
                {
                    float buffValue = (effect.initialValue + effect.growthValue * abilityLevel);
                    auraBuffBonus.cachedAuraBonuses.Add(new TempEffectBonusContainer.StatusBonus(effect.bonusType, effect.modifyType, buffValue, effect.duration));
                    auraBuffBonus.auraStrength += buffValue;

                    if (!GameManager.Instance.isInBattle && !abilityBase.isSoulAbility)
                    {
                        if ((abilityBase.abilityType == AbilityType.AURA && abilityBase.targetType == AbilityTargetType.ALLY) || abilityBase.abilityType == AbilityType.SELF_BUFF)
                            data.AddTemporaryBonus(buffValue * auraBuffBonus.auraEffectMultiplier * auraBuffBonus.selfAuraEffectMultiplier, effect.bonusType, effect.modifyType, true);
                    }
                }
                else
                {
                    auraBuffBonus.cachedAuraSpecialEffects.Add(new TempEffectBonusContainer.SpecialBonus(effect.effectType, effect.initialValue + effect.growthValue * abilityLevel, effect.duration));
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

    protected void UpdateShotParameters(ActorData data, IEnumerable<GroupType> tags)
    {
        // Attack AoE
        if (abilityBase.abilityType == AbilityType.ATTACK && abilityBase.useWeaponRangeForAOE)
        {
            if (data is HeroData hero)
            {
                //Get Weapon, use unarmed (1f) otherwise
                if (hero.GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon weapon)
                    AreaRadius = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AREA_RADIUS, BonusType.MELEE_ATTACK_RANGE).CalculateStat(weapon.WeaponRange);
                else
                    AreaRadius = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AREA_RADIUS, BonusType.MELEE_ATTACK_RANGE).CalculateStat(1f);
            }
            else if (data is EnemyData enemy)
            {
                AreaRadius = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AREA_RADIUS, BonusType.MELEE_ATTACK_RANGE).CalculateStat(TargetRange);
            }
        }
        else  // Spell AoE
        {
            AreaRadius = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AREA_RADIUS).CalculateStat(abilityBase.areaRadius);
        }

        AreaLength = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AREA_RADIUS).CalculateStat(abilityBase.areaLength);
        AreaScaling = AreaRadius / abilityBase.areaRadius;

        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
        {
            TargetRange = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.AURA_RANGE).CalculateStat(abilityBase.targetRange);
            return;
        }

        ProjectileSpeed = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_SPEED).CalculateStat(abilityBase.projectileSpeed);

        if (abilityBase.GetGroupTypes().Contains(GroupType.CANNOT_MODIFY_PROJECTILE_COUNT))
        {
            ProjectileCount = abilityBase.projectileCount;
        }
        else if (abilityBase.abilityShotType == AbilityShotType.HITSCAN_MULTI)
        {
            if (abilityBase.GetGroupTypes().Contains(GroupType.PROJECTILE))
                ProjectileCount = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.HITSCAN_MULTI_TARGET_COUNT, BonusType.PROJECTILE_COUNT).CalculateStat(abilityBase.projectileCount);
            else
                ProjectileCount = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.HITSCAN_MULTI_TARGET_COUNT).CalculateStat(abilityBase.projectileCount);
        }
        else
        {
            ProjectileCount = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_COUNT).CalculateStat(abilityBase.projectileCount);
        }

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

        if (abilityBase.abilityShotType == AbilityShotType.PROJECTILE)
        {
            if (data is HeroData)
                ProjectileHoming = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_HOMING).CalculateStat(15f);
            else
                ProjectileHoming = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PROJECTILE_HOMING).CalculateStat(0f);
        }
        else
        {
            ProjectileHoming = 0;
        }
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

            if (abilityBase.abilityShotType == AbilityShotType.NOVA_AOE)
            {
                data.GetMultiStatBonus(rangeBonus, abilityBonuses, tags, BonusType.AREA_RADIUS);
            }

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

            if (abilityBase.useWeaponRangeForAOE && abilityBase.useWeaponRangeForTargeting)
                data.GetMultiStatBonus(rangeBonus, abilityBonuses, tags, BonusType.AREA_RADIUS);

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

            MainCriticalChance = critBonus.CalculateStat(data.BaseEnemyData.attackCriticalChance);
            Cooldown = 1 / speedBonus.CalculateStat(data.BaseEnemyData.attackSpeed);
            TargetRange = rangeBonus.CalculateStat(data.BaseEnemyData.attackTargetRange);
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
            multi.UnionWith(Helpers.GetMultiplierTypes(abilityBase.abilityType, element));

            mainDamageBase[element].ClearBonuses();

            data.GetMultiStatBonus(mainDamageBase[element].minBonus, abilityBonuses, mainHandTags, min.ToArray());
            data.GetMultiStatBonus(mainDamageBase[element].maxBonus, abilityBonuses, mainHandTags, max.ToArray());
            data.GetMultiStatBonus(mainDamageBase[element].multiplierBonus, abilityBonuses, mainHandTags, multi.ToArray());

            HashSet<BonusType> availableConversions = data.BonusesIntersection(abilityBonuses.Keys, Helpers.GetConversionTypes(element));
            if (availableConversions.Count > 0)
            {
                GetElementConversionValues(data, mainHandTags, availableConversions, mainDamageBase[element].conversions, abilityBonuses);
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
                    GetElementConversionValues(data, offHandTags, availableConversions, offhandDamageBase[element].conversions, abilityBonuses);
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

    public static MinMaxRange CalculateDamageConversion(ActorData data, int[] convertedDamageMin,
                                       int[] convertedDamageMax, IEnumerable<GroupType> tags,
                                       MinMaxRange damageContainer, float[] conversions, ElementType element,
                                       HashSet<BonusType> multi, StatBonus minBonus, StatBonus maxBonus, StatBonus multiplierBonus)
    {
        float baseMin = damageContainer.min + minBonus.CalculateStat(0);
        float baseMax = damageContainer.max + maxBonus.CalculateStat(0);

        float finalBaseMin = baseMin;
        float finalBaseMax = baseMax;

        for (int i = 0; i < 7; i++)
        {
            if (conversions[i] == 0)
                continue;

            float convertedMin = baseMin * conversions[i];
            float convertedMax = baseMax * conversions[i];

            finalBaseMin -= convertedMin;
            finalBaseMax -= convertedMax;

            HashSet<BonusType> bonusesForConverted = new HashSet<BonusType>(multi);
            bonusesForConverted.UnionWith(Helpers.GetMultiplierTypes(AbilityType.NON_ABILITY, element, (ElementType)i));
            StatBonus totalMultiplierBonus = data.GetMultiStatBonus(null, tags, bonusesForConverted.ToArray());
            totalMultiplierBonus.AddBonuses(multiplierBonus);

            convertedMin = totalMultiplierBonus.CalculateStat(convertedMin);
            convertedMax = totalMultiplierBonus.CalculateStat(convertedMax);

            convertedDamageMin[i] += (int)Math.Max(convertedMin, 0);
            convertedDamageMax[i] += (int)Math.Max(convertedMax, 0);
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
            StatBonus finalMultiplier = data.GetMultiStatBonus(null, tags, multi.ToArray());
            finalMultiplier.AddBonuses(multiplierBonus);
            returnValue.min = (int)Math.Max(finalMultiplier.CalculateStat(finalBaseMin), 0);
            returnValue.max = (int)Math.Max(finalMultiplier.CalculateStat(finalBaseMax), 0);
        }

        return returnValue;
    }

    public static void GetElementConversionValues(ActorData data, IEnumerable<GroupType> weaponTags, HashSet<BonusType> availableConversions, float[] conversionsArray, Dictionary<BonusType, StatBonus> abilitySpecificBonuses)
    {
        int conversionSum = 0;
        HashSet<ElementType> elementList = new HashSet<ElementType>();
        foreach (BonusType conversion in availableConversions)
        {
            ElementType convertTo = (ElementType)Enum.Parse(typeof(ElementType), conversion.ToString().Split('_')[2]);
            elementList.Add(convertTo);
            int conversionValue = data.GetMultiStatBonus(abilitySpecificBonuses, weaponTags, conversion).CalculateStat(0);
            conversionsArray[(int)convertTo] = conversionValue;
            conversionSum += conversionValue;
        }
        foreach (ElementType convert in elementList)
        {
            if (conversionSum > 100)
            {
                conversionsArray[(int)convert] /= conversionSum;
            }
            else
            {
                conversionsArray[(int)convert] /= 100f;
            }
        }
    }

    protected void UpdateDamage(EnemyData data, Dictionary<ElementType, AbilityDamageBase> damageLevels, IEnumerable<GroupType> tags, float damageModifier = 1.0f)
    {
        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
            return;

        int[] mainConvertedDamageMin = new int[7];
        int[] mainConvertedDamageMax = new int[7];

        float flatDamageMod = abilityBase.flatDamageMultiplier;
        AbilityType abilityType = abilityBase.abilityType;

        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            mainDamageBase[element].baseMin = 0;
            mainDamageBase[element].baseMax = 0;
            MinMaxRange abilityBaseDamage;

            if (damageLevels.ContainsKey(element))
            {
                abilityBaseDamage = damageLevels[element].damage[abilityLevel];
            }
            else
            {
                abilityBaseDamage = new MinMaxRange();
            }

            if (abilityBase.abilityType == AbilityType.ATTACK && element == ElementType.PHYSICAL)
            {
                flatDamageMod = abilityBase.weaponMultiplier + abilityBase.weaponMultiplierScaling * 50;
                mainDamageBase[element].baseMin = (abilityBaseDamage.min + data.minAttackDamage) * flatDamageMod;
                mainDamageBase[element].baseMax = (abilityBaseDamage.max + data.maxAttackDamage) * flatDamageMod;
            }
            else
            {
                mainDamageBase[element].baseMin = abilityBaseDamage.min;
                mainDamageBase[element].baseMax = abilityBaseDamage.max;
            }

            IList<GroupType> damageTags = abilityBase.GetGroupTypes();
            HashSet<GroupType> mainHandTags = AbilityOwner.GetActorTagsAndDataTags();

            HashSet<BonusType> min = new HashSet<BonusType>();
            HashSet<BonusType> max = new HashSet<BonusType>();
            HashSet<BonusType> multi = new HashSet<BonusType>();

            Helpers.GetGlobalAndFlatDamageTypes(element, abilityBase.abilityType, abilityBase.abilityShotType, damageTags, min, max, multi);
            multi.UnionWith(Helpers.GetMultiplierTypes(abilityBase.abilityType, element));

            mainDamageBase[element].ClearBonuses();

            data.GetMultiStatBonus(mainDamageBase[element].minBonus, abilityBonuses, mainHandTags, min.ToArray());
            data.GetMultiStatBonus(mainDamageBase[element].maxBonus, abilityBonuses, mainHandTags, max.ToArray());
            data.GetMultiStatBonus(mainDamageBase[element].multiplierBonus, abilityBonuses, mainHandTags, multi.ToArray());

            mainDamageBase[element].CalculateRange(flatDamageMod, finalDamageModifier);

            //Debug.Log(mainDamageBase[element].calculatedRange.min + " " + mainDamageBase[element].calculatedRange.max + " " + element);
        }
    }

    protected void UpdateOnHitDataBonuses(ActorData data, IEnumerable<GroupType> tags)
    {
        string triggeredEffectSourceName = abilityBase.idName + abilitySlot;

        ClearTriggeredEffects(data, triggeredEffectSourceName);
        foreach (TriggeredEffectBonusProperty triggerProp in abilityBase.triggeredEffects)
        {
            TriggeredEffect triggeredEffect = new TriggeredEffect(triggerProp, triggerProp.effectMinValue + triggerProp.effectMaxValue * abilityLevel, triggeredEffectSourceName);
            triggeredEffects[triggerProp.triggerType].Add(triggeredEffect);

            if (!abilityBase.isSoulAbility && (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF))
                data.TriggeredEffects[triggerProp.triggerType].Add(triggeredEffect);
        }

        if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
            return;

        abilityOnHitData.UpdateStatusEffectData(data, tags, abilityBonuses);

        StatBonus physicalNegate = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.PHYSICAL_RESISTANCE_NEGATION);
        abilityOnHitData.SetNegation(ElementType.PHYSICAL, physicalNegate.CalculateStat(0));

        StatBonus fireNegate = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.FIRE_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION);
        abilityOnHitData.SetNegation(ElementType.FIRE, fireNegate.CalculateStat(0));

        StatBonus coldNegate = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.COLD_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION);
        abilityOnHitData.SetNegation(ElementType.COLD, coldNegate.CalculateStat(0));

        StatBonus lightningNegate = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.LIGHTNING_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION);
        abilityOnHitData.SetNegation(ElementType.LIGHTNING, lightningNegate.CalculateStat(0));

        StatBonus earthNegate = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.EARTH_RESISTANCE_NEGATION, BonusType.ELEMENTAL_RESISTANCE_NEGATION);
        abilityOnHitData.SetNegation(ElementType.EARTH, earthNegate.CalculateStat(0));

        StatBonus divineNegate = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.DIVINE_RESISTANCE_NEGATION, BonusType.PRIMORDIAL_RESISTANCE_NEGATION);
        abilityOnHitData.SetNegation(ElementType.DIVINE, divineNegate.CalculateStat(0));

        StatBonus voidNegate = data.GetMultiStatBonus(abilityBonuses, tags, BonusType.VOID_RESISTANCE_NEGATION, BonusType.PRIMORDIAL_RESISTANCE_NEGATION);
        abilityOnHitData.SetNegation(ElementType.VOID, voidNegate.CalculateStat(0));
    }
}