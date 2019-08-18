﻿using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }
    private Dictionary<string, AbilityParticleSystem> particleSystems = new Dictionary<string, AbilityParticleSystem>();
    private Dictionary<string, bool> hasParticleEffect = new Dictionary<string, bool>();

    private void Awake()
    {
        Instance = this;
    }

    public float EmitAbilityParticle(string abilityId, ParticleSystem.EmitParams emitParams, float scaling)
    {
        AbilityParticleSystem abilityPs = GetParticleSystem(abilityId);
        if (abilityPs == null)
            return 0;

        emitParams.applyShapeToPosition = true;

        abilityPs.Emit(emitParams, abilityPs.emitCount, scaling);
        return abilityPs.waitUntilNextEmit;
    }

    public float EmitAbilityParticleArc(string abilityId, ParticleSystem.EmitParams emitParams, float scaling, float rotationAngle, Transform parent)
    {
        AbilityParticleSystem abilityPs = GetParticleSystem(abilityId);
        if (abilityPs == null)
            return 0;

        emitParams.applyShapeToPosition = true;

        abilityPs.Emit(emitParams, abilityPs.emitCount, scaling, rotationAngle, parent);
        return abilityPs.waitUntilNextEmit;
    }

    private AbilityParticleSystem GetParticleSystem(string abilityId)
    {
        if (!hasParticleEffect.ContainsKey(abilityId))
        {
            AbilityParticleSystem abs = ResourceManager.Instance.GetAbilityParticleSystem(abilityId);
            if (abs == null)
            {
                hasParticleEffect.Add(abilityId, false);
            }
            else
            {
                particleSystems.Add(abilityId, abs);
                hasParticleEffect.Add(abilityId, true);
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

    public void ClearParticleSystems()
    {
        particleSystems.Clear();
        hasParticleEffect.Clear();
    }
}