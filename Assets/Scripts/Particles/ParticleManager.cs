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
    private Dictionary<ElementType, ParticleSystem> hitEffectSystems = new Dictionary<ElementType, ParticleSystem>();
    private Dictionary<string, Stack<AbilityParticleSystem>> perActorParticleSystems = new Dictionary<string, Stack<AbilityParticleSystem>>();
    private List<AbilityParticleSystem> inUseParticlesSystems = new List<AbilityParticleSystem>();

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void LateUpdate()
    {
        for (int i = inUseParticlesSystems.Count-1; i > 0; i--)
        {
            AbilityParticleSystem abs = inUseParticlesSystems[i];
            if (abs == null)
            {
                inUseParticlesSystems.Remove(abs);
            }
            else if (!abs.ps.IsAlive()) {
                perActorParticleSystems[abs.abilityId].Push(abs);
                inUseParticlesSystems.Remove(abs);
            }
            
        }
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

    public float EmitAbilityParticle(AbilityParticleSystem abilityPs, ParticleSystem.EmitParams emitParams, float scaling, Transform parent)
    {
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
        if (!particleSystems.ContainsKey(abilityId))
        {
            AbilityParticleSystem abs = ResourceManager.Instance.GetAbilityParticleSystem(abilityId);
            if (abs == null)
            {
                AbilityBase abilityBase = ResourceManager.Instance.GetAbilityBase(abilityId);
                abs = ResourceManager.Instance.GetAbilityParticleSystem(abilityBase.effectSprite);
            }
            if (abs != null)
            {
                abs.abilityId = abilityId;
                particleSystems.Add(abilityId, abs);
                if (abs.useDifferentForSameFrame)
                {
                    perActorParticleSystems.Add(abilityId, new Stack<AbilityParticleSystem>());
                    perActorParticleSystems[abilityId].Push(abs);
                }
            }
            else
            {
                particleSystems.Add(abilityId, null);
            }
        }

        particleSystems.TryGetValue(abilityId, out AbilityParticleSystem abilityPs);
        if (abilityPs != null)
        {
            if (abilityPs.useDifferentForSameFrame)
            {
                abilityPs = perActorParticleSystems[abilityId].Pop();
                inUseParticlesSystems.Add(abilityPs);
                if (perActorParticleSystems[abilityId].Count == 0)
                    perActorParticleSystems[abilityId].Push(Instantiate(abilityPs));
            }
            return abilityPs;
        }
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
        perActorParticleSystems.Clear();
        inUseParticlesSystems.Clear();
    }
}