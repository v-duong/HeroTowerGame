using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
    public const float BASE_SHIELD_RESTORE_DELAY = 4.0f;

    public ActorData Data { get; protected set; }
    public float actorTimeScale = 1f;
    private readonly List<ActorEffect> statusEffects = new List<ActorEffect>();
    private readonly List<StatBonusBuffEffect> buffEffects = new List<StatBonusBuffEffect>();
    protected UIHealthBar healthBar;
    protected List<ActorAbility> instancedAbilitiesList = new List<ActorAbility>();
    protected int nextMovementNode;
    protected bool isMoving;
    public bool isBoss = false;
    public int attackLocks = 0;
    public HashSet<GroupType> actorTags = new HashSet<GroupType>();
    public HashSet<GroupType> actorTargetTags = new HashSet<GroupType>();

    public abstract ActorType GetActorType();

    public abstract void Death();

    protected abstract void Move();

    public void UpdateStatusEffects()
    {
        float dT = Time.deltaTime;
        int index = statusEffects.Count - 1;
        while (index >= 0)
        {
            statusEffects[index].Update(dT);
            if (statusEffects[index].duration <= 0)
            {
                RemoveStatusEffect(statusEffects[index]);
            }
            index--;
        }

        index = buffEffects.Count - 1;
        while (index >= 0)
        {
            buffEffects[index].Update(dT);
            if (buffEffects[index].duration <= 0)
            {
                RemoveStatusEffect(buffEffects[index]);
            }
            index--;
        }
    }

    protected void Start()
    {
        healthBar = GetComponentInChildren<UIHealthBar>();
        InitializeHealthBar();
    }

    protected void Update()
    {
        UpdateStatusEffects();
        ApplyRegenEffects();
        if (Data.IsDead)
            Death();
        if (!gameObject.activeSelf)
            return;
        if (isMoving)
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
            healthBar.Initialize(Data.MaximumHealth, Data.CurrentHealth, Data.MaximumManaShield, Data.CurrentManaShield, transform);
    }

    public void AddStatusEffect(ActorEffect statusEffect)
    {
        if (statusEffect.effectType == EffectType.BUFF || statusEffect.effectType == EffectType.DEBUFF)
        {
            buffEffects.Add((StatBonusBuffEffect)statusEffect);
        }
        else
        {
            List<ActorEffect> existingStatus = statusEffects.FindAll(x => x.GetType() == statusEffect.GetType());

            statusEffect.duration *= Data.AfflictedStatusDuration;

            if (existingStatus.Count == 1 && statusEffect.MaxStacks == 1)
            {
                if (statusEffect.GetSimpleEffectValue() > existingStatus[0].GetSimpleEffectValue())
                    RemoveStatusEffect(existingStatus[0]);
                else if (statusEffect.GetSimpleEffectValue() == existingStatus[0].GetSimpleEffectValue())
                {
                    existingStatus[0].RefreshDuration(statusEffect.duration);
                    return;
                }
                else
                    return;
            }
            else if (existingStatus.Count == statusEffect.MaxStacks)
            {
                ActorEffect effectToRemove = existingStatus.Find(x => x.GetSimpleEffectValue() < statusEffect.GetSimpleEffectValue());
                if (effectToRemove != null)
                    RemoveStatusEffect(effectToRemove);
                else
                    return;
            }
            else if (existingStatus.Count > statusEffect.MaxStacks)
                return;

            statusEffects.Add(statusEffect);
            statusEffect.OnApply();
            actorTags.Add(statusEffect.StatusTag);
        }
        Data.UpdateActorData();
    }

    public void RemoveStatusEffect(ActorEffect statusEffect)
    {
        statusEffect.OnExpire();

        if (statusEffect.effectType == EffectType.BUFF || statusEffect.effectType == EffectType.DEBUFF)
            buffEffects.Remove((StatBonusBuffEffect)statusEffect);
        else
        {
            statusEffects.Remove(statusEffect);
            actorTags.Remove(statusEffect.StatusTag);
        }
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

    public List<StatBonusBuffEffect> GetBuffStatusEffect(string statusName)
    {
        List<StatBonusBuffEffect> buffs = buffEffects.FindAll(x => x.BuffName.Equals(statusName));
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

    public List<AbilityBase> GetAbilitiesInList()
    {
        List<AbilityBase> ret = new List<AbilityBase>();
        foreach (ActorAbility ability in instancedAbilitiesList)
        {
            ret.Add(ability.abilityBase);
        }
        return ret;
    }

    public void ModifyCurrentHealth(float mod)
    {
        if (mod == 0)
            return;

        if (Data.CurrentHealth - mod > Data.MaximumHealth)
            Data.CurrentHealth = Data.MaximumHealth;
        else
            Data.CurrentHealth -= mod;

        healthBar.UpdateHealthBar(Data.MaximumHealth, Data.CurrentHealth, Data.MaximumManaShield, Data.CurrentManaShield);
    }

    public float ModifyCurrentShield(float mod)
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
        else if (Data.CurrentManaShield < mod)
        {
            Data.CurrentManaShield = 0;
            remainingDamage = mod - Data.CurrentManaShield;
        }
        else
        {
            Data.CurrentManaShield -= mod;
            remainingDamage = 0;
        }

        if (mod > 0)
        {
            Data.CurrentShieldDelay = Math.Max(BASE_SHIELD_RESTORE_DELAY * Data.ShieldRestoreDelayModifier, 0.5f);
        }

        healthBar.UpdateHealthBar(Data.MaximumHealth, Data.CurrentHealth, Data.MaximumManaShield, Data.CurrentManaShield);

        return remainingDamage;
    }

    public void ApplyRegenEffects()
    {
        if (Data.MaximumManaShield > 0)
        {
            if (Data.CurrentShieldDelay > 0)
                Data.CurrentShieldDelay -= Time.deltaTime;

            float shieldModifier = -Data.ShieldRegenRate;
            if (Data.CurrentShieldDelay <= 0f && Data.CurrentManaShield < Data.MaximumManaShield)
                shieldModifier += -Data.ShieldRestoreRate;
            ModifyCurrentShield(shieldModifier * Time.deltaTime);
        }
        ModifyCurrentHealth(-Data.HealthRegenRate * Time.deltaTime);
    }

    public void ApplyDamage(Dictionary<ElementType, float> damage, OnHitDataContainer onHitData, bool isHit, bool isFromSecondaryEffect, EffectType? sourceType = null)
    {
        float total = 0;
        if (Data.IsDead)
            return;

        if (damage.ContainsKey(ElementType.PHYSICAL))
        {
            float armorReductionRate = 1.0f;
            if (isHit && Data.Armor > 0)
            {
                armorReductionRate = 1.0f - (Data.Armor / (Data.Armor + damage[ElementType.PHYSICAL]));
            }
            float resistanceReductionRate = 1.0f - ((Data.GetResistance(ElementType.PHYSICAL) - onHitData.GetNegation(ElementType.PHYSICAL)) / 100f);
            float physicalReduction = armorReductionRate * resistanceReductionRate;
            //Debug.Log(Data.GetResistance(ElementType.PHYSICAL) + " " + onHitData.physicalNegation + " " + damage[ElementType.PHYSICAL]);
            float physicalDamage = physicalReduction * damage[ElementType.PHYSICAL];
            total += Math.Max(0, physicalDamage);
        }

        if (damage.ContainsKey(ElementType.FIRE))
        {
            float fireDamage = (1f - (Data.GetResistance(ElementType.FIRE) - onHitData.GetNegation(ElementType.FIRE)) / 100f) * damage[ElementType.FIRE];
            total += Math.Max(0, fireDamage);
        }

        if (damage.ContainsKey(ElementType.COLD))
        {
            float coldDamage = (1f - (Data.GetResistance(ElementType.COLD) - onHitData.GetNegation(ElementType.COLD)) / 100f) * damage[ElementType.COLD];
            total += Math.Max(0, coldDamage);
            damage[ElementType.COLD] = coldDamage;
        }

        if (damage.ContainsKey(ElementType.LIGHTNING))
        {
            float lightningDamage = (1f - (Data.GetResistance(ElementType.LIGHTNING) - onHitData.GetNegation(ElementType.LIGHTNING)) / 100f) * damage[ElementType.LIGHTNING];
            total += Math.Max(0, lightningDamage);
        }

        if (damage.ContainsKey(ElementType.EARTH))
        {
            float earthDamage = (1f - (Data.GetResistance(ElementType.EARTH) - onHitData.GetNegation(ElementType.EARTH)) / 100f) * damage[ElementType.EARTH];
            total += Math.Max(0, earthDamage);
            damage[ElementType.EARTH] = earthDamage;
        }

        if (damage.ContainsKey(ElementType.DIVINE))
        {
            float divineDamage = (1f - (Data.GetResistance(ElementType.DIVINE) - onHitData.GetNegation(ElementType.DIVINE)) / 100f) * damage[ElementType.DIVINE];
            total += Math.Max(0, divineDamage);
            damage[ElementType.DIVINE] = divineDamage;
        }

        if (damage.ContainsKey(ElementType.VOID))
        {
            float voidDamage = (1f - (Data.GetResistance(ElementType.VOID) - onHitData.GetNegation(ElementType.VOID)) / 100f) * damage[ElementType.VOID];
            total += Math.Max(0, voidDamage);
        }

        if (isHit && !isFromSecondaryEffect)
            total *= onHitData.directHitDamage;

        if (isBoss)
            total *= onHitData.vsBossDamage;

        if (sourceType != EffectType.POISON)
            total = ModifyCurrentShield(total);

        if (Data.HealthIsHitsToKill && isHit && total >= 1f)
            ModifyCurrentHealth(1);
        else
            ModifyCurrentHealth(total);

        if (Data.IsDead)
        {
            onHitData.ApplyTriggerEffects(TriggerType.ON_KILL, this);
        }
        if (isHit)
        {
            if (Data.IsDead)
                onHitData.ApplyTriggerEffects(TriggerType.ON_HIT_KILL, this);
            ApplyAfterHitEffects(damage, onHitData);
        }
    }

    public void ApplyAfterHitEffects(Dictionary<ElementType, float> damage, OnHitDataContainer onHitData)
    {
        onHitData.ApplyTriggerEffects(TriggerType.ON_HIT, this);

        if (damage.ContainsKey(ElementType.PHYSICAL) && damage[ElementType.PHYSICAL] > 0)
        {
            if (onHitData.DidEffectProc(EffectType.BLEED, Data.AfflictedStatusAvoidance))
            {
                onHitData.ApplyEffectToTarget(this, EffectType.BLEED, damage[ElementType.PHYSICAL] * onHitData.GetEffectEffectiveness(EffectType.BLEED) * Data.AfflictedStatusDamageResistance * BleedEffect.BASE_DAMAGE_MULTIPLIER);
            }
            if (onHitData.DidEffectProc(EffectType.POISON, Data.AfflictedStatusAvoidance))
            {
                onHitData.ApplyEffectToTarget(this, EffectType.POISON, damage[ElementType.PHYSICAL] * onHitData.GetEffectEffectiveness(EffectType.POISON) * Data.AfflictedStatusDamageResistance * PoisonEffect.BASE_DAMAGE_MULTIPLIER);
            }
        }

        if (damage.ContainsKey(ElementType.FIRE) && damage[ElementType.FIRE] > 0 && onHitData.DidEffectProc(EffectType.BURN, Data.AfflictedStatusAvoidance))
        {
            //float percent = (damage[ElementType.FIRE] * onHitData.GetEffectEffectiveness(EffectType.BURN) * Data.AfflictedStatusDamageResistance) / damage[ElementType.FIRE];
            //Debug.Log(damage[ElementType.FIRE] + " BURN  " + damage[ElementType.FIRE] * onHitData.GetEffectEffectiveness(EffectType.BURN) * Data.AfflictedStatusDamageResistance + "   " + percent.ToString("F3"));
            onHitData.ApplyEffectToTarget(this, EffectType.BURN, damage[ElementType.FIRE] * onHitData.GetEffectEffectiveness(EffectType.BURN) * Data.AfflictedStatusDamageResistance * BurnEffect.BASE_DAMAGE_MULTIPLIER);
        }

        if (damage.ContainsKey(ElementType.COLD) && damage[ElementType.COLD] > 0 && onHitData.DidEffectProc(EffectType.CHILL, Data.AfflictedStatusAvoidance))
        {
            float percentageDealt = damage[ElementType.COLD] / Data.MaximumHealth;
            
            float chillEffectPower = ChillEffect.BASE_CHILL_EFFECT * Math.Min(percentageDealt / (ChillEffect.BASE_CHILL_THRESHOLD * Data.AfflictedStatusThreshold), 1f) * onHitData.GetEffectEffectiveness(EffectType.CHILL);
            //Debug.Log(damage[ElementType.COLD] + " " + percentageDealt + " " + chillEffectPower);
            onHitData.ApplyEffectToTarget(this, EffectType.CHILL, chillEffectPower);
        }

        if (damage.ContainsKey(ElementType.LIGHTNING) && damage[ElementType.LIGHTNING] > 0 && onHitData.DidEffectProc(EffectType.ELECTROCUTE, Data.AfflictedStatusAvoidance))
        {
            onHitData.ApplyEffectToTarget(this, EffectType.ELECTROCUTE, damage[ElementType.LIGHTNING] * onHitData.GetEffectEffectiveness(EffectType.ELECTROCUTE) * Data.AfflictedStatusDamageResistance * ElectrocuteEffect.BASE_DAMAGE_MULTIPLIER);
        }

        if (damage.ContainsKey(ElementType.EARTH) && damage[ElementType.EARTH] > 0 && onHitData.DidEffectProc(EffectType.FRACTURE, Data.AfflictedStatusAvoidance))
        {
            float percentageDealt = damage[ElementType.EARTH] / Data.MaximumHealth;
            float fractureEffectPower = FractureEffect.BASE_FRACTURE_EFFECT * Math.Min(percentageDealt / (FractureEffect.BASE_FRACTURE_THRESHOLD * Data.AfflictedStatusThreshold), 1f) * onHitData.GetEffectEffectiveness(EffectType.FRACTURE);

            onHitData.ApplyEffectToTarget(this, EffectType.FRACTURE, fractureEffectPower);
        }

        if (damage.ContainsKey(ElementType.DIVINE) && damage[ElementType.DIVINE] > 0 && onHitData.DidEffectProc(EffectType.PACIFY, Data.AfflictedStatusAvoidance))
        {
            float percentageDealt = damage[ElementType.DIVINE] / Data.MaximumHealth;
            float pacifyEffectPower = PacifyEffect.BASE_PACIFY_EFFECT * Math.Min(percentageDealt / (PacifyEffect.BASE_PACIFY_THRESHOLD * Data.AfflictedStatusThreshold), 1f) * onHitData.GetEffectEffectiveness(EffectType.PACIFY);

            onHitData.ApplyEffectToTarget(this, EffectType.PACIFY, pacifyEffectPower);
        }

        if (damage.ContainsKey(ElementType.VOID) && damage[ElementType.VOID] > 0 && onHitData.DidEffectProc(EffectType.RADIATION, Data.AfflictedStatusAvoidance))
        {
            onHitData.ApplyEffectToTarget(this, EffectType.RADIATION, damage[ElementType.VOID] * onHitData.GetEffectEffectiveness(EffectType.RADIATION) * Data.AfflictedStatusDamageResistance * RadiationEffect.BASE_DAMAGE_MULTIPLIER);
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
        foreach (var x in instancedAbilitiesList)
        {
            x.StopFiring(this);
        }

        gameObject.SetActive(false);
        healthBar.gameObject.SetActive(false);
    }

    public void EnableHealthBar()
    {
        if (healthBar != null)
            healthBar.gameObject.SetActive(true);
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
            if (baseDamage.ContainsKey(elementType))
            {
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
        }

        float critChance = Data.GetMultiStatBonus(tagsToUse, BonusType.GLOBAL_CRITICAL_CHANCE).CalculateStat(0f);
        bool isCrit = UnityEngine.Random.Range(0f, 100f) < critChance;
        Dictionary<ElementType, float> returnDict = new Dictionary<ElementType, float>();

        foreach (ElementType elementType in Enum.GetValues(typeof(ElementType)))
        {
            float damage = UnityEngine.Random.Range(minDamage[(int)elementType] + convertedMinDamage[(int)elementType], maxDamage[(int)elementType] + convertedMaxDamage[(int)elementType] + 1);

            if (damage > 0)
                Debug.Log(elementType + " " + damage);

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