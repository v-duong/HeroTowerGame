using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ActorAbility
{
    public readonly AbilityBase abilityBase;
    public int abilityLevel;
    public AbilityColliderContainer abilityCollider;
    public Dictionary<ElementType, AbilityDamageContainer> mainDamageBase = new Dictionary<ElementType, AbilityDamageContainer>();
    public Dictionary<ElementType, AbilityDamageContainer> offhandDamageBase = new Dictionary<ElementType, AbilityDamageContainer>();
    private readonly Dictionary<BonusType, StatBonus> abilityBonuses;
    public Dictionary<TriggerType, List<TriggeredEffect>> triggeredEffects;

    public AbilityOnHitDataContainer abilityOnHitData;
    public LinkedActorAbility LinkedAbility { get; private set; }
    public List<Actor> targetList = new List<Actor>();
    public Actor CurrentTarget { get; private set; }

    public Actor AbilityOwner { get; private set; }
    public float AreaLength { get; private set; }
    public float AreaRadius { get; private set; }
    public float Cooldown { get; private set; }
    public float HitscanDelay { get; private set; }
    public float ProjectileSize { get; private set; }
    public float ProjectileSpeed { get; private set; }
    public int ProjectilePierce { get; private set; }
    public int ProjectileChain { get; private set; }
    public float ProjectileHoming { get; private set; }
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
    public float soulCooldown;
    public float currentSoulCooldownTimer;
    public float soulCost;
    public float soulAbilityDuration;

    private TempEffectBonusContainer auraBuffBonus;

    public int targetLayer;
    public int targetMask;
    public int abilitySlot;
    public HashSet<GroupType> abilityGroupTypes;
    protected float finalDamageModifier = 1.0f;

    private float AreaScaling;
    private float ProjectileScaling;

    private bool firingRoutineRunning;
    private IEnumerator firingRoutine;
    private ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();

    private Coroutine currentShotCoroutine;

    public ActorAbility(AbilityBase ability, int layer)
    {
        if (ability == null)
            return;
        abilityBase = ability;

        abilityBonuses = new Dictionary<BonusType, StatBonus>();

        abilityOnHitData = new AbilityOnHitDataContainer(this);

        abilityGroupTypes = new HashSet<GroupType>();
        abilityGroupTypes.UnionWith(ability.GetGroupTypes());

        if (ability.abilityType == AbilityType.ATTACK)
            abilityGroupTypes.Add(GroupType.ATTACK);
        else if (ability.abilityType == AbilityType.SPELL)
            abilityGroupTypes.Add(GroupType.SPELL);

        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            mainDamageBase[element] = new AbilityDamageContainer();
            offhandDamageBase[element] = new AbilityDamageContainer();
        }

        triggeredEffects = new Dictionary<TriggerType, List<TriggeredEffect>>();
        foreach (TriggerType t in Enum.GetValues(typeof(TriggerType)))
        {
            triggeredEffects.Add(t, new List<TriggeredEffect>());
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

        soulCooldown = abilityBase.cooldownTime;
        soulCost = abilityBase.soulCost;
        soulAbilityDuration = abilityBase.soulAbilityDuration;

        if (abilityBase.abilityType == AbilityType.ATTACK)
            AlternatesAttacks = !abilityBase.useBothWeaponsForDual;
        else
            AlternatesAttacks = false;

        auraBuffBonus = new TempEffectBonusContainer
        {
            cachedAuraBonuses = new List<TempEffectBonusContainer.StatusBonus>(),
            cachedAuraSpecialEffects = new List<TempEffectBonusContainer.SpecialBonus>()
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

    public void SetAbilitySlotNum(int slot)
    {
        abilitySlot = slot;
        if (slot > 0 && !abilityBase.isSoulAbility)
        {
            StatBonus speedPenalty = new StatBonus();
            speedPenalty.AddBonus(ModifyType.MULTIPLY, -25);
            StatBonus damagePenalty = new StatBonus();
            damagePenalty.AddBonus(ModifyType.MULTIPLY, -33);
            abilityBonuses.Add(BonusType.GLOBAL_ABILITY_SPEED, speedPenalty);
            abilityBonuses.Add(BonusType.GLOBAL_DAMAGE, damagePenalty);
        }
    }

    public void SetDamageAndSpeedModifier(float damage, float speed)
    {
        StatBonus damageMod = new StatBonus();
        StatBonus speedMod = new StatBonus();
        damageMod.AddBonus(ModifyType.MULTIPLY, damage);
        speedMod.AddBonus(ModifyType.MULTIPLY, speed);
        abilityBonuses.Add(BonusType.GLOBAL_DAMAGE, damageMod);
        abilityBonuses.Add(BonusType.AURA_EFFECT, damageMod);
        abilityBonuses.Add(BonusType.GLOBAL_ABILITY_SPEED, speedMod);
    }

    public void SetAbilityOwner(Actor actor)
    {
        AbilityOwner = actor;
        if (AbilityOwner is EnemyActor)
            abilityOnHitData.accuracy = (float)Helpers.GetEnemyAccuracyScaling(AbilityOwner.Data.Level);
        if (LinkedAbility != null)
            LinkedAbility.SetAbilityOwner(AbilityOwner);
    }

    protected void UpdateCurrentTarget()
    {
        if (targetList.Count > 0)
        {
            if (AbilityOwner.forcedTarget != null && !AbilityOwner.forcedTarget.Data.IsDead && targetList.Contains(AbilityOwner.forcedTarget))
            {
                CurrentTarget = AbilityOwner.forcedTarget;
                return;
            }

            RarityType rarity = RarityType.NORMAL;
            bool foundAttacker = false;
            bool prioritizeRarity = AbilityOwner.targetingFlags.HasFlag(SecondaryTargetingFlags.PRIORITIZE_RARITY);
            bool prioritizeAttacker = AbilityOwner.targetingFlags.HasFlag(SecondaryTargetingFlags.PRIORITIZE_ATTACKERS);

            switch (AbilityOwner.targetingPriority)
            {
                case PrimaryTargetingType.CLOSEST:
                    float maxDistance = float.PositiveInfinity;
                    foreach (Actor actor in targetList)
                    {
                        if (CheckSecondaryFlags(prioritizeRarity, prioritizeAttacker, ref rarity, ref foundAttacker, actor))
                            continue;

                        float distance = Vector3.SqrMagnitude(actor.transform.position - AbilityOwner.transform.position);
                        if (Vector3.SqrMagnitude(actor.transform.position - AbilityOwner.transform.position) < maxDistance)
                        {
                            maxDistance = distance;
                            CurrentTarget = actor;
                        }
                    }
                    break;

                case PrimaryTargetingType.FURTHEST:
                    float minDistance = 0;

                    foreach (Actor actor in targetList)
                    {
                        if (CheckSecondaryFlags(prioritizeRarity, prioritizeAttacker, ref rarity, ref foundAttacker, actor))
                            continue;

                        float distance = Vector3.SqrMagnitude(actor.transform.position - AbilityOwner.transform.position);
                        if (distance > minDistance)
                        {
                            minDistance = distance;
                            CurrentTarget = actor;
                        }
                    }
                    break;

                case PrimaryTargetingType.FIRST:
                    int maxNode = 0;
                    float maxNodeDistance = 1f;

                    if (AbilityOwner.GetActorType() == ActorType.ENEMY)
                        CurrentTarget = targetList[UnityEngine.Random.Range(0, targetList.Count)];

                    foreach (Actor actor in targetList)
                    {
                        if (CheckSecondaryFlags(prioritizeRarity, prioritizeAttacker, ref rarity, ref foundAttacker, actor))
                            continue;

                        if (actor is EnemyActor)
                        {
                            if (actor.NextMovementNode > maxNode || actor.NextMovementNode == maxNode && ((EnemyActor)actor).distanceToNextNode < maxNodeDistance)
                            {
                                maxNode = actor.NextMovementNode;
                                maxNodeDistance = ((EnemyActor)actor).distanceToNextNode;
                                CurrentTarget = actor;
                            }
                        }
                    }

                    break;

                case PrimaryTargetingType.LAST:
                    int minNode = int.MaxValue;
                    float minNodeDistance = 0f;

                    if (AbilityOwner.GetActorType() == ActorType.ENEMY)
                        CurrentTarget = targetList[UnityEngine.Random.Range(0, targetList.Count)];

                    foreach (Actor actor in targetList)
                    {
                        if (CheckSecondaryFlags(prioritizeRarity, prioritizeAttacker, ref rarity, ref foundAttacker, actor))
                            continue;

                        if (actor is EnemyActor)
                        {
                            if (actor.NextMovementNode < minNode || actor.NextMovementNode == minNode && ((EnemyActor)actor).distanceToNextNode > minNodeDistance)
                            {
                                maxNode = actor.NextMovementNode;
                                maxNodeDistance = ((EnemyActor)actor).distanceToNextNode;
                                CurrentTarget = actor;
                            }
                        }
                    }

                    break;

                case PrimaryTargetingType.RANDOM:
                    CurrentTarget = targetList[UnityEngine.Random.Range(0, targetList.Count)];
                    break;

                case PrimaryTargetingType.LEAST_HEALTH:
                    float minHealth = float.PositiveInfinity;

                    foreach (Actor actor in targetList)
                    {
                        if (CheckSecondaryFlags(prioritizeRarity, prioritizeAttacker, ref rarity, ref foundAttacker, actor))
                            continue;

                        float currentHealth = (actor.Data.CurrentHealth + actor.Data.CurrentManaShield) / actor.Data.AggroPriorityModifier;
                        if (currentHealth < minHealth)
                        {
                            minHealth = currentHealth;
                            CurrentTarget = actor;
                        }
                    }
                    break;

                case PrimaryTargetingType.MOST_HEALTH:
                    float maxHealth = 0;

                    foreach (Actor actor in targetList)
                    {
                        if (CheckSecondaryFlags(prioritizeRarity, prioritizeAttacker, ref rarity, ref foundAttacker, actor))
                            continue;

                        float currentHealth = (actor.Data.CurrentHealth + actor.Data.CurrentManaShield) * actor.Data.AggroPriorityModifier;
                        if (currentHealth > maxHealth)
                        {
                            maxHealth = currentHealth;
                            CurrentTarget = actor;
                        }
                    }
                    break;

                case PrimaryTargetingType.LOWEST_HEALTH_PERCENT:
                    float lowestHealthPercent = float.PositiveInfinity;

                    foreach (Actor actor in targetList)
                    {
                        if (CheckSecondaryFlags(prioritizeRarity, prioritizeAttacker, ref rarity, ref foundAttacker, actor))
                            continue;

                        float currentHealthPercent = ((actor.Data.CurrentHealth + actor.Data.CurrentManaShield) / (actor.Data.MaximumHealth + actor.Data.CurrentManaShield)) / actor.Data.AggroPriorityModifier;
                        if (currentHealthPercent < lowestHealthPercent)
                        {
                            lowestHealthPercent = currentHealthPercent;
                            CurrentTarget = actor;
                        }
                    }
                    break;

                case PrimaryTargetingType.HIGHEST_HEALTH_PERCENT:
                    float highestHealthPercent = 0;

                    foreach (Actor actor in targetList)
                    {
                        if (CheckSecondaryFlags(prioritizeRarity, prioritizeAttacker, ref rarity, ref foundAttacker, actor))
                            continue;

                        float currentHealthPercent = ((actor.Data.CurrentHealth + actor.Data.CurrentManaShield) / (actor.Data.MaximumHealth + actor.Data.CurrentManaShield)) * actor.Data.AggroPriorityModifier;
                        if (currentHealthPercent > highestHealthPercent)
                        {
                            highestHealthPercent = currentHealthPercent;
                            CurrentTarget = actor;
                        }
                    }
                    break;
                    break;

                default:
                    CurrentTarget = targetList[0];
                    break;
            }
        }
        else
        {
            CurrentTarget = null;
        }
    }

    private bool CheckSecondaryFlags(bool priorityRarity, bool prioritizeAttacker, ref RarityType highestRarity, ref bool foundAttacker, Actor actor)
    {
        if (actor is EnemyActor)
        {
            if (priorityRarity && ((EnemyActor)actor).enemyRarity < highestRarity)
            {
                return true;
            }
            else
            {
                highestRarity = ((EnemyActor)actor).enemyRarity;
            }

            if (prioritizeAttacker
                && ((EnemyActor)actor).Data.BaseEnemyData.enemyType != EnemyType.TARGET_ATTACKER
                && ((EnemyActor)actor).Data.BaseEnemyData.enemyType != EnemyType.HIT_AND_RUN)
            {
                return true;
            }
            else
            {
                foundAttacker = true;
            }

            return false;
        }
        else
        {
            return false;
        }
    }

    public void AddToTargetList(Actor actor)
    {
        if (actor == AbilityOwner)
            return;
        targetList.Add(actor);
        //UpdateCurrentTarget(actor);
    }

    public void RemoveFromTargetList(Actor actor)
    {
        targetList.Remove(actor);
        if (CurrentTarget == actor)
        {
            //UpdateCurrentTarget(actor);
        }
    }

    public void ClearTriggeredEffects(ActorData data, string triggeredEffectSourceName)
    {
        foreach (TriggerType t in Enum.GetValues(typeof(TriggerType)))
        {
            triggeredEffects[t].Clear();

            if (abilityBase.abilityType == AbilityType.AURA || abilityBase.abilityType == AbilityType.SELF_BUFF)
                data.TriggeredEffects[t].RemoveAll(x => x.sourceName == triggeredEffectSourceName);
        }
    }

    public Dictionary<ElementType, float> CalculateDamageValues(Actor target, Actor source, float blockDamageModifier)
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

        foreach (TriggeredEffect triggeredEffect in source.Data.TriggeredEffects[TriggerType.WHEN_HITTING])
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
            minDamage[(int)elementType] = dicToUse[elementType].calculatedRange.min;
            maxDamage[(int)elementType] = dicToUse[elementType].calculatedRange.max;

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
                GetElementConversionValues(source.Data, weaponTags, availableConversions, dicToUse[elementType].conversions, abilityBonuses);
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

        bool isCrit = UnityEngine.Random.Range(0f, 100f) < critChance;
        if (isCrit)
        {
            abilityOnHitData.ApplyTriggerEffects(TriggerType.ON_CRIT, target);
        }

        foreach (ElementType elementType in values)
        {
            float damage = UnityEngine.Random.Range(minDamage[(int)elementType] + convertedMinDamage[(int)elementType], maxDamage[(int)elementType] + convertedMaxDamage[(int)elementType] + 1);
            if (damage == 0)
                continue;

            if (isCrit)
                damage *= criticalDamage;

            returnDict.Add(elementType, damage * abilityBase.hitDamageModifier * blockDamageModifier);
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
                    firingRoutine = FiringRoutine_Aura();
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
            if (AbilityOwner is HeroActor hero && hero.isBeingRecalled)
                yield return null;

            if (AbilityOwner.attackLocks > 0)
                yield return null;

            if (targetList.Count > 0)
            {
                UpdateCurrentTarget();

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
            }

            if (fired)
            {
                fired = false;
                while (currentShotCoroutine != null)
                {
                    yield return null;
                }
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
                    if (abilityBase.targetType != AbilityTargetType.ENEMY)
                        ApplyAuraBuff(AbilityOwner, 0.75f);
                    foreach (Actor target in targetList)
                        ApplyAuraBuff(target, 0.75f);
                    break;

                case AbilityType.SELF_BUFF:
                    ApplyAuraBuff(AbilityOwner, float.PositiveInfinity);
                    yield break;

                default:
                    break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    public void FireSoulAbility(Actor selectedTarget, Vector3 selectedPosition, List<Actor> targets = null)
    {
        if (selectedTarget == null)
            return;

        AbilityOwner.Data.CurrentSoulPoints -= soulCost;
        currentSoulCooldownTimer = soulCooldown;

        if (targets == null)
        {
            targets = new List<Actor>() {
                selectedTarget
               };
        }

        if (abilityBase.useAreaAroundTarget)
        {
            var hits = Physics2D.OverlapCircleAll(selectedTarget.transform.position, AreaRadius, targetMask);

            foreach (Collider2D hit in hits)
            {
                Actor actor = hit.gameObject.GetComponent<Actor>();
                if (actor != null && !targets.Contains(actor))
                {
                    targets.Add(actor);
                }
            }
        }

        float soulEffectMultiplier = AbilityOwner.Data.GetMultiStatBonus(abilityBonuses, AbilityOwner.GetActorTagsAndDataTags(), BonusType.SOUL_ABILITY_EFFECT).CalculateStat(1f);

        List<TempEffectBonusContainer.StatusBonus> damageBonus = null;
        float buffPower = 0;

        if (abilityBase.abilityType == AbilityType.AREA_BUFF || abilityBase.abilityType == AbilityType.SELF_BUFF)
        {
            damageBonus = GetDamageBaseAsBuffs(soulEffectMultiplier, ref buffPower);
        }

        foreach (Actor target in targets)
        {
            if (abilityBase.abilityType == AbilityType.AREA_BUFF || abilityBase.abilityType == AbilityType.SELF_BUFF)
            {
                AddTriggeredEffectsAsBuffs(target, soulEffectMultiplier);
                ApplySourcedBuffToTarget(damageBonus, target, abilityBase.idName + ".damageBuff", buffPower, soulAbilityDuration);
            }

            ApplySoulEffectBuffs(target, soulEffectMultiplier, damageBonus);
        }

        if (abilityBase.abilityType != AbilityType.AREA_BUFF && abilityBase.abilityType != AbilityType.SELF_BUFF)
        {
            switch (abilityBase.abilityShotType)
            {
                case AbilityShotType.PROJECTILE:
                    foreach (Actor target in targets)
                    {
                        AbilityOwner.StartCoroutine(FireProjectile(AbilityOwner.transform.position, target));
                    }
                    break;

                case AbilityShotType.PROJECTILE_NOVA:
                    AbilityOwner.StartCoroutine(FireProjectile(AbilityOwner.transform.position, null, AbilityOwner.transform.position + new Vector3(UnityEngine.Random.Range(0.1f, 0.5f), UnityEngine.Random.Range(0.1f, 0.5f), 0)));
                    break;

                case AbilityShotType.ARC_AOE:
                case AbilityShotType.NOVA_ARC_AOE:
                    FireArcAoe(AbilityOwner.transform.position, selectedPosition);
                    break;

                case AbilityShotType.NOVA_AOE:
                case AbilityShotType.RADIAL_AOE:
                    FireRadialAoe(AbilityOwner.transform.position, selectedPosition);
                    break;

                case AbilityShotType.HITSCAN_SINGLE:
                    foreach (Actor target in targets)
                    {
                        AbilityOwner.StartCoroutine(FireHitscan(AbilityOwner.transform.position, target, null));
                    }
                    break;

                case AbilityShotType.HITSCAN_MULTI:
                    FireHitscanMulti(AbilityOwner.transform.position);
                    break;

                case AbilityShotType.LINEAR_AOE:
                    FireLinearAoe(AbilityOwner.transform.position, selectedPosition);
                    break;

                case AbilityShotType.FORWARD_MOVING_ARC:
                case AbilityShotType.FORWARD_MOVING_RADIAL:
                case AbilityShotType.FORWARD_MOVING_LINEAR:
                    FireMovingAoe();
                    break;
            }
        }
    }

    protected List<TempEffectBonusContainer.StatusBonus> GetDamageBaseAsBuffs(float soulEffectMultiplier, ref float buffPower)
    {
        List<TempEffectBonusContainer.StatusBonus> statusBonuses = new List<TempEffectBonusContainer.StatusBonus>();

        foreach (var damagebase in abilityBase.damageLevels)
        {
            if (!Enum.TryParse("GLOBAL_" + damagebase.Key + "_DAMAGE_MIN", out BonusType damageTypeMin))
                continue;

            if (!Enum.TryParse("GLOBAL_" + damagebase.Key + "_DAMAGE_MAX", out BonusType damageTypeMax))
                continue;

            float minVal = damagebase.Value.damage[abilityLevel].min * soulEffectMultiplier;
            float maxVal = damagebase.Value.damage[abilityLevel].max * soulEffectMultiplier;

            statusBonuses.Add(new TempEffectBonusContainer.StatusBonus(damageTypeMin, ModifyType.FLAT_ADDITION, minVal, 0));
            statusBonuses.Add(new TempEffectBonusContainer.StatusBonus(damageTypeMax, ModifyType.FLAT_ADDITION, maxVal, 0));
            buffPower += minVal + maxVal;
        }

        return statusBonuses;
    }

    protected void ApplySoulEffectBuffs(Actor target, float soulEffectMultiplier, List<TempEffectBonusContainer.StatusBonus> damageBonus)
    {
        for (int i = 0; i < abilityBase.appliedEffects.Count; i++)
        {
            AbilityScalingAddedEffect appliedEffect = abilityBase.appliedEffects[i];

            if (appliedEffect.effectType == EffectType.BUFF || appliedEffect.effectType == EffectType.DEBUFF)
            {
                string buffName = abilityBase.idName + ".soulBuff" + i;
                float buffPower = soulEffectMultiplier * (appliedEffect.initialValue + appliedEffect.growthValue * abilityLevel);

                TempEffectBonusContainer.StatusBonus newBonus = new TempEffectBonusContainer.StatusBonus(appliedEffect.bonusType, appliedEffect.modifyType, buffPower, appliedEffect.duration);

                ApplySourcedBuffToTarget(newBonus, target, buffName);
            }
            else
            {
                float effectPower = AbilityOwner.Data.GetMultiStatBonus(abilityBonuses, null, BonusType.SOUL_ABILITY_EFFECT).CalculateStat(appliedEffect.initialValue + appliedEffect.growthValue * abilityLevel);

                ActorEffect.ApplyEffectToTarget(target, AbilityOwner, appliedEffect.effectType, effectPower, appliedEffect.duration, 1);
            }
        }
    }

    protected void ApplySourcedBuffToTarget(TempEffectBonusContainer.StatusBonus statusBonus, Actor target, string buffName)
    {
        SourcedActorBuffEffect buff = null;
        List<SourcedActorBuffEffect> buffs = target.GetBuffStatusEffect(buffName);

        if (buffs.Count > 0)
            buff = buffs[0];

        float buffPower = statusBonus.effectValue;

        if (buff != null)
        {
            if (buffPower == buff.BuffPower)
            {
                buff.RefreshDuration(statusBonus.effectDuration);
                return;
            }
            else if (buffPower < buff.BuffPower)
            {
                return;
            }
            else
            {
                target.RemoveStatusEffect(buff, true);
            }
        }

        target.AddStatusEffect(new StatBonusBuffEffect(target, AbilityOwner, statusBonus, statusBonus.effectDuration, buffName, BuffType, 1f));
    }

    protected void ApplySourcedBuffToTarget(List<TempEffectBonusContainer.StatusBonus> statusBonus, Actor target, string buffName, float buffPower, float duration)
    {
        SourcedActorBuffEffect buff = null;
        List<SourcedActorBuffEffect> buffs = target.GetBuffStatusEffect(buffName);

        if (buffs.Count > 0)
            buff = buffs[0];

        if (buff != null)
        {
            if (buffPower == buff.BuffPower)
            {
                buff.RefreshDuration(duration);
                return;
            }
            else if (buffPower < buff.BuffPower)
            {
                return;
            }
            else
            {
                target.RemoveStatusEffect(buff, true);
            }
        }

        target.AddStatusEffect(new StatBonusBuffEffect(target, AbilityOwner, statusBonus, duration, buffName, BuffType, 1f));
    }

    private void AddTriggeredEffectsAsBuffs(Actor target, float soulEffectMulti)
    {
        for (int i = 0; i < abilityBase.triggeredEffects.Count; i++)
        {
            TriggeredEffectBonusProperty triggeredEffectProp = abilityBase.triggeredEffects[i];
            SourcedActorBuffEffect triggeredEffect = null;
            string triggeredEffectName = abilityBase.idName + "." + triggeredEffectProp.triggerType + "." + triggeredEffectProp.triggerType;
            List<SourcedActorBuffEffect> buffs = target.GetBuffStatusEffect(triggeredEffectName);
            if (buffs.Count > 0)
                triggeredEffect = buffs[0];

            float effectPower = soulEffectMulti * (triggeredEffectProp.effectMinValue + triggeredEffectProp.effectMaxValue * abilityLevel);

            if (triggeredEffect != null)
            {
                if (effectPower == triggeredEffect.BuffPower)
                {
                    triggeredEffect.RefreshDuration(soulAbilityDuration);
                    continue;
                }
                else if (effectPower < triggeredEffect.BuffPower)
                {
                    continue;
                }
                else
                {
                    target.RemoveStatusEffect(triggeredEffect, true);
                }
            }

            target.AddStatusEffect(new TemporaryTriggerEffectBuff(target,
                                                           AbilityOwner,
                                                           triggeredEffectProp,
                                                           effectPower,
                                                           soulAbilityDuration,
                                                           triggeredEffectName,
                                                           BuffType));
        }
    }

    protected void ApplyAuraBuff(Actor target, float duration)
    {
        float auraMultiplier = auraBuffBonus.auraEffectMultiplier;
        if (target == AbilityOwner)
            auraMultiplier *= auraBuffBonus.selfAuraEffectMultiplier;

        if (auraMultiplier <= 0f)
            return;

        if (auraBuffBonus.cachedAuraBonuses.Count > 0)
        {
            SourcedActorBuffEffect buff = null;
            List<SourcedActorBuffEffect> buffs = target.GetBuffStatusEffect(abilityBase.idName);
            if (buffs.Count > 0)
                buff = buffs[0];

            if (buff != null)
            {
                if (auraBuffBonus.auraStrength == buff.BuffPower)
                {
                    buff.RefreshDuration(0.75f);
                }
                else if (auraBuffBonus.auraStrength < buff.BuffPower)
                {
                }
                else
                {
                    target.RemoveStatusEffect(buff, true);
                    target.AddStatusEffect(new StatBonusBuffEffect(target, AbilityOwner, auraBuffBonus.cachedAuraBonuses, duration, abilityBase.idName, BuffType, auraMultiplier));
                }
            }
            else
            {
                target.AddStatusEffect(new StatBonusBuffEffect(target, AbilityOwner, auraBuffBonus.cachedAuraBonuses, duration, abilityBase.idName, BuffType, auraMultiplier));
            }
        }

        for (int i = 0; i < auraBuffBonus.cachedAuraSpecialEffects.Count; i++)
        {
            TempEffectBonusContainer.SpecialBonus specialEffect = auraBuffBonus.cachedAuraSpecialEffects[i];
            ActorEffect.ApplyEffectToTarget(target, AbilityOwner, specialEffect.effectType, specialEffect.effectValue, 0.75f, auraMultiplier);
        }
    }

    protected void FireRadialAoe(Vector3 origin, Vector3 target)
    {
        Collider2D[] hits;

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

        ParticleManager.Instance.EmitAbilityParticle(abilityBase.idName, emitParams, AreaScaling, AbilityOwner.transform);

        foreach (Collider2D hit in hits)
        {
            Actor actor = hit.gameObject.GetComponent<Actor>();
            if (actor != null)
            {
                ApplyDamageToActor(actor, true);
            }
        }
    }

    protected IEnumerator FireHitscan(Vector3 origin, Actor target, List<Actor> sharedHitlist)
    {
        Actor lastHitTarget;

        yield return new WaitForSeconds(HitscanDelay);

        AbilityParticleSystem abilityParticleSystem = ParticleManager.Instance.GetParticleSystem(abilityBase.idName);

        if (abilityParticleSystem != null && abilityParticleSystem.extraEmitOnSelf)
        {
            emitParams.position = AbilityOwner.transform.position;
            ParticleManager.Instance.EmitAbilityParticle(abilityParticleSystem, emitParams, 1, AbilityOwner.transform);
        }

        emitParams.position = target.transform.position;
        ParticleManager.Instance.EmitAbilityParticle(abilityParticleSystem, emitParams, 1, AbilityOwner.transform);
        ApplyDamageToActor(target, true);

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

                ApplyDamageToActor(actor, true);

                emitParams.position = actor.transform.position;
                ParticleManager.Instance.EmitAbilityParticle(abilityParticleSystem, emitParams, 1, AbilityOwner.transform);

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
                    AbilityOwner.StartCoroutine(FireHitscan_Chained(origin, possibleTargets[index], ProjectileChain - 1, hitList, abilityParticleSystem));
                }
            }
        }

        yield break;
    }

    protected IEnumerator FireHitscan_Chained(Vector3 origin, Actor target, int remainingChainCount, List<Actor> hitList, AbilityParticleSystem abilityParticleSystem)
    {
        emitParams.position = target.transform.position;

        yield return new WaitForSeconds(HitscanDelay / 2);

        ParticleManager.Instance.EmitAbilityParticle(abilityParticleSystem, emitParams, 1, AbilityOwner.transform);
        ApplyDamageToActor(target, true);
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
                    AbilityOwner.StartCoroutine(FireHitscan_Chained(origin, possibleTargets[index], remainingChainCount - 1, hitList, abilityParticleSystem));
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
                    ApplyDamageToActor(actor, true);
                }
            }
        }
    }

    protected IEnumerator FireProjectile(Vector3 origin, Actor target, Vector3? positionOverride = null)
    {
        Vector3 heading;

        if (positionOverride != null)
        {
            heading = ((Vector3)positionOverride - origin).normalized;
        }
        else
        {
            heading = (target.transform.position - origin).normalized;
        }

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
            pooledProjectile.transform.up = (pooledProjectile.transform.position + pooledProjectile.CurrentHeading) - pooledProjectile.transform.position;

            if (abilityBase.abilityShotType == AbilityShotType.PROJECTILE_NOVA)
            {
                pooledProjectile.SetHeading(Quaternion.Euler(0, 0, spreadAngle * i) * heading);
                pooledProjectile.sharedHitList = sharedList;
            }
            else
            {
                if (isSpread)
                {
                    if (i == 0)
                        pooledProjectile.currentTarget = target;
                    int angleMultiplier = (int)Math.Round(i / 2f, MidpointRounding.AwayFromZero);
                    if (i % 2 == 0)
                    {
                        angleMultiplier *= -1;
                    }

                    pooledProjectile.SetHeading(Quaternion.Euler(0, 0, spreadAngle * angleMultiplier) * heading);
                    pooledProjectile.sharedHitList = sharedList;
                }
                else
                {
                    pooledProjectile.SetHeading(Quaternion.Euler(0, 0, spreadAngle * UnityEngine.Random.Range(-1f, 1f)) * heading);
                }
            }

            pooledProjectile.transform.localScale = new Vector2(ProjectileSize, ProjectileSize);
            pooledProjectile.pierceCount = ProjectilePierce;
            pooledProjectile.chainCount = ProjectileChain;
            pooledProjectile.homingRate = ProjectileHoming;

            if (abilityBase.hasLinkedAbility)
                pooledProjectile.linkedAbility = LinkedAbility;

            pooledProjectile.timeToLive = 4f * abilityBase.projectileLifespanMulti;
            pooledProjectile.currentSpeed = ProjectileSpeed;
            pooledProjectile.onHitData = abilityOnHitData;
            pooledProjectile.gameObject.layer = targetLayer;
            pooledProjectile.layerMask = targetMask;

            SetProjectileEffects(pooledProjectile, ProjectileSize);

            if (!isSpread)
                yield return new WaitForSeconds(abilityBase.hitscanDelay);
        }

        currentShotCoroutine = null;

        yield break;
    }

    private void SetProjectileEffects(Projectile pooledProjectile, float scaleFactor)
    {
        AbilityParticleSystem particleSystem = ParticleManager.Instance.GetParticleSystem(abilityBase.idName);
        if (particleSystem != null)
        {
            GameObject particle = GameObject.Instantiate(particleSystem.gameObject, pooledProjectile.transform, false);
            particle.transform.localPosition = Vector3.zero;
            pooledProjectile.particles = particle.GetComponent<ParticleSystem>();
            pooledProjectile.particles.transform.localScale = new Vector2(scaleFactor, scaleFactor);
            pooledProjectile.particles.Play();
        }

        pooledProjectile.GetComponent<SpriteRenderer>().sprite = ResourceManager.Instance.GetAbilitySprite(abilityBase.idName);
    }

    protected void FireMovingAoe(Vector3 origin, Vector3 target)
    {
        Vector3 heading = (target - origin).normalized;
        heading.z = 0;

        Projectile pooledProjectile;

        if (abilityBase.abilityShotType == AbilityShotType.FORWARD_MOVING_LINEAR)
        {
            pooledProjectile = StageManager.Instance.BattleManager.BoxProjectilePool.GetProjectile();
            BoxCollider2D collider = pooledProjectile.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(AreaRadius * 2f, 0.75f);
            collider.offset = new Vector2(0, collider.size.y / 2f);
        }
        else
        {
            pooledProjectile = StageManager.Instance.BattleManager.ProjectilePool.GetProjectile();
        }

        pooledProjectile.pierceCount = 999;

        pooledProjectile.SetHeading(heading);
        pooledProjectile.transform.position = origin;
        pooledProjectile.transform.up = (pooledProjectile.transform.position + pooledProjectile.CurrentHeading) - pooledProjectile.transform.position;

        pooledProjectile.timeToLive = abilityBase.projectileLifespanMulti;
        pooledProjectile.currentSpeed = AreaLength / abilityBase.projectileLifespanMulti;

        if (abilityBase.hasLinkedAbility)
            pooledProjectile.linkedAbility = LinkedAbility;

        pooledProjectile.onHitData = abilityOnHitData;
        pooledProjectile.gameObject.layer = targetLayer;
        pooledProjectile.layerMask = targetMask;

        SetProjectileEffects(pooledProjectile, AreaScaling);
    }

    protected void FireLinearAoe(Vector3 origin, Vector3 target)
    {
        Vector3 heading = (target - origin).normalized;
        Vector3 horizontal = Vector3.Cross(heading, Vector3.forward).normalized * AreaRadius;

        /*
        Debug.DrawLine(origin, origin + horizontal, Color.red, 1);
        Debug.DrawLine(origin, origin - horizontal, Color.red, 1);
        Debug.DrawLine(origin - horizontal, origin - horizontal + (heading * AreaLength), Color.red, 1);
        Debug.DrawLine(origin + horizontal, origin + horizontal + (heading * AreaLength), Color.red, 1);
        */

        emitParams.position = origin;
        float angle = Vector2.SignedAngle(Vector2.up, heading);
        ParticleManager.Instance.EmitAbilityParticle_Rotated(abilityBase.idName, emitParams, AreaScaling, angle, AbilityOwner.transform);

        //Collider2D[] hits = Physics2D.OverlapAreaAll(origin + horizontal, origin - horizontal + (heading * AreaLength), targetMask);
        RaycastHit2D[] hitcast = Physics2D.BoxCastAll(origin, new Vector2(AreaRadius * 2f, AreaRadius * 2f), angle, heading, AreaLength, targetMask);

        foreach (RaycastHit2D hit in hitcast)
        {
            Actor actor = hit.transform.GetComponent<Actor>();
            if (actor == null)
                continue;

            ApplyDamageToActor(actor, true);
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
        FireMovingAoe(abilityCollider.transform.position, CurrentTarget.transform.position);
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

            float threshold = 0.05f;

            //Grab next movement steps for enemy then calculate enemy move time to
            //that node. If travel time of projectile and enemy of node are within
            //error then shoot toward that node.
            //Only calculate few nodes ahead for performance and game balance.
            currentShotCoroutine = AbilityOwner.StartCoroutine(FireProjectile(abilityCollider.transform.position, CurrentTarget));
            return;

            for (int i = 0; i < 9; i++)
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
                        FireProjectile(abilityCollider.transform.position, CurrentTarget, (Vector3)nextNode);
                        return;
                    }
                }
                else
                {
                    break;
                }
            }
            FireProjectile(abilityCollider.transform.position, CurrentTarget);
        }
        else
        {
            currentShotCoroutine = AbilityOwner.StartCoroutine(FireProjectile(abilityCollider.transform.position, CurrentTarget));
        }
    }

    public string GetAuraBuffString()
    {
        string s = "";
        int i = 0;
        List<int> bonusesToSkip = new List<int>();

        foreach (var bonus in auraBuffBonus.cachedAuraBonuses)
        {
            if (bonusesToSkip.Contains(i))
            {
                i++;
                continue;
            }

            string bonusString = bonus.bonusType.ToString();
            if (bonusString.Contains("DAMAGE_MIN") && bonus.modifyType == ModifyType.FLAT_ADDITION)
            {
                BonusType maxType = (BonusType)Enum.Parse(typeof(BonusType), bonusString.Replace("_MIN", "_MAX"));
                int matchedIndex = auraBuffBonus.cachedAuraBonuses.FindIndex(x => x.bonusType == maxType);

                if (matchedIndex > 0 && auraBuffBonus.cachedAuraBonuses[matchedIndex].modifyType == ModifyType.FLAT_ADDITION)
                {
                    bonusesToSkip.Add(matchedIndex);

                    s += "○ " + LocalizationManager.Instance.GetLocalizationText("bonusType." + bonusString.Replace("_MIN", "")) + " ";
                    s += "<nobr>+" + auraBuffBonus.cachedAuraBonuses[i].effectValue + "~" + auraBuffBonus.cachedAuraBonuses[matchedIndex].effectValue + "</nobr>\n";
                }
            }
            else
            {
                s += "○ " + LocalizationManager.Instance.GetLocalizationText_BonusType(bonus.bonusType, bonus.modifyType, (float)Math.Round(bonus.effectValue, 3), GroupType.NO_GROUP, bonus.effectDuration);
            }

            i++;
        }
        return s;
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

        criticalChance = Math.Min(criticalChance, 1f);

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

    public bool ApplyDamageToActor(Actor target, bool isHit, float damageModifier = 1f)
    {
        if (target == null || target.Data.IsDead)
        {
            return false;
        }

        damageModifier = 1f;
        if (isHit)
        {
            if (Actor.DidTargetPhase(target, abilityBase.abilityType))
            {
                target.Data.OnHitData.ApplyTriggerEffects(TriggerType.ON_PHASING, AbilityOwner);
                return false;
            }

            if (Actor.DidTargetDodge(target, abilityOnHitData.accuracy))
            {
                target.Data.OnHitData.ApplyTriggerEffects(TriggerType.ON_DODGE, AbilityOwner);
                return false;
            }

            if (Actor.DidTargetParry(target, abilityBase.abilityType))
            {
                target.Data.OnHitData.ApplyTriggerEffects(TriggerType.ON_PARRY, AbilityOwner);
                return false;
            }
        }

        Dictionary<ElementType, float> damageDict = CalculateDamageValues(target, AbilityOwner, damageModifier);

        if (abilityBase.hitCount > 1)
        {
            AbilityOwner.StartCoroutine(ApplyMultiHitDamage(target, damageDict, abilityOnHitData, isHit));
        }
        else
        {
            target.ApplyDamage(damageDict, abilityOnHitData, isHit, false);
        }
        return true;
    }

    private IEnumerator ApplyMultiHitDamage(Actor target, Dictionary<ElementType, float> damage, AbilityOnHitDataContainer onHitDataContainer, bool isHit)
    {
        float delayBetweenHits = abilityBase.delayBetweenHits;
        for (int i = 0; i < abilityBase.hitCount; i++)
        {
            if (target.Data.IsDead || !target.isActiveAndEnabled)
                yield break;
            target.ApplyDamage(damage, onHitDataContainer, isHit, false);
            yield return new WaitForSeconds(delayBetweenHits);
        }
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