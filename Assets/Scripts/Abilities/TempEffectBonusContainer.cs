﻿using System.Collections.Generic;

public class TempEffectBonusContainer
{
    public List<StatusBonus> cachedAuraBonuses;
    public List<SpecialBonus> cachedAuraSpecialEffects;
    public float auraStrength = 0;
    public float auraEffectMultiplier = 1f;
    public float selfAuraEffectMultiplier = 1f;
    public bool isOutdated = true;

    public class StatusBonus
    {
        public BonusType bonusType;
        public ModifyType modifyType;
        public float effectValue;
        public float effectDuration;

        public StatusBonus(BonusType bonusType, ModifyType modifyType, float effectValue, float effectDuration)
        {
            this.bonusType = bonusType;
            this.modifyType = modifyType;
            this.effectValue = effectValue;
            this.effectDuration = effectDuration;
        }
    }

    public class SpecialBonus
    {
        public EffectType effectType;
        public float effectValue;
        public float effectDuration;

        public SpecialBonus(EffectType effectType, float effectValue, float effectDuration)
        {
            this.effectType = effectType;
            this.effectValue = effectValue;
            this.effectDuration = effectDuration;
        }
    }
}