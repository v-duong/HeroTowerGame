using System.Collections.Generic;
using UnityEngine;

public abstract class Actor : MonoBehaviour
{
    public ActorData Data { get; protected set; }
    public float actorTimeScale = 1f;
    private readonly List<ActorStatusEffect> statusEffects = new List<ActorStatusEffect>();
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
        healthBar.Initialize(Data.MaximumHealth, Data.CurrentHealth, this.transform);
    }

    public void AddStatusEffect(ActorStatusEffect statusEffect)
    {
        statusEffects.Add(statusEffect);
    }

    public void RemoveStatusEffect(ActorStatusEffect statusEffect)
    {
        statusEffects.Remove(statusEffect);
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
        collider.radius = ability.abilityBase.targetRange + 0.5f;
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

        healthBar.UpdateHealthBar(Data.MaximumHealth, Data.CurrentHealth);
        if (Data.CurrentHealth <= 0)
        {
            Death();
        }
    }

    public void ApplyDamage(Dictionary<ElementType, double> damage, AbilityStatusEffectDataContainer statusData, bool isHit = true)
    {
        double total = 0, physicalDamage = 0, fireDamage = 0, coldDamage = 0, lightningDamage = 0, earthDamage = 0, divineDamage = 0, voidDamage = 0;

        if (damage.ContainsKey(ElementType.PHYSICAL))
        {
            physicalDamage = (1.0 - Data.Resistances[ElementType.PHYSICAL] / 100d) * damage[ElementType.PHYSICAL];
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

        Debug.Log(total);

        if (isBoss)
            ModifyCurrentHealth(total * statusData.vsBossDamage);
        else
            ModifyCurrentHealth(total);

        if (physicalDamage != 0 && statusData.DidBleedProc())
        {
            AddStatusEffect(new BleedEffect(this, physicalDamage * statusData.bleedEffectiveness, statusData.bleedDuration));
        }

        if (fireDamage != 0 && statusData.DidBurnProc())
        {
            AddStatusEffect(new BurnEffect(this, fireDamage * statusData.burnEffectiveness, statusData.burnDuration));
        }

        if (coldDamage != 0 && statusData.DidChillProc())
        {
            AddStatusEffect(new ChillEffect(this, statusData.chillEffectiveness, statusData.chillDuration));
        }

        if (lightningDamage != 0 && statusData.DidElectrocuteProc())
        {
            AddStatusEffect(new ElectrocuteEffect(this, lightningDamage * statusData.electrocuteEffectiveness, statusData.electrocuteDuration));
        }

        if (earthDamage != 0 && statusData.DidFractureProc())
        {
            AddStatusEffect(new FractureEffect(this, statusData.fractureEffectiveness, statusData.fractureDuration));
        }

        if (divineDamage != 0 && statusData.DidPacifyProc())
        {
            AddStatusEffect(new PacifyEffect(this, statusData.pacifyEffectiveness, statusData.pacifyDuration));
        }

        if (voidDamage != 0 && statusData.DidRadiationProc())
        {
            AddStatusEffect(new RadiationEffect(this, voidDamage * statusData.radiationEffectiveness, statusData.radiationDuration));
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