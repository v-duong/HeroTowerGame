using UnityEngine;

public class BleedEffect : ActorStatusEffect
{
    protected float damagePerSecond;
    protected Vector2 lastPosition;
    protected float timeSinceLastCheck;

    public BleedEffect(Actor target, Actor source, float inputDamage, float duration) : base(target, source)
    {
        effectType = EffectType.BLEED;
        damagePerSecond = inputDamage * 0.2f;
        this.duration = duration;
        timeSinceLastCheck = 0;
        lastPosition = target.transform.position;
        
    }

    protected override void OnApply()
    {
    }

    public override void OnExpire()
    {
        target.RemoveStatusEffect(this);
    }

    public override void Update(float deltaTime)
    {
        float tick = DurationUpdate(deltaTime);
        float additionalDamage = 0;
        timeSinceLastCheck += tick;
        if (timeSinceLastCheck >= 0.2f)
        {
            timeSinceLastCheck -= 0.2f;
            Vector2 position = target.transform.position;
            float distance = Mathf.Sqrt((position - lastPosition).sqrMagnitude);
            lastPosition = position;
            additionalDamage = distance * damagePerSecond * 0.2f;
        }
        target.ApplySingleElementDamage(ElementType.PHYSICAL, damagePerSecond * tick + additionalDamage, Source.Data.PhysicalNegation, false);
        if (duration <= 0)
            OnExpire();
    }

    public override float GetEffectValue()
    {
        return damagePerSecond * (target.Data.GetResistance(ElementType.PHYSICAL) - Source.Data.PhysicalNegation) / 100f ;
    }
}