using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
    public const float BASE_SHIELD_RESTORE_DELAY = 3.0f;

    public ActorData Data { get; protected set; }
    public float actorTimeScale = 1f;
    private readonly List<ActorStatusEffect> statusEffects = new List<ActorStatusEffect>();
    private readonly List<StatBonusBuffEffect> buffEffects = new List<StatBonusBuffEffect>();
    protected UIHealthBar healthBar;
    protected List<ActorAbility> instancedAbilitiesList = new List<ActorAbility>();
    protected List<AbilityColliderContainer> abilityColliders = new List<AbilityColliderContainer>();
    protected int nextMovementNode;
    protected bool isMoving;
    public bool isBoss = false;

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
            index--;
        }
    }

    protected void Start()
    {
        InitializeHealthBar();
    }

    protected void Update()
    {
        UpdateStatusEffects();
        ApplyRegenEffects();
        if (Data.IsDead)
            Death();
        if (!this.gameObject.activeSelf)
            return;
        if (isMoving)
        {
            Move();
        }
        healthBar.UpdatePosition(this.transform);
    }

    public HashSet<GroupType> GetActorTags()
    {
        HashSet<GroupType> tags = new HashSet<GroupType>();
        return tags;
    }

    public void InitializeHealthBar()
    {
        healthBar = GetComponentInChildren<UIHealthBar>();
        healthBar.Initialize(Data.MaximumHealth, Data.CurrentHealth, Data.MaximumManaShield, Data.CurrentManaShield, this.transform);
    }

    public void AddStatusEffect(ActorStatusEffect statusEffect)
    {
        if (statusEffect.effectType == EffectType.AURA_BUFF || statusEffect.effectType == EffectType.SELF_BUFF)
            buffEffects.Add((StatBonusBuffEffect)statusEffect);
        else
        {
            ActorStatusEffect existingStatus = statusEffects.Find(x => x.GetType() == statusEffect.GetType());

            if (existingStatus != null)
            {
                if (existingStatus.GetEffectValue() < statusEffect.GetEffectValue())
                    RemoveStatusEffect(existingStatus);
                else
                    return;
            }

            statusEffects.Add(statusEffect);
        }
    }

    public void RemoveStatusEffect(ActorStatusEffect statusEffect)
    {
        if (statusEffect.effectType == EffectType.AURA_BUFF || statusEffect.effectType == EffectType.SELF_BUFF)
            buffEffects.Remove((StatBonusBuffEffect)statusEffect);
        else
        {
            statusEffects.Remove(statusEffect);
        }
    }

    public StatBonusBuffEffect GetBuffStatusEffect(string statusName)
    {
        StatBonusBuffEffect buff = buffEffects.Find(x => x.BuffName.Equals(statusName));
        return buff;
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
        if (Data.CurrentHealth - mod > Data.MaximumHealth)
            Data.CurrentHealth = Data.MaximumHealth;
        else
            Data.CurrentHealth -= mod;

        healthBar.UpdateHealthBar(Data.MaximumHealth, Data.CurrentHealth, Data.MaximumManaShield, Data.CurrentManaShield);
        if (Data.CurrentHealth <= 0)
        {
            Death();
        }
    }

    public float ModifyCurrentShield(float mod)
    {
        float remainingDamage;

        if (Data.CurrentManaShield == 0 || Data.MaximumManaShield == 0)
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
            Data.CurrentShieldDelay = BASE_SHIELD_RESTORE_DELAY * Data.ShieldRestoreDelayModifier;
        }

        healthBar.UpdateHealthBar(Data.MaximumHealth, Data.CurrentHealth, Data.MaximumManaShield, Data.CurrentManaShield);

        return remainingDamage;
    }

    public void ApplyRegenEffects()
    {
        if (Data.MaximumManaShield > 0)
        {
            float shieldModifier = -Data.ShieldRegenRate;
            if (Data.CurrentShieldDelay <= 0f && Data.CurrentManaShield < Data.MaximumManaShield)
                shieldModifier += -Data.ShieldRestoreRate;
            ModifyCurrentShield(shieldModifier * Time.deltaTime);
        }
        ModifyCurrentHealth(Data.HealthRegenRate * Time.deltaTime);
    }

    public void ApplyDamage(Dictionary<ElementType, float> damage, AbilityOnHitDataContainer onHitData, bool isHit)
    {
        float total = 0;
        float physicalDamage = 0, fireDamage = 0, coldDamage = 0, lightningDamage = 0, earthDamage = 0, divineDamage = 0, voidDamage = 0;

        if (isHit)
        {
            if (Data.AttackPhasing > 0 && onHitData.Type == AbilityType.ATTACK)
            {
                if (Data.AttackPhasing == 100 || Random.Range(0, 100) < Data.AttackPhasing)
                    return;
            }
            else if (Data.MagicPhasing > 0 && onHitData.Type == AbilityType.SPELL)
            {
                if (Data.MagicPhasing == 100 || Random.Range(0, 100) < Data.MagicPhasing)
                    return;
            }
            if (Data.DodgeRating > 0)
            {
                float dodgePercent = 1 - (onHitData.accuracy / (onHitData.accuracy + (Data.DodgeRating) / 2f));
                dodgePercent = Mathf.Min(dodgePercent, 85f);
                if (Random.Range(0, 100f) < dodgePercent)
                {
                    return;
                }
            }
        }

        if (damage.ContainsKey(ElementType.PHYSICAL))
        {
            float armorValue = 0;
            if (isHit)
            {
                armorValue = Data.Armor / (Data.Armor + damage[ElementType.PHYSICAL]);
            }
            float physicalResistance = Mathf.Min((1.0f - (Data.GetResistance(ElementType.PHYSICAL) - onHitData.physicalNegation) / 100f) + armorValue, 0.95f);
            //Debug.Log(Data.GetResistance(ElementType.PHYSICAL) + " " + onHitData.physicalNegation + " " + damage[ElementType.PHYSICAL]);
            physicalDamage = physicalResistance * damage[ElementType.PHYSICAL];
            total += System.Math.Max(0, physicalDamage);
        }

        if (damage.ContainsKey(ElementType.FIRE))
        {
            fireDamage = (1f - (Data.GetResistance(ElementType.FIRE) - onHitData.fireNegation) / 100f) * damage[ElementType.FIRE];
            total += System.Math.Max(0, fireDamage);
        }

        if (damage.ContainsKey(ElementType.COLD))
        {
            coldDamage = (1f - (Data.GetResistance(ElementType.COLD) - onHitData.coldNegation) / 100f) * damage[ElementType.COLD];
            total += System.Math.Max(0, coldDamage);
        }

        if (damage.ContainsKey(ElementType.LIGHTNING))
        {
            lightningDamage = (1f - (Data.GetResistance(ElementType.LIGHTNING) - onHitData.lightningNegation) / 100f) * damage[ElementType.LIGHTNING];
            total += System.Math.Max(0, lightningDamage);
        }

        if (damage.ContainsKey(ElementType.EARTH))
        {
            earthDamage = (1f - (Data.GetResistance(ElementType.EARTH) - onHitData.earthNegation) / 100f) * damage[ElementType.EARTH];
            total += System.Math.Max(0, earthDamage);
        }

        if (damage.ContainsKey(ElementType.DIVINE))
        {
            divineDamage = (1f - (Data.GetResistance(ElementType.DIVINE) - onHitData.divineNegation) / 100f) * damage[ElementType.DIVINE];
            total += System.Math.Max(0, divineDamage);
        }

        if (damage.ContainsKey(ElementType.VOID))
        {
            voidDamage = (1f - (Data.GetResistance(ElementType.VOID) - onHitData.voidNegation) / 100f) * damage[ElementType.VOID];
            total += System.Math.Max(0, voidDamage);
        }
        if (isBoss)
            total = total * onHitData.vsBossDamage;

        total = ModifyCurrentShield(total);

        if (Data.HealthIsHitsToKill && isHit && total >= 1f)
            ModifyCurrentHealth(1);
        else
            ModifyCurrentHealth(total);

        if (isHit)
            ApplyAfterHitEffects(damage, onHitData);
    }

    public void ApplyAfterHitEffects(Dictionary<ElementType, float> damage, AbilityOnHitDataContainer postDamageData)
    {
        if (damage[ElementType.PHYSICAL] != 0 && postDamageData.DidEffectProc(EffectType.BLEED))
        {
            AddStatusEffect(new BleedEffect(this, postDamageData.sourceActor, damage[ElementType.PHYSICAL] * postDamageData.GetEffectEffectiveness(EffectType.BLEED), postDamageData.GetEffectDuration(EffectType.BLEED)));
        }

        if (damage[ElementType.FIRE] != 0 && postDamageData.DidEffectProc(EffectType.BURN))
        {
            AddStatusEffect(new BurnEffect(this, postDamageData.sourceActor, damage[ElementType.FIRE] * postDamageData.GetEffectEffectiveness(EffectType.BURN), postDamageData.GetEffectDuration(EffectType.BURN)));
        }

        if (damage[ElementType.COLD] != 0 && postDamageData.DidEffectProc(EffectType.CHILL))
        {
            AddStatusEffect(new ChillEffect(this, postDamageData.sourceActor, postDamageData.GetEffectEffectiveness(EffectType.CHILL), postDamageData.GetEffectDuration(EffectType.CHILL)));
        }

        if (damage[ElementType.LIGHTNING] != 0 && postDamageData.DidEffectProc(EffectType.ELECTROCUTE))
        {
            AddStatusEffect(new ElectrocuteEffect(this, postDamageData.sourceActor, damage[ElementType.LIGHTNING] * postDamageData.GetEffectEffectiveness(EffectType.ELECTROCUTE), postDamageData.GetEffectDuration(EffectType.ELECTROCUTE)));
        }

        if (damage[ElementType.EARTH] != 0 && postDamageData.DidEffectProc(EffectType.FRACTURE))
        {
            AddStatusEffect(new FractureEffect(this, postDamageData.sourceActor, postDamageData.GetEffectEffectiveness(EffectType.FRACTURE), postDamageData.GetEffectDuration(EffectType.FRACTURE)));
        }

        if (damage[ElementType.DIVINE] != 0 && postDamageData.DidEffectProc(EffectType.PACIFY))
        {
            AddStatusEffect(new PacifyEffect(this, postDamageData.sourceActor, postDamageData.GetEffectEffectiveness(EffectType.PACIFY), postDamageData.GetEffectDuration(EffectType.PACIFY)));
        }

        if (damage[ElementType.VOID] != 0 && postDamageData.DidEffectProc(EffectType.RADIATION))
        {
            AddStatusEffect(new RadiationEffect(this, postDamageData.sourceActor, damage[ElementType.VOID] * postDamageData.GetEffectEffectiveness(EffectType.RADIATION), postDamageData.GetEffectDuration(EffectType.RADIATION)));
        }
    }

    public void ApplySingleElementDamage(ElementType element, float damage, int resistanceNegation, bool isHit = true)
    {
        float finalDamage = (1f - (Data.GetResistance(element) - resistanceNegation) / 100f) * damage;
        finalDamage = ModifyCurrentShield(finalDamage);
        ModifyCurrentHealth(finalDamage);
    }

    public void DisableActor()
    {
        gameObject.SetActive(false);
        healthBar.gameObject.SetActive(false);
    }
}

public enum ActorType
{
    ENEMY,
    ALLY
}