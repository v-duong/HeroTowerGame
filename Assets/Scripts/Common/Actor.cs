using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
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
        if (!this.gameObject.activeSelf)
            return;
        if (isMoving)
        {
            Move();
        }
        healthBar.UpdatePosition(this.transform);
    }

    public void InitializeHealthBar()
    {
        healthBar = GetComponentInChildren<UIHealthBar>();
        healthBar.Initialize(Data.MaximumHealth, Data.CurrentHealth, Data.MaximumManaShield, Data.CurrentManaShield, this.transform);
    }

    public void AddStatusEffect(ActorStatusEffect statusEffect)
    {
        statusEffects.Add(statusEffect);
        if (statusEffect.effectType == EffectType.AURA_BUFF || statusEffect.effectType == EffectType.SELF_BUFF)
            buffEffects.Add((StatBonusBuffEffect)statusEffect);
    }

    public void RemoveStatusEffect(ActorStatusEffect statusEffect)
    {
        statusEffects.Remove(statusEffect);
        if (statusEffect.effectType == EffectType.AURA_BUFF || statusEffect.effectType == EffectType.SELF_BUFF)
            buffEffects.Remove((StatBonusBuffEffect)statusEffect);
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
        collider.radius = ability.abilityBase.targetRange;
        abilityContainer.abilityCollider = collider;
        collider.isTrigger = true;

        collider.gameObject.layer = ability.targetLayer;
    }

    public void ModifyCurrentHealth(double mod)
    {
        if (Data.CurrentHealth - mod > Data.MaximumHealth)
            Data.CurrentHealth = Data.MaximumHealth;
        else
            Data.CurrentHealth -= (float)mod;

        healthBar.UpdateHealthBar(Data.MaximumHealth, Data.CurrentHealth, Data.MaximumManaShield, Data.MaximumHealth);
        if (Data.CurrentHealth <= 0)
        {
            Death();
        }
    }

    public double ModifyCurrentShield(double mod)
    {
        if (Data.CurrentManaShield == 0)
            return mod;
        else if (Data.CurrentManaShield - mod > Data.MaximumManaShield)
        {
            Data.CurrentManaShield = Data.MaximumManaShield;
            return 0;
        }
        else if (Data.CurrentManaShield < mod)
        {
            mod -= Data.CurrentManaShield;
            Data.CurrentManaShield = 0;
            return mod;
        }
        else
        {
            Data.CurrentManaShield -= (float)mod;
            return 0;
        }
    }

    public void ApplyDamage(Dictionary<ElementType, double> damage, AbilityOnHitDataContainer onHitData, bool isHit = true)
    {
        double total = 0, physicalDamage = 0, fireDamage = 0, coldDamage = 0, lightningDamage = 0, earthDamage = 0, divineDamage = 0, voidDamage = 0;

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
                if (Random.Range(0,100f) < dodgePercent)
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
                armorValue = Data.Armor / (float)(Data.Armor + physicalDamage);
            }
            float physicalResistance = Mathf.Min((1.0f - Data.Resistances[ElementType.PHYSICAL] / 100f) + armorValue, 0.95f);
            physicalDamage = physicalResistance * damage[ElementType.PHYSICAL];
            total += physicalDamage;
        }

        if (damage.ContainsKey(ElementType.FIRE))
        {
            fireDamage = (1.0 - Data.Resistances[ElementType.FIRE] / 100d) * damage[ElementType.FIRE];
            total += fireDamage;
        }

        if (damage.ContainsKey(ElementType.COLD))
        {
            coldDamage = (1.0 - Data.Resistances[ElementType.COLD] / 100d) * damage[ElementType.COLD];
            total += coldDamage;
        }

        if (damage.ContainsKey(ElementType.LIGHTNING))
        {
            lightningDamage = (1.0 - Data.Resistances[ElementType.LIGHTNING] / 100d) * damage[ElementType.LIGHTNING];
            total += lightningDamage;
        }

        if (damage.ContainsKey(ElementType.EARTH))
        {
            earthDamage = (1.0 - Data.Resistances[ElementType.EARTH] / 100d) * damage[ElementType.EARTH];
            total += earthDamage;
        }

        if (damage.ContainsKey(ElementType.DIVINE))
        {
            divineDamage = (1.0 - Data.Resistances[ElementType.DIVINE] / 100d) * damage[ElementType.DIVINE];
            total += divineDamage;
        }

        if (damage.ContainsKey(ElementType.VOID))
        {
            voidDamage = (1.0 - Data.Resistances[ElementType.VOID] / 100d) * damage[ElementType.VOID];
            total += voidDamage;
        }
        if (isBoss)
            total = total * onHitData.vsBossDamage;
        total = ModifyCurrentShield(total);
        ModifyCurrentHealth(total);

        ApplyAfterHitEffects(damage, onHitData);
    }

    public void ApplyAfterHitEffects(Dictionary<ElementType, double> damage, AbilityOnHitDataContainer postDamageData)
    {
        if (damage[ElementType.PHYSICAL] != 0 && postDamageData.DidBleedProc())
        {
            AddStatusEffect(new BleedEffect(this, damage[ElementType.PHYSICAL] * postDamageData.bleedEffectiveness, postDamageData.bleedDuration));
        }

        if (damage[ElementType.FIRE] != 0 && postDamageData.DidBurnProc())
        {
            AddStatusEffect(new BurnEffect(this, damage[ElementType.FIRE] * postDamageData.burnEffectiveness, postDamageData.burnDuration));
        }

        if (damage[ElementType.COLD] != 0 && postDamageData.DidChillProc())
        {
            AddStatusEffect(new ChillEffect(this, postDamageData.chillEffectiveness, postDamageData.chillDuration));
        }

        if (damage[ElementType.LIGHTNING] != 0 && postDamageData.DidElectrocuteProc())
        {
            AddStatusEffect(new ElectrocuteEffect(this, damage[ElementType.LIGHTNING] * postDamageData.electrocuteEffectiveness, postDamageData.electrocuteDuration));
        }

        if (damage[ElementType.EARTH] != 0 && postDamageData.DidFractureProc())
        {
            AddStatusEffect(new FractureEffect(this, postDamageData.fractureEffectiveness, postDamageData.fractureDuration));
        }

        if (damage[ElementType.DIVINE] != 0 && postDamageData.DidPacifyProc())
        {
            AddStatusEffect(new PacifyEffect(this, postDamageData.pacifyEffectiveness, postDamageData.pacifyDuration));
        }

        if (damage[ElementType.VOID] != 0 && postDamageData.DidRadiationProc())
        {
            AddStatusEffect(new RadiationEffect(this, damage[ElementType.VOID] * postDamageData.radiationEffectiveness, postDamageData.radiationDuration));
        }
    }

    public void ApplySingleElementDamage(ElementType element, double damage, bool isHit = true)
    {
        double finalDamage = ((1.0 - Data.Resistances[element] / 100d) * damage);
        ModifyCurrentHealth(finalDamage);
    }

    public void DisableActor()
    {
        this.gameObject.SetActive(false);
        this.healthBar.gameObject.SetActive(false);
    }
}

public enum ActorType
{
    ENEMY,
    ALLY
}