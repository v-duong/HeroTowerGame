using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public abstract class AbilityEffect : ScriptableObject {
    public abstract int EffectVarCount { get; }
    public abstract void DoEffect(Projectile proj, Actor target);
    public abstract void Initialize(List<float> effectVar);
}


[Serializable] 
public class AbilityEffectEntry
{
    public AbilityEffect effect;
    public List<float> effectVariables;
}


public enum AbilityEffectTag {
    CHAIN,
    AREA,
}