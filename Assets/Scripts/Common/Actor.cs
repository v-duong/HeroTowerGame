using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
    public const float BASE_SHIELD_RESTORE_DELAY = 4.0f;
    private const float SHIELD_RECHARGE_PROTECTION_TIME = -5.0f;

    public ActorData Data { get; protected set; }
    public float actorTimeScale = 1f;
    private readonly List<ActorEffect> statusEffects = new List<ActorEffect>();
    private readonly List<SourcedActorEffect> buffEffects = new List<SourcedActorEffect>();
    private readonly List<TemporaryTriggerEffectBuff> temporaryTriggerEffectBuffs = new List<TemporaryTriggerEffectBuff>();
    protected UIHealthBar healthBar;
    protected List<ActorAbility> instancedAbilitiesList = new List<ActorAbility>();
    public int NextMovementNode { get; protected set; }
    public bool IsMoving { get; protected set; }
    public bool isBoss = false;
    public int attackLocks = 0;
    public HashSet<GroupType> actorTags = new HashSet<GroupType>();
    public HashSet<GroupType> actorTargetTags = new HashSet<GroupType>();
    public PrimaryTargetingType targetingPriority = PrimaryTargetingType.CLOSEST;
    public SecondaryTargetingFlags targetingFlags = SecondaryTargetingFlags.NONE;
    public Actor forcedTarget = null;

    public abstract ActorType GetActorType();

    public abstract void Death();

    protected abstract void Move();

    public void UpdateStatusEffects()
    {
        float dT = Time.deltaTime;
        int index = statusEffects.Count - 1;
        bool needsUpdate = false;

        while (index >= 0)
        {
            statusEffects[index].Update(dT);
            if (statusEffects[index].duration <= 0)
            {
                RemoveStatusEffect(statusEffects[index], true);
                needsUpdate = true;
            }
            index--;
        }

        index = buffEffects.Count - 1;
        while (index >= 0)
        {
            buffEffects[index].Update(dT);
            if (buffEffects[index].duration <= 0)
            {
                RemoveStatusEffect(buffEffects[index], true);
                needsUpdate = true;
            }
            index--;
        }

        if (needsUpdate)
            Data.UpdateActorData();
    }

    protected void Start()
    {
        healthBar = GetComponentInChildren<UIHealthBar>();
        InitializeHealthBar();
    }

    protected virtual void Update()
    {
        if (StageManager.Instance.BattleManager.startedSpawn)
        {
            UpdateStatusEffects();
            ApplyRegenEffects();
        }

        if (Data.IsDead)
            Death();

        if (!gameObject.activeSelf)
            return;
        if (IsMoving)
        {
            Move();
        }
        healthBar.UpdatePosition(transform);
    }

    public HashSet<GroupType> GetActorTags()
    {
        return actorTags;
    }

    public HashSet<GroupType> GetActorTagsAndDataTags()
    {
        HashSet<GroupType> returnSet = new HashSet<GroupType>(GetActorTags());
        returnSet.UnionWith(Data.GroupTypes);
        return returnSet;
    }

    public HashSet<GroupType> GetActorTagsAsTarget()
    {
        UpdateTargetTags();
        return actorTargetTags;
    }

    private void UpdateTargetTags()
    {
        actorTargetTags.Clear();
        foreach (GroupType g in actorTags)
        {
            if (g.ToString().StartsWith("SELF_IS"))
            {
                string s = g.ToString().Replace("SELF_IS", "TARGET");
                GroupType newTag = (GroupType)Enum.Parse(typeof(GroupType), s);
                actorTargetTags.Add(newTag);
            }
        }
    }

    public void InitializeHealthBar()
    {
        if (healthBar != null)
            healthBar.InitializeForActor(Data.MaximumHealth, Data.CurrentHealth, Data.MaximumManaShield, Data.CurrentManaShield, transform);
    }

    public void AddStatusEffect(ActorEffect statusEffect)
    {
        if (statusEffect.effectType == EffectType.BUFF || statusEffect.effectType == EffectType.DEBUFF)
        {
            if (statusEffect is SourcedActorEffect namedEffect)
            {
                buffEffects.Add(namedEffect);
                statusEffect.OnApply();
                Data.UpdateActorData();
            }
            else
            {
                Debug.Log("Should not have buff type.");
            }
        }
        else
        {
            List<ActorEffect> existingStatus = statusEffects.FindAll(x => x.GetType() == statusEffect.GetType());

            if (statusEffect.Source.GetActorType() != this.GetActorType())
                statusEffect.duration *= Data.AfflictedStatusDuration;

            int stackCount;
            if (existingStatus.Count > 0 && statusEffect.StacksIncrementExistingEffect)
            {
                stackCount = existingStatus[0].Stacks;
            }
            else
                stackCount = existingStatus.Count;

            if (statusEffect.StacksIncrementExistingEffect && stackCount > 0)
            {
                if (stackCount < statusEffect.MaxStacks)
                    existingStatus[0].SetStacks(existingStatus[0].Stacks + 1);
                return;
            }
            else
            {
                if (stackCount == statusEffect.MaxStacks)
                {
                    ActorEffect effectToRemove = existingStatus.Find(x => x.GetSimpleEffectValue() < statusEffect.GetSimpleEffectValue());
                    if (effectToRemove != null)
                        RemoveStatusEffect(effectToRemove, true);
                    else
                    {
                        existingStatus.OrderBy(x => x.duration).First().RefreshDuration(statusEffect.duration);
                        return;
                    }
                }
                else if (stackCount > statusEffect.MaxStacks)
                    return;
            }

            statusEffects.Add(statusEffect);
            //Debug.Log(Data.Name + " " + statusEffect + " " + statusEffect.GetSimpleEffectValue());
            statusEffect.OnApply();
            actorTags.Add(statusEffect.StatusTag);
            Data.UpdateActorData();
        }
    }

    public void RemoveStatusEffect(ActorEffect statusEffect, bool deferUpdates)
    {
        statusEffect.OnExpire();

        if (statusEffect.effectType == EffectType.BUFF || statusEffect.effectType == EffectType.DEBUFF)
            buffEffects.Remove((StatBonusBuffEffect)statusEffect);
        else
        {
            statusEffects.Remove(statusEffect);

            if (GetStatusEffect(statusEffect.effectType) == null)
                actorTags.Remove(statusEffect.StatusTag);
        }

        if (!deferUpdates)
            Data.UpdateActorData();
    }

    public ActorEffect GetStatusEffect(EffectType effect)
    {
        List<ActorEffect> actorEffects = statusEffects.FindAll(x => x.effectType == effect);
        if (actorEffects.Count == 0)
            return null;
        else if (actorEffects.Count == 1)
            return actorEffects[0];
        else
        {
            actorEffects.OrderBy(x => x.GetSimpleEffectValue());
            return actorEffects[0];
        }
    }

    public List<ActorEffect> GetStatusEffectAll(EffectType effect)
    {
        return statusEffects.FindAll(x => x.effectType == effect);
    }

    public List<ActorEffect> GetStatusEffectAll(params EffectType[] effects)
    {
        return statusEffects.FindAll(x => effects.Contains(x.effectType));
    }

    public void ClearStatusEffects(bool useExpireEffects)
    {
        if (useExpireEffects)
        {
            foreach (ActorEffect effect in statusEffects)
            {
                effect.OnExpire();
            }

            foreach (ActorEffect effect in buffEffects)
            {
                effect.OnExpire();
            }
        }

        statusEffects.Clear();
        buffEffects.Clear();
    }

    public List<SourcedActorEffect> GetBuffStatusEffect(string statusName)
    {
        List<SourcedActorEffect> buffs = buffEffects.FindAll(x => x.EffectName.Equals(statusName));
        return buffs;
    }

    public void AddAbilityToList(ActorAbility ability)
    {
        instancedAbilitiesList.Add(ability);
        ability.SetAbilityOwner(this);

        GameObject newObject = Instantiate(ResourceManager.Instance.AbilityContainerPrefab, transform);
        AbilityColliderContainer abilityContainer = newObject.GetComponent<AbilityColliderContainer>();
        abilityContainer.ability = ability;
        abilityContainer.parentActor = this;
        abilityContainer.transform.position = transform.position;
        ability.abilityCollider = abilityContainer;

        var collider = newObject.AddComponent<CircleCollider2D>();
        collider.radius = ability.TargetRange;
        abilityContainer.abilityCollider = collider;
        collider.isTrigger = true;

        collider.gameObject.layer = ability.targetLayer;
    }

    public List<AbilityBase> GetAbilitiyBasesInList()
    {
        List<AbilityBase> ret = new List<AbilityBase>();
        foreach (ActorAbility ability in instancedAbilitiesList)
        {
            ret.Add(ability.abilityBase);
        }
        return ret;
    }

    public IList<ActorAbility> GetInstancedAbilities()
    {
        return instancedAbilitiesList.AsReadOnly();
    }

    public void ModifyCurrentHealth(float mod)
    {
        if (mod == 0)
            return;

        if (Data.CurrentHealth - mod > Data.MaximumHealth)
            Data.CurrentHealth = Data.MaximumHealth;
        else
            Data.CurrentHealth -= mod;

        if (healthBar != null)
            healthBar.UpdateHealthBar(Data.MaximumHealth, Data.CurrentHealth, Data.MaximumManaShield, Data.CurrentManaShield, true);
    }

    public void ModifyCurrentSoulpoints(float mod)
    {
        if (mod == 0)
            return;

        if (Data.CurrentSoulPoints - mod > Data.MaximumSoulPoints)
            Data.CurrentSoulPoints = Data.MaximumSoulPoints;
        else
            Data.CurrentSoulPoints -= mod;
    }

    public float ModifyCurrentShield(float mod, bool willModInterruptRecharge)
    {
        float remainingDamage;

        if (mod == 0)
            return 0;

        if (Data.MaximumManaShield == 0 || mod > 0 && Data.CurrentManaShield == 0)
            return mod;

        if (Data.CurrentManaShield - mod > Data.MaximumManaShield)
        {
            Data.CurrentManaShield = Data.MaximumManaShield;
            remainingDamage = 0;
        }
        else if (mod > Data.CurrentManaShield)
        {
            remainingDamage = mod - Data.CurrentManaShield;
            Data.CurrentManaShield = 0;
        }
        else
        {
            Data.CurrentManaShield -= mod;
            remainingDamage = 0;
        }

        if (mod > 0 && willModInterruptRecharge && !Data.RechargeCannotBeStopped)
        {
            Data.CurrentShieldDelay = Math.Max(BASE_SHIELD_RESTORE_DELAY * Data.ShieldRestoreDelayModifier, 1f);
        }

        if (healthBar != null)
            healthBar.UpdateHealthBar(Data.MaximumHealth, Data.CurrentHealth, Data.MaximumManaShield, Data.CurrentManaShield, true);

        return remainingDamage;
    }

    public void ApplyRegenEffects()
    {
        if (Data.MaximumManaShield > 0)
        {
            float previousShieldDelay = Data.CurrentShieldDelay;
            Data.CurrentShieldDelay -= Time.deltaTime;

            if (Data.HasSpecialBonus(BonusType.SHIELD_RESTORE_CANNOT_BE_INTERRUPTED) && previousShieldDelay > 0f && Data.CurrentShieldDelay <= 0f)
                Data.RechargeCannotBeStopped = true;
            else if (Data.RechargeCannotBeStopped && Data.CurrentShieldDelay <= SHIELD_RECHARGE_PROTECTION_TIME)
                Data.RechargeCannotBeStopped = false;

            float shieldModifier = -Data.ShieldRegenRate;
            if (Data.CurrentShieldDelay <= 0f && Data.CurrentManaShield < Data.MaximumManaShield)
                shieldModifier += -Data.ShieldRestoreRate;
            ModifyCurrentShield(shieldModifier * Time.deltaTime, false);
        } else if (Data.MaximumManaShield == 0)
        {
            if (Data.CurrentManaShield > 0)
            {
                Data.CurrentManaShield = 0;
                if (healthBar != null)
                    healthBar.UpdateHealthBar(Data.MaximumHealth, Data.CurrentHealth, Data.MaximumManaShield, Data.CurrentManaShield, true);
            }
        }
        ModifyCurrentHealth(-Data.HealthRegenRate * Time.deltaTime);
        ModifyCurrentSoulpoints(-Data.SoulRegenRate * Time.deltaTime);
    }

    public static bool DidTargetBlock(Actor target)
    {
        if (target.Data.BlockChance > 0)
        {
            if (target.Data.BlockChance == 1f || UnityEngine.Random.Range(0, 1f) < target.Data.BlockChance)
                return true;
        }
        return false;
    }

    public static bool DidTargetDodge(Actor target, float accuracy)
    {
        if (target.Data.DodgeRating > 0)
        {
            float dodgePercent = 1f - (accuracy / (accuracy + target.Data.DodgeRating / 2f));
            dodgePercent = Mathf.Min(dodgePercent, 0.85f);

            if (UnityEngine.Random.Range(0, 1f) < dodgePercent)
                return true;
        }

        return false;
    }

    public static bool DidTargetParry(Actor target, AbilityType abilityType)
    {
        if (target.Data.AttackParryChance > 0 && abilityType == AbilityType.ATTACK)
        {
            if (target.Data.AttackParryChance == 100 || UnityEngine.Random.Range(0, 100) < target.Data.AttackParryChance)
                return true;
        }
        else if (target.Data.SpellParryChance > 0 && abilityType == AbilityType.SPELL)
        {
            if (target.Data.SpellParryChance == 100 || UnityEngine.Random.Range(0, 100) < target.Data.SpellParryChance)
                return true;
        }

        return false;
    }

    public static bool DidTargetPhase(Actor target, AbilityType abilityType)
    {
        if (target.Data.AttackPhasing > 0 && abilityType == AbilityType.ATTACK)
        {
            if (target.Data.AttackPhasing == 100 || UnityEngine.Random.Range(0, 100) < target.Data.AttackPhasing)
                return true;
        }
        else if (target.Data.SpellPhasing > 0 && abilityType == AbilityType.SPELL)
        {
            if (target.Data.SpellPhasing == 100 || UnityEngine.Random.Range(0, 100) < target.Data.SpellPhasing)
                return true;
        }

        return false;
    }

    public void ApplyDamage(Dictionary<ElementType, float> damage, OnHitDataContainer onHitData, bool isHit,
                            bool isFromSecondaryEffect, EffectType? sourceType = null,
                            EffectApplicationFlags restrictionFlags = EffectApplicationFlags.NONE)
    {
        float total = 0;

        if (Data.IsDead)
            return;

        if (Data.IsDead && isHit)
            return;

        Dictionary<ElementType, float> damageTaken = new Dictionary<ElementType, float>(damage);

        float damageTakenModifier = Data.DamageTakenModifier;
        float blockDamageReduction = 1f;

        if (isHit)
        {
            if (!isFromSecondaryEffect)
                damageTakenModifier *= onHitData.directHitDamage;

            if (DidTargetBlock(this))
            {
                blockDamageReduction *= 1f - Data.BlockProtection;
                damageTakenModifier *= blockDamageReduction;
                Data.OnHitData.ApplyTriggerEffects(TriggerType.ON_BLOCK, onHitData.SourceActor);
            }

            //if (isBoss)
            //damageTakenModifier *= onHitData.vsBossDamage;
        }

        foreach (ElementType elementType in damage.Keys)
        {
            bool isDamageTakenNonZero = false;
            if (elementType == ElementType.PHYSICAL && damage.ContainsKey(ElementType.PHYSICAL))
            {
                float armorReductionRate = 1.0f;
                if (isHit && Data.Armor > 0)
                {
                    armorReductionRate = 1.0f - (Data.Armor / (Data.Armor + damage[ElementType.PHYSICAL]));
                }
                float resistanceReductionRate = 1.0f - (Data.GetResistance(ElementType.PHYSICAL) / 100f);
                float physicalReduction = (armorReductionRate * resistanceReductionRate) + (onHitData.GetNegation(ElementType.PHYSICAL) / 100f);
                //Debug.Log(Data.GetResistance(ElementType.PHYSICAL) + " " + onHitData.physicalNegation + " " + damage[ElementType.PHYSICAL]);
                float physicalDamage = physicalReduction * damage[ElementType.PHYSICAL] * damageTakenModifier;
                total += Math.Max(0, physicalDamage);
                damageTaken[ElementType.PHYSICAL] = (damage[ElementType.PHYSICAL] * blockDamageReduction);
                isDamageTakenNonZero = physicalDamage > 0;
            }
            else
            {
                if (damage.ContainsKey(elementType))
                {
                    float nonPhysicalDamage = (1f - (Data.GetResistance(elementType) - onHitData.GetNegation(elementType)) / 100f) * damage[elementType] * damageTakenModifier;
                    total += Math.Max(0, nonPhysicalDamage);
                    damageTaken[elementType] = (damage[elementType] * blockDamageReduction);
                    isDamageTakenNonZero = nonPhysicalDamage > 0;
                }
            }

            if (isHit && isDamageTakenNonZero)
                ParticleManager.Instance.EmitOnHitEffect(elementType, transform.position);
        }

        float actualDamageTaken = total;

        if (sourceType == EffectType.POISON)
            total = ModifyCurrentShield(total * Data.PoisonResistance, true) + (total * (1f - Data.PoisonResistance));
        else
            total = ModifyCurrentShield(total, true);

        actualDamageTaken -= total;

        if (Data.HealthIsHitsToKill && isHit && total >= 1f)
        {
            ModifyCurrentHealth(1);

            actualDamageTaken += 1;
        }
        else if (total > 0)
        {
            MassShieldAura massShield = (MassShieldAura)GetStatusEffect(EffectType.MASS_SHIELD_AURA);
            if (massShield != null && massShield.Source != this)
            {
                total = massShield.TransferDamage(total * massShield.DamageTransferRate) + total * (1f - massShield.DamageTransferRate);
            }

            BodyguardAura bodyguard = (BodyguardAura)GetStatusEffect(EffectType.BODYGUARD_AURA);
            if (isHit && bodyguard != null && bodyguard.Source != this)
            {
                if (bodyguard.TransferDamage(total, out float damageMod))
                {
                    total *= 1f - bodyguard.DamageTransferRate;
                    foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
                    {
                        if (damage.ContainsKey(element))
                            damage[element] *= damageMod;
                    }
                    bodyguard.Source.ApplyAfterHitEffects(damage, onHitData, restrictionFlags);
                }
            }

            actualDamageTaken += total;

            ModifyCurrentHealth(total);
        }

        if (onHitData.SourceActor != this)
        {
            if (Data.IsDead)
            {
                if (onHitData.SourceActor is HeroActor hero)
                    hero.Data.killCount++;

                onHitData.ApplyTriggerEffects(TriggerType.ON_KILL, this);

                if (isHit)
                    onHitData.ApplyTriggerEffects(TriggerType.ON_HIT_KILL, this);
            }
            else if (isHit)
                Data.OnHitData.ApplyTriggerEffects(TriggerType.WHEN_HIT_BY, onHitData.SourceActor);
        }

        if ((GameManager.Instance.PlayerStats.showDamageNumbers) && (isHit || sourceType == EffectType.RETALIATION_DAMAGE))
        {
            FloatingDamageText damageText = StageManager.Instance.BattleManager.DamageTextPool.GetDamageText();
            damageText.transform.SetParent(StageManager.Instance.WorldCanvas.transform, false);
            damageText.transform.position = this.transform.position;


            if (sourceType == EffectType.RETALIATION_DAMAGE)
                damageText.SetDamageText(actualDamageTaken, Color.gray);
            else
                damageText.SetDamageText(actualDamageTaken, Color.white);
        }

        if (isHit)
        {
            ApplyAfterHitEffects(damageTaken, onHitData, restrictionFlags);
        }
    }

    public void ApplyAfterHitEffects(Dictionary<ElementType, float> damage, OnHitDataContainer onHitData, EffectApplicationFlags restrictionFlags)
    {
        onHitData.ApplyTriggerEffects(TriggerType.ON_HIT, this);

        float HealthForThreshold = Data.MaximumHealth * Data.AfflictedStatusThreshold;
        if (Data.HasSpecialBonus(BonusType.USE_SHIELD_FOR_STATUS_THRESHOLD))
        {
            HealthForThreshold = Data.MaximumManaShield * Data.AfflictedStatusThreshold;
        }

        if (damage.ContainsKey(ElementType.PHYSICAL) && damage[ElementType.PHYSICAL] > 0)
        {
            if (!restrictionFlags.HasFlag(EffectApplicationFlags.CANNOT_BLEED)
                && onHitData.DidEffectProc(EffectType.BLEED, Data.AfflictedStatusAvoidance))
            {
                onHitData.ApplyEffectToTarget(this, EffectType.BLEED, damage[ElementType.PHYSICAL] * onHitData.effectData[EffectType.BLEED].Effectiveness * Data.AfflictedStatusDamageResistance * BleedEffect.BASE_DAMAGE_MULTIPLIER);
            }
            if (!restrictionFlags.HasFlag(EffectApplicationFlags.CANNOT_POISON)
                && onHitData.DidEffectProc(EffectType.POISON, Data.AfflictedStatusAvoidance))
            {
                onHitData.ApplyEffectToTarget(this, EffectType.POISON, damage[ElementType.PHYSICAL] * onHitData.effectData[EffectType.POISON].Effectiveness * Data.AfflictedStatusDamageResistance * PoisonEffect.BASE_DAMAGE_MULTIPLIER);
            }
        }

        if (!restrictionFlags.HasFlag(EffectApplicationFlags.CANNOT_BURN)
            && damage.ContainsKey(ElementType.FIRE)
            && damage[ElementType.FIRE] > 0
            && onHitData.DidEffectProc(EffectType.BURN, Data.AfflictedStatusAvoidance))
        {
            //float percent = (damage[ElementType.FIRE] * onHitData.GetEffectEffectiveness(EffectType.BURN) * Data.AfflictedStatusDamageResistance) / damage[ElementType.FIRE];
            //Debug.Log(damage[ElementType.FIRE] + " BURN  " + damage[ElementType.FIRE] * onHitData.GetEffectEffectiveness(EffectType.BURN) * Data.AfflictedStatusDamageResistance + "   " + percent.ToString("F3"));
            onHitData.ApplyEffectToTarget(this, EffectType.BURN, damage[ElementType.FIRE] * onHitData.effectData[EffectType.BURN].Effectiveness * Data.AfflictedStatusDamageResistance * BurnEffect.BASE_DAMAGE_MULTIPLIER);
        }

        if (!restrictionFlags.HasFlag(EffectApplicationFlags.CANNOT_CHILL)
            && damage.ContainsKey(ElementType.COLD)
            && damage[ElementType.COLD] > 0
            && onHitData.DidEffectProc(EffectType.CHILL, Data.AfflictedStatusAvoidance))
        {
            float percentageDealt = damage[ElementType.COLD] / HealthForThreshold;

            float chillEffectPower = ChillEffect.BASE_CHILL_EFFECT * Math.Min(percentageDealt / ChillEffect.BASE_CHILL_THRESHOLD, 1f) * onHitData.effectData[EffectType.CHILL].Effectiveness;
            //Debug.Log(damage[ElementType.COLD] + " " + percentageDealt + " " + chillEffectPower);
            onHitData.ApplyEffectToTarget(this, EffectType.CHILL, chillEffectPower);
        }

        if (!restrictionFlags.HasFlag(EffectApplicationFlags.CANNOT_ELECTROCUTE)
            && damage.ContainsKey(ElementType.LIGHTNING)
            && damage[ElementType.LIGHTNING] > 0
            && onHitData.DidEffectProc(EffectType.ELECTROCUTE, Data.AfflictedStatusAvoidance))
        {
            onHitData.ApplyEffectToTarget(this, EffectType.ELECTROCUTE, damage[ElementType.LIGHTNING] * onHitData.effectData[EffectType.ELECTROCUTE].Effectiveness * Data.AfflictedStatusDamageResistance * ElectrocuteEffect.BASE_DAMAGE_MULTIPLIER);
        }

        if (!restrictionFlags.HasFlag(EffectApplicationFlags.CANNOT_FRACTURE)
            && damage.ContainsKey(ElementType.EARTH)
            && damage[ElementType.EARTH] > 0
            && onHitData.DidEffectProc(EffectType.FRACTURE, Data.AfflictedStatusAvoidance))
        {
            float percentageDealt = damage[ElementType.EARTH] / HealthForThreshold;
            float fractureEffectPower = FractureEffect.BASE_FRACTURE_EFFECT * Math.Min(percentageDealt / FractureEffect.BASE_FRACTURE_THRESHOLD, 1f) * onHitData.effectData[EffectType.FRACTURE].Effectiveness;

            onHitData.ApplyEffectToTarget(this, EffectType.FRACTURE, fractureEffectPower);
        }

        if (!restrictionFlags.HasFlag(EffectApplicationFlags.CANNOT_PACIFY)
            && damage.ContainsKey(ElementType.DIVINE)
            && damage[ElementType.DIVINE] > 0
            && onHitData.DidEffectProc(EffectType.PACIFY, Data.AfflictedStatusAvoidance))
        {
            float percentageDealt = damage[ElementType.DIVINE] / HealthForThreshold;
            float pacifyEffectPower = PacifyEffect.BASE_PACIFY_EFFECT * Math.Min(percentageDealt / PacifyEffect.BASE_PACIFY_THRESHOLD, 1f) * onHitData.effectData[EffectType.PACIFY].Effectiveness;

            onHitData.ApplyEffectToTarget(this, EffectType.PACIFY, pacifyEffectPower);
        }

        if (!restrictionFlags.HasFlag(EffectApplicationFlags.CANNOT_RADIATION)
            && damage.ContainsKey(ElementType.VOID)
            && damage[ElementType.VOID] > 0
            && onHitData.DidEffectProc(EffectType.RADIATION, Data.AfflictedStatusAvoidance))
        {
            onHitData.ApplyEffectToTarget(this, EffectType.RADIATION, damage[ElementType.VOID] * onHitData.effectData[EffectType.RADIATION].Effectiveness * Data.AfflictedStatusDamageResistance * RadiationEffect.BASE_DAMAGE_MULTIPLIER);
        }
    }

    public void ApplySingleElementDamage(ElementType element, float damage, OnHitDataContainer onHitData, bool isHit, bool isFromSecondaryEffect, EffectType? sourceType = null)
    {
        ApplyDamage(new Dictionary<ElementType, float> {
            { element, damage }
        }, onHitData, isHit, isFromSecondaryEffect, sourceType);
    }

    public void DisableActor()
    {
        foreach (ActorAbility x in instancedAbilitiesList)
        {
            x.StopFiring(this);
        }

        gameObject.SetActive(false);
        healthBar.gameObject.SetActive(false);
    }

    public void EnableHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(true);
            healthBar.UpdatePosition(transform);
            healthBar.UpdateHealthBar(Data.MaximumHealth, Data.CurrentHealth, Data.MaximumManaShield, Data.CurrentManaShield, true);
        }
    }

    public Dictionary<ElementType, float> ScaleSecondaryDamageValue(Actor target, Dictionary<ElementType, MinMaxRange> baseDamage, IEnumerable<GroupType> effectTags)
    {
        HashSet<GroupType> targetTypes = target.GetActorTagsAsTarget();
        Dictionary<BonusType, StatBonus> whenHitBonusDict = new Dictionary<BonusType, StatBonus>();
        int[] minDamage = new int[7];
        int[] maxDamage = new int[7];
        int[] convertedMinDamage = new int[7];
        int[] convertedMaxDamage = new int[7];
        float[] conversions = new float[7];
        HashSet<GroupType> tagsToUse = new HashSet<GroupType>(effectTags);
        tagsToUse.UnionWith(GetActorTagsAndDataTags());

        foreach (TriggeredEffect triggeredEffect in Data.TriggeredEffects[TriggerType.WHEN_HITTING])
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

        foreach (ElementType elementType in Enum.GetValues(typeof(ElementType)))
        {
            if (!baseDamage.ContainsKey(elementType))
            {
                baseDamage[elementType] = new MinMaxRange();
            }
            minDamage[(int)elementType] = baseDamage[elementType].min;
            maxDamage[(int)elementType] = baseDamage[elementType].max;

            HashSet<BonusType> minTypes = new HashSet<BonusType>();
            HashSet<BonusType> maxTypes = new HashSet<BonusType>();
            HashSet<BonusType> multiTypes = new HashSet<BonusType>();

            Helpers.GetGlobalAndFlatDamageTypes(elementType, tagsToUse, minTypes, maxTypes, multiTypes);
            multiTypes.UnionWith(Helpers.GetMultiplierTypes(AbilityType.NON_ABILITY, elementType));

            StatBonus minBonus = Data.GetMultiStatBonus(tagsToUse, minTypes.ToArray());
            StatBonus maxBonus = Data.GetMultiStatBonus(tagsToUse, maxTypes.ToArray());
            StatBonus multiBonus = new StatBonus();

            foreach (KeyValuePair<BonusType, StatBonus> keyValue in whenHitBonusDict)
            {
                if (minTypes.Contains(keyValue.Key))
                    minBonus.AddBonuses(keyValue.Value);
                else if (maxTypes.Contains(keyValue.Key))
                    maxBonus.AddBonuses(keyValue.Value);
                else if (multiTypes.Contains(keyValue.Key))
                    multiBonus.AddBonuses(keyValue.Value);
            }

            HashSet<BonusType> availableConversions = Data.BonusesIntersection(null, Helpers.GetConversionTypes(elementType));
            if (availableConversions.Count > 0)
            {
                Array.Clear(conversions, 0, 7);
                ActorAbility.GetElementConversionValues(Data, tagsToUse, availableConversions, conversions, null);
                MinMaxRange baseRange = ActorAbility.CalculateDamageConversion(Data, convertedMinDamage, convertedMaxDamage, tagsToUse, baseDamage[elementType], conversions, elementType, multiTypes, minBonus, maxBonus, multiBonus);
                minDamage[(int)elementType] = baseRange.min;
                maxDamage[(int)elementType] = baseRange.max;
            }
            else
            {
                Data.GetMultiStatBonus(multiBonus, null, tagsToUse, multiTypes.ToArray());
                minDamage[(int)elementType] = (int)Math.Max(multiBonus.CalculateStat(baseDamage[elementType].min + minBonus.CalculateStat(0f)), 0);
                maxDamage[(int)elementType] = (int)Math.Max(multiBonus.CalculateStat(baseDamage[elementType].max + maxBonus.CalculateStat(0f)), 0);
            }
        }

        float critChance = Data.GetMultiStatBonus(tagsToUse, BonusType.GLOBAL_CRITICAL_CHANCE).CalculateStat(0f);
        bool isCrit = UnityEngine.Random.Range(0f, 100f) < critChance;
        Dictionary<ElementType, float> returnDict = new Dictionary<ElementType, float>();

        foreach (ElementType elementType in Enum.GetValues(typeof(ElementType)))
        {
            float damage = UnityEngine.Random.Range(minDamage[(int)elementType] + convertedMinDamage[(int)elementType], maxDamage[(int)elementType] + convertedMaxDamage[(int)elementType] + 1);

            if (isCrit)
                damage *= 1 + (Data.GetMultiStatBonus(tagsToUse, BonusType.GLOBAL_CRITICAL_DAMAGE).CalculateStat(50) / 100f);

            returnDict.Add(elementType, damage);
        }

        return returnDict;
    }
}

public enum ActorType
{
    ENEMY,
    ALLY
}