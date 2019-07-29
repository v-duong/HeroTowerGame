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

    public abstract ActorType GetActorType();

    public abstract void Death();

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

        if (ability.TargetType == AbilityTargetType.ENEMY)
        {
            collider.gameObject.layer = LayerMask.NameToLayer("EnemyDetect");
        }
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

    public void ApplyDamage(Dictionary<ElementType, int> damage)
    {
        double total = 0;
        ElementType element;
        for (int i = 0; i < (int)ElementType.COUNT; i++)
        {
            element = (ElementType)i;
            if (damage.ContainsKey(element))
            {
                total += ((1.0 - Data.Resistances[element] / 100d) * damage[element]);
            }
        }
        ModifyCurrentHealth(total);
    }
}

public enum ActorType
{
    ENEMY,
    ALLY
}