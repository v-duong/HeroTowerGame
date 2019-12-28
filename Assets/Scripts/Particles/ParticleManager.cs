using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public ParticleSystem hitEffectPhysicalPrefab;
    public ParticleSystem hitEffectFirePrefab;
    public ParticleSystem hitEffectColdPrefab;
    public ParticleSystem hitEffectLightningPrefab;
    public ParticleSystem hitEffectEarthPrefab;
    public ParticleSystem hitEffectDivinePrefab;
    public ParticleSystem hitEffectVoidPrefab;

    public static ParticleManager Instance { get; private set; }
    private Dictionary<string, AbilityParticleSystem> particleSystems = new Dictionary<string, AbilityParticleSystem>();
    private Dictionary<string, bool> hasParticleEffect = new Dictionary<string, bool>();
    private Dictionary<ElementType, ParticleSystem> hitEffectSystems = new Dictionary<ElementType, ParticleSystem>();

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public bool DoesAbilityEmitOnSelf(string abilityId)
    {
        AbilityParticleSystem abilityPs = GetParticleSystem(abilityId);
        if (abilityPs == null)
            return false;
        return abilityPs.extraEmitOnSelf;
    }

    public float EmitAbilityParticle(string abilityId, ParticleSystem.EmitParams emitParams, float scaling, Transform parent)
    {
        AbilityParticleSystem abilityPs = GetParticleSystem(abilityId);
        if (abilityPs == null)
            return 0;

        emitParams.applyShapeToPosition = true;

        abilityPs.Emit(emitParams, abilityPs.emitCount, scaling);
        return abilityPs.waitUntilNextEmit;
    }

    public float EmitAbilityParticle_Rotated(string abilityId, ParticleSystem.EmitParams emitParams, float scaling, float rotationAngle, Transform parent)
    {
        AbilityParticleSystem abilityPs = GetParticleSystem(abilityId);
        if (abilityPs == null)
            return 0;

        emitParams.applyShapeToPosition = true;

        abilityPs.Emit(emitParams, abilityPs.emitCount, scaling, rotationAngle, parent);
        return abilityPs.waitUntilNextEmit;
    }

    public AbilityParticleSystem GetParticleSystem(string abilityId)
    {
        if (!hasParticleEffect.ContainsKey(abilityId))
        {
            
            AbilityParticleSystem abs = ResourceManager.Instance.GetAbilityParticleSystem(abilityId);
            if (abs == null)
            {
                AbilityBase abilityBase = ResourceManager.Instance.GetAbilityBase(abilityId);
                abs = ResourceManager.Instance.GetAbilityParticleSystem(abilityBase.effectSprite);
            }
            if (abs != null)
            {
                particleSystems.Add(abilityId, abs);
                hasParticleEffect.Add(abilityId, true);
            } else
            {
                hasParticleEffect.Add(abilityId, false);
            }
        }

        if (hasParticleEffect[abilityId] == false)
            return null;

        particleSystems.TryGetValue(abilityId, out AbilityParticleSystem abilityPs);
        if (abilityPs != null)
            return abilityPs;
        else
            return null;
    }

    public void InitializeHitEffectInstances()
    {
        hitEffectSystems.Clear();
        hitEffectSystems.Add(ElementType.PHYSICAL, Instantiate(hitEffectPhysicalPrefab));
        hitEffectSystems.Add(ElementType.FIRE, Instantiate(hitEffectFirePrefab));
        hitEffectSystems.Add(ElementType.COLD, Instantiate(hitEffectColdPrefab));
        hitEffectSystems.Add(ElementType.LIGHTNING, Instantiate(hitEffectLightningPrefab));
        hitEffectSystems.Add(ElementType.EARTH, Instantiate(hitEffectEarthPrefab));
        hitEffectSystems.Add(ElementType.DIVINE, Instantiate(hitEffectDivinePrefab));
        hitEffectSystems.Add(ElementType.VOID, Instantiate(hitEffectVoidPrefab));
    }

    public void EmitOnHitEffect(ElementType element, Vector3 position)
    {
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
        {
            position = position
        };
        hitEffectSystems[element]?.Emit(emitParams, 1);
    }

    public void ClearParticleSystems()
    {
        particleSystems.Clear();
        hasParticleEffect.Clear();
    }
}