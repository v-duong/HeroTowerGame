﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class HeroData : ActorData
{
    public float BaseStrength { get; private set; }
    public float BaseIntelligence { get; private set; }
    public float BaseAgility { get; private set; }
    public float BaseWill { get; private set; }

    public int Strength { get; private set; }
    public int Intelligence { get; private set; }
    public int Agility { get; private set; }
    public int Will { get; private set; }

    public int ArchetypePoints { get; private set; }

    private const float DUAL_WIELD_ATTACK_SPEED_BONUS = 10f;

    //private Dictionary<BonusType, StatBonus> archetypeStatBonuses;
    private Dictionary<BonusType, StatBonus> attributeStatBonuses;

    private Dictionary<BonusType, StatBonusCollection> archetypeStatBonuses;

    private Equipment[] equipList;
    private HeroArchetypeData[] archetypeList;
    private List<AbilitySlot> abilitySlotList;

    public HeroArchetypeData PrimaryArchetype => archetypeList[0];
    public HeroArchetypeData SecondaryArchetype => archetypeList[1];
    public int assignedTeam;

    public bool IsLocked;
    public bool isDualWielding;
    private bool UpdatesAreDeferred = false;

    private HeroData() : base()
    {
        Initialize();
    }

    private HeroData(string name) : base()
    {
        Initialize(name);
    }

    public HeroData(SaveData.HeroSaveData heroSaveData) : base()
    {
        UpdatesAreDeferred = true;
        PlayerStats ps = GameManager.Instance.PlayerStats;
        equipList = new Equipment[10];
        archetypeList = new HeroArchetypeData[2];
        archetypeStatBonuses = new Dictionary<BonusType, StatBonusCollection>();
        attributeStatBonuses = new Dictionary<BonusType, StatBonus>();
        abilitySlotList = new List<AbilitySlot>() { new AbilitySlot(0), new AbilitySlot(1) };

        Id = heroSaveData.id;
        Name = heroSaveData.name;
        Level = heroSaveData.level;
        ArchetypePoints = Level;
        Experience = heroSaveData.experience;

        BaseHealth = heroSaveData.baseHealth;
        BaseSoulPoints = heroSaveData.baseSoulPoints;
        BaseStrength = heroSaveData.baseStrength;
        BaseIntelligence = heroSaveData.baseIntelligence;
        BaseAgility = heroSaveData.baseAgility;
        BaseWill = heroSaveData.baseWill;

        BaseDodgeRating = 0;
        BaseResolveRating = 0;
        BaseAttackPhasing = 0;
        BaseMagicPhasing = 0;
        movementSpeed = 2.5f;

        archetypeList[0] = new HeroArchetypeData(heroSaveData.primaryArchetypeData, this);

        if (heroSaveData.secondaryArchetypeData != null)
        {
            archetypeList[1] = new HeroArchetypeData(heroSaveData.secondaryArchetypeData, this);
        }

        foreach (EquipSlotType equipSlot in Enum.GetValues(typeof(EquipSlotType)))
        {
            if (equipSlot == EquipSlotType.RING)
                continue;

            if (heroSaveData.equipList[(int)equipSlot] != Guid.Empty)
            {
                EquipToSlot(ps.GetEquipmentByGuid(heroSaveData.equipList[(int)equipSlot]), equipSlot);
            }
        }

        if (heroSaveData.firstAbilitySlot != null)
        {
            var firstSlot = heroSaveData.firstAbilitySlot;
            FindAndEquipAbility(firstSlot.abilityId, firstSlot.sourceId, firstSlot.sourceType, 0);
        }

        if (heroSaveData.secondAbilitySlot != null)
        {
            var slotData = heroSaveData.secondAbilitySlot;
            FindAndEquipAbility(slotData.abilityId, slotData.sourceId, slotData.sourceType, 1);
        }

        UpdatesAreDeferred = false;

        UpdateActorData();
    }

    public void InitHeroActor(GameObject actor)
    {
        CurrentHealth = MaximumHealth;
        CurrentManaShield = MaximumManaShield;
        CurrentSoulPoints = 50;
        HeroActor hero = actor.AddComponent<HeroActor>();
        CurrentActor = hero;
        hero.Initialize(this);
    }

    private void Initialize(string name = "")
    {
        Name = name;
        Level = 0;
        Experience = 0;
        ArchetypePoints = 0;
        BaseHealth = 100;
        BaseSoulPoints = 50;
        BaseManaShield = 0;
        BaseStrength = 10;
        BaseAgility = 10;
        BaseIntelligence = 10;
        BaseWill = 10;
        BaseArmor = 0;
        BaseDodgeRating = 0;
        BaseResolveRating = 0;
        BaseAttackPhasing = 0;
        BaseMagicPhasing = 0;
        movementSpeed = 2.5f;
        IsLocked = false;
        isDualWielding = false;
        assignedTeam = -1;
        equipList = new Equipment[10];
        archetypeList = new HeroArchetypeData[2];
        //archetypeStatBonuses = new Dictionary<BonusType, StatBonus>();
        archetypeStatBonuses = new Dictionary<BonusType, StatBonusCollection>();
        attributeStatBonuses = new Dictionary<BonusType, StatBonus>();
        abilitySlotList = new List<AbilitySlot>() { new AbilitySlot(0), new AbilitySlot(1) };
    }

    public static HeroData CreateNewHero(string name, ArchetypeItem primaryArchetype, ArchetypeItem subArchetype = null)
    {
        HeroData hero = new HeroData(name);
        hero.archetypeList[0] = new HeroArchetypeData(primaryArchetype, hero);
        if (subArchetype != null)
            hero.archetypeList[1] = new HeroArchetypeData(subArchetype, hero);
        hero.LevelUp();
        return hero;
    }

    private void LevelUp()
    {
        if (Level == 100)
            return;
        Level++;
        ArchetypePoints++;
        BaseHealth += PrimaryArchetype.HealthGrowth;
        BaseSoulPoints += PrimaryArchetype.SoulPointGrowth;
        BaseStrength += PrimaryArchetype.StrengthGrowth;
        BaseIntelligence += PrimaryArchetype.IntelligenceGrowth;
        BaseAgility += PrimaryArchetype.AgilityGrowth;
        BaseWill += PrimaryArchetype.WillGrowth;

        if (SecondaryArchetype != null)
        {
            BaseHealth += SecondaryArchetype.HealthGrowth / 2;
            BaseSoulPoints += SecondaryArchetype.SoulPointGrowth / 2;
            BaseStrength += SecondaryArchetype.StrengthGrowth / 2;
            BaseIntelligence += SecondaryArchetype.IntelligenceGrowth / 2;
            BaseAgility += SecondaryArchetype.AgilityGrowth / 2;
            BaseWill += SecondaryArchetype.WillGrowth / 2;
        }

        UpdateActorData();
    }

    public void AddExperience(int experience)
    {
        if (Level >= 100)
            return;
        Experience += experience;
        while (Experience >= Helpers.GetRequiredExperience(Level + 1))
        {
            LevelUp();
            if (Level >= 100)
            {
                Experience = Helpers.GetRequiredExperience(100);
                break;
            }
        }
    }

    public bool EquipAbility(AbilityBase ability, int slot, IAbilitySource source)
    {
        if (slot >= 3)
            return false;

        if (abilitySlotList[slot] != null)
            UnequipAbility(slot);

        if (slot == 0)
        {
            if (abilitySlotList[1].Ability != null
                && abilitySlotList[1].Ability.abilityBase == ability && abilitySlotList[1].source == source)
            {
                UnequipAbility(1);
            }
            abilitySlotList[slot].SetAbilityToSlot(ability, source);
        }
        else
        {
            if (abilitySlotList[0].Ability != null
                && abilitySlotList[0].Ability.abilityBase == ability && abilitySlotList[0].source == source)
            {
                UnequipAbility(0);
            }
            abilitySlotList[slot].SetAbilityToSlot(ability, source);
        }
        source.OnAbilityEquip(ability, this, slot);
        UpdateActorData();
        return true;
    }

    public bool UnequipAbility(int slot)
    {
        if (slot >= 3)
            return false;
        if (abilitySlotList[slot].Ability == null)
            return false;
        IAbilitySource source = abilitySlotList[slot].source;
        source.OnAbilityUnequip(abilitySlotList[slot].Ability.abilityBase, this, slot);
        abilitySlotList[slot].ClearAbility();
        return true;
    }

    public ActorAbility GetAbilityFromSlot(int slot)
    {
        return abilitySlotList[slot].Ability;
    }

    public int GetAbilityLevel(AbilityBase ability)
    {
        return GetAbilityLevel();
    }

    public int GetAbilityLevel()
    {
        if (Level != 100)
            return (int)((Level - 1) / 2d);
        else
            return 50;
    }

    public Equipment GetEquipmentInSlot(EquipSlotType slot)
    {
        return equipList[(int)slot];
    }

    public bool EquipToSlot(Equipment equip, EquipSlotType slot)
    {
        if (equip.IsEquipped)
            return false;

        if (equip.Base.equipSlot == EquipSlotType.RING)
        {
            if (slot != EquipSlotType.RING_SLOT_1 && slot != EquipSlotType.RING_SLOT_2)
                return false;
        }
        else if (equip.Base.equipSlot == EquipSlotType.WEAPON)
        {
            if (slot == EquipSlotType.OFF_HAND)
            {
                if (equipList[(int)EquipSlotType.WEAPON] == null || equipList[(int)EquipSlotType.WEAPON].GetGroupTypes().Contains(GroupType.TWO_HANDED_WEAPON))
                    slot = EquipSlotType.WEAPON;
                else if ((equipList[(int)EquipSlotType.WEAPON].GetGroupTypes().Contains(GroupType.MELEE_WEAPON) && !equip.GetGroupTypes().Contains(GroupType.MELEE_WEAPON))
                         || (equipList[(int)EquipSlotType.WEAPON].GetGroupTypes().Contains(GroupType.RANGED_WEAPON) && !equip.GetGroupTypes().Contains(GroupType.RANGED_WEAPON)))
                    return false;
            }
            else if (equip.GetGroupTypes().Contains(GroupType.TWO_HANDED_WEAPON))
            {
                if (slot == EquipSlotType.WEAPON && equipList[(int)EquipSlotType.OFF_HAND] != null)
                    UnequipFromSlot(EquipSlotType.OFF_HAND);
                else if (slot == EquipSlotType.OFF_HAND)
                    slot = EquipSlotType.WEAPON;
            }
        }
        else if (equip.Base.equipSlot != slot)
            return false;

        if (equipList[(int)slot] != null)
            UnequipFromSlot(slot);

        switch (slot)
        {
            case EquipSlotType.HEADGEAR:
            case EquipSlotType.BODY_ARMOR:
            case EquipSlotType.BOOTS:
            case EquipSlotType.GLOVES:
            case EquipSlotType.BELT:
            case EquipSlotType.NECKLACE:
            case EquipSlotType.WEAPON:
            case EquipSlotType.OFF_HAND:
            case EquipSlotType.RING_SLOT_1:
            case EquipSlotType.RING_SLOT_2:
                equipList[(int)slot] = equip;
                break;

            default:
                return false;
        }
        equip.equippedToHero = this;

        ApplyEquipmentBonuses(equip.GetAllAffixes());

        if (GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon && GetEquipmentInSlot(EquipSlotType.OFF_HAND) is Weapon && !isDualWielding)
        {
            isDualWielding = true;
            AddStatBonus(BonusType.GLOBAL_ATTACK_SPEED, GroupType.DUAL_WIELD, ModifyType.MULTIPLY, DUAL_WIELD_ATTACK_SPEED_BONUS);
        }

        UpdateActorData();

        return true;
    }

    public bool ModifyArchetypePoints(int mod, bool force = false)
    {
        ArchetypePoints += mod;
        return true;
    }

    public bool UnequipFromSlot(EquipSlotType slot)
    {
        if (equipList[(int)slot] == null)
            return false;
        Equipment equip = equipList[(int)slot];
        equipList[(int)slot] = null;
        equip.equippedToHero = null;
        RemoveEquipmentBonuses(equip.GetAllAffixes());

        if (isDualWielding && !(GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon && GetEquipmentInSlot(EquipSlotType.OFF_HAND) is Weapon))
        {
            isDualWielding = false;
            RemoveStatBonus(BonusType.GLOBAL_ATTACK_SPEED, GroupType.DUAL_WIELD, ModifyType.MULTIPLY, DUAL_WIELD_ATTACK_SPEED_BONUS);
        }

        UpdateActorData();
        return true;
    }

    private void ApplyEquipmentBonuses(List<Affix> affixes)
    {
        foreach (Affix affix in affixes)
        {
            for (int i = 0; i < affix.Base.affixBonuses.Count; i++)
            {
                AffixBonusProperty b = affix.Base.affixBonuses[i];
                //ignore local mods
                if (b.bonusType >= (BonusType)0x700)
                    continue;
                else
                    AddStatBonus(b.bonusType, b.restriction, b.modifyType, affix.GetAffixValue(i));
            }
            for (int i = 0; i < affix.Base.triggeredEffects.Count; i++)
            {
                TriggeredEffectBonusProperty triggeredEffect = affix.Base.triggeredEffects[i];
                TriggeredEffect t = new TriggeredEffect(triggeredEffect, affix.GetEffectValue(i));
                AddTriggeredEffect(triggeredEffect, t);
            }
        }
    }

    public void AddTriggeredEffect(TriggeredEffectBonusProperty triggeredEffect, TriggeredEffect effectInstance)
    {
        switch (triggeredEffect.triggerType)
        {
            case TriggerType.ON_HIT:
                OnHitEffects.Add(effectInstance);
                break;

            case TriggerType.WHEN_HIT_BY:
                WhenHitEffects.Add(effectInstance);
                break;

            case TriggerType.WHEN_HITTING:
                WhenHittingEffects.Add(effectInstance);
                break;

            case TriggerType.ON_KILL:
                OnKillEffects.Add(effectInstance);
                break;

            case TriggerType.HEALTH_THRESHOLD:
                break;

            case TriggerType.SHIELD_THRESHOLD:
                break;

            case TriggerType.SOULPOINT_THRESHOLD:
                break;
        }
    }

    public void RemoveTriggeredEffect(TriggeredEffectBonusProperty triggeredEffect)
    {
        TriggeredEffect t;
        switch (triggeredEffect.triggerType)
        {
            case TriggerType.ON_HIT:
                t = OnHitEffects.Find(x => x.BaseEffect == triggeredEffect);
                OnHitEffects.Remove(t);
                break;

            case TriggerType.WHEN_HIT_BY:
                t = WhenHitEffects.Find(x => x.BaseEffect == triggeredEffect);
                WhenHitEffects.Remove(t);
                break;

            case TriggerType.WHEN_HITTING:
                t = WhenHittingEffects.Find(x => x.BaseEffect == triggeredEffect);
                WhenHittingEffects.Remove(t);
                break;

            case TriggerType.ON_KILL:
                t = OnKillEffects.Find(x => x.BaseEffect == triggeredEffect);
                OnKillEffects.Remove(t);
                break;

            case TriggerType.HEALTH_THRESHOLD:
                break;

            case TriggerType.SHIELD_THRESHOLD:
                break;

            case TriggerType.SOULPOINT_THRESHOLD:
                break;
        }
    }

    private void RemoveEquipmentBonuses(List<Affix> affixes)
    {
        foreach (Affix affix in affixes)
        {
            for (int i = 0; i < affix.Base.affixBonuses.Count; i++)
            {
                AffixBonusProperty b = affix.Base.affixBonuses[i];
                //ignore local mods
                if (b.bonusType < (BonusType)0x700)
                {
                    RemoveStatBonus(b.bonusType, b.restriction, b.modifyType, affix.GetAffixValue(i));
                }
            }
            for (int i = 0; i < affix.Base.triggeredEffects.Count; i++)
            {
                TriggeredEffectBonusProperty triggeredEffect = affix.Base.triggeredEffects[i];
                RemoveTriggeredEffect(triggeredEffect);
            }
        }
    }

    public void AddArchetypeStatBonus(BonusType type, GroupType restriction, ModifyType modifier, float value)
    {
        if (!archetypeStatBonuses.ContainsKey(type))
            archetypeStatBonuses[type] = new StatBonusCollection();
        archetypeStatBonuses[type].AddBonus(restriction, modifier, value);
    }

    public void RemoveArchetypeStatBonus(BonusType type, GroupType restriction, ModifyType modifier, float value)
    {
        if (!archetypeStatBonuses.ContainsKey(type))
            return;
        archetypeStatBonuses[type].RemoveBonus(restriction, modifier, value);
    }

    public override void UpdateActorData()
    {
        if (UpdatesAreDeferred)
            return;

        GroupTypes = GetGroupTypes();
        UpdateHeroAttributes();
        UpdateAbilities();
        UpdateDefenses();

        movementSpeed = GetMultiStatBonus(GroupTypes, BonusType.MOVEMENT_SPEED).CalculateStat(2.5f);

        base.UpdateActorData();
    }

    public void UpdateDefenses()
    {
        ApplyHealthBonuses();
        ApplySoulPointBonuses();
        CalculateDefenses();
        AttackPhasing = GetMultiStatBonus(GroupTypes, BonusType.ATTACK_PHASING).CalculateStat(BaseAttackPhasing);
        MagicPhasing = GetMultiStatBonus(GroupTypes, BonusType.MAGIC_PHASING).CalculateStat(BaseMagicPhasing);
    }

    private void UpdateHeroAttributes()
    {
        float finalBaseStrength = BaseStrength + PrimaryArchetype.AllocatedPoints * PrimaryArchetype.StrengthGrowth;
        float finalBaseIntelligence = BaseIntelligence + PrimaryArchetype.AllocatedPoints * PrimaryArchetype.IntelligenceGrowth;
        float finalBaseAgility = BaseAgility + PrimaryArchetype.AllocatedPoints * PrimaryArchetype.AgilityGrowth;
        float finalBaseWill = BaseWill + PrimaryArchetype.AllocatedPoints * PrimaryArchetype.WillGrowth;

        if (SecondaryArchetype != null)
        {
            finalBaseStrength += SecondaryArchetype.AllocatedPoints * SecondaryArchetype.StrengthGrowth;
            finalBaseIntelligence += SecondaryArchetype.AllocatedPoints * SecondaryArchetype.IntelligenceGrowth;
            finalBaseAgility += SecondaryArchetype.AllocatedPoints * SecondaryArchetype.AgilityGrowth;
            finalBaseWill += SecondaryArchetype.AllocatedPoints * SecondaryArchetype.WillGrowth;
        }

        Strength = (int)GetMultiStatBonus(GroupTypes, BonusType.STRENGTH).CalculateStat(finalBaseStrength);
        ApplyStrengthBonuses();

        Intelligence = (int)GetMultiStatBonus(GroupTypes, BonusType.INTELLIGENCE).CalculateStat(finalBaseIntelligence);
        ApplyIntelligenceBonuses();

        Agility = (int)GetMultiStatBonus(GroupTypes, BonusType.AGILITY).CalculateStat(finalBaseAgility);
        ApplyAgilityBonuses();

        Will = (int)GetMultiStatBonus(GroupTypes, BonusType.WILL).CalculateStat(finalBaseWill);
        ApplyWillBonuses();
    }

    private void ApplyStrengthBonuses()
    {
        /*
         * +1% Armor per 5 Str
         * +1% Attack Damage per 10 Str
         */
        int armorMod = (int)Math.Round(Strength / 5f, MidpointRounding.AwayFromZero);
        int attackDamageMod = (int)Math.Round(Strength / 10f, MidpointRounding.AwayFromZero);

        if (!attributeStatBonuses.ContainsKey(BonusType.GLOBAL_ARMOR))
            attributeStatBonuses.Add(BonusType.GLOBAL_ARMOR, new StatBonus());
        attributeStatBonuses[BonusType.GLOBAL_ARMOR].SetAdditive(armorMod);

        if (!attributeStatBonuses.ContainsKey(BonusType.ATTACK_DAMAGE))
            attributeStatBonuses.Add(BonusType.ATTACK_DAMAGE, new StatBonus());
        attributeStatBonuses[BonusType.ATTACK_DAMAGE].SetAdditive(attackDamageMod);
    }

    private void ApplyIntelligenceBonuses()
    {
        /*
         * +1% Shield per 5 Int
         * +1% Spell Damage per 10 Int
         */
        int shieldMod = (int)Math.Round(Intelligence / 5f, MidpointRounding.AwayFromZero);
        int spellDamageMod = (int)Math.Round(Intelligence / 10f, MidpointRounding.AwayFromZero);

        if (!attributeStatBonuses.ContainsKey(BonusType.GLOBAL_MAX_SHIELD))
            attributeStatBonuses.Add(BonusType.GLOBAL_MAX_SHIELD, new StatBonus());
        attributeStatBonuses[BonusType.GLOBAL_MAX_SHIELD].SetAdditive(shieldMod);

        if (!attributeStatBonuses.ContainsKey(BonusType.SPELL_DAMAGE))
            attributeStatBonuses.Add(BonusType.SPELL_DAMAGE, new StatBonus());
        attributeStatBonuses[BonusType.SPELL_DAMAGE].SetAdditive(spellDamageMod);
    }

    private void ApplyAgilityBonuses()
    {
        /*
         * +1% Dodge per 5 Agi
         * +1% Attack/Cast Speed per 20 Agi
         */
        int dodgeRatingMod = (int)Math.Round(Agility / 5f, MidpointRounding.AwayFromZero);
        int attackSpeedMod = (int)Math.Round(Agility / 20f, MidpointRounding.AwayFromZero);
        int castSpeedMod = (int)Math.Round(Agility / 20f, MidpointRounding.AwayFromZero);

        if (!attributeStatBonuses.ContainsKey(BonusType.GLOBAL_DODGE_RATING))
            attributeStatBonuses.Add(BonusType.GLOBAL_DODGE_RATING, new StatBonus());
        attributeStatBonuses[BonusType.GLOBAL_DODGE_RATING].SetAdditive(dodgeRatingMod);

        if (!attributeStatBonuses.ContainsKey(BonusType.GLOBAL_ATTACK_SPEED))
            attributeStatBonuses.Add(BonusType.GLOBAL_ATTACK_SPEED, new StatBonus());
        attributeStatBonuses[BonusType.GLOBAL_ATTACK_SPEED].SetAdditive(attackSpeedMod);

        if (!attributeStatBonuses.ContainsKey(BonusType.CAST_SPEED))
            attributeStatBonuses.Add(BonusType.CAST_SPEED, new StatBonus());
        attributeStatBonuses[BonusType.CAST_SPEED].SetAdditive(castSpeedMod);
    }

    private void ApplyWillBonuses()
    {
        /*
         * +1% Resolve per 5 Will
         * +1% Debuff Damage per 10 Will
         */
        int resolveRatingMod = (int)Math.Round(Will / 5f, MidpointRounding.AwayFromZero);
        int debuffDamageMod = (int)Math.Round(Will / 10f, MidpointRounding.AwayFromZero);

        BonusType bonus1 = BonusType.GLOBAL_RESOLVE_RATING;
        BonusType bonus2 = BonusType.STATUS_EFFECT_DAMAGE;

        if (!attributeStatBonuses.ContainsKey(bonus1))
            attributeStatBonuses.Add(bonus1, new StatBonus());
        attributeStatBonuses[bonus1].SetAdditive(resolveRatingMod);

        if (!attributeStatBonuses.ContainsKey(bonus2))
            attributeStatBonuses.Add(bonus2, new StatBonus());
        attributeStatBonuses[bonus2].SetAdditive(debuffDamageMod);
    }

    public void CalculateDefenses()
    {
        int ArmorFromEquip = 0;
        int ShieldFromEquip = 0;
        int DodgeFromEquip = 0;
        int ResolveFromEquip = 0;
        foreach (Equipment equippedItem in equipList)
        {
            if (equippedItem != null && equippedItem.GetItemType() == ItemType.ARMOR)
            {
                Armor equip = equippedItem as Armor;
                ArmorFromEquip += equip.armor;
                ShieldFromEquip += equip.shield;
                DodgeFromEquip += equip.dodgeRating;
                ResolveFromEquip += equip.resolveRating;
            }
        }

        Armor = GetMultiStatBonus(GroupTypes, BonusType.GLOBAL_ARMOR).CalculateStat(BaseArmor + ArmorFromEquip);
        MaximumManaShield = GetMultiStatBonus(GroupTypes, BonusType.GLOBAL_MAX_SHIELD).CalculateStat(BaseManaShield + ShieldFromEquip);
        if (MaximumManaShield != 0)
        {
            float shieldPercent = CurrentManaShield / MaximumManaShield;
            CurrentManaShield = MaximumManaShield * shieldPercent;
        }
        DodgeRating = GetMultiStatBonus(GroupTypes, BonusType.GLOBAL_DODGE_RATING).CalculateStat(BaseDodgeRating + DodgeFromEquip);
        ResolveRating = GetMultiStatBonus(GroupTypes, BonusType.GLOBAL_RESOLVE_RATING).CalculateStat(BaseResolveRating + ResolveFromEquip);

        int statusThreshold = (int)(ResolveRating / 5f / 100f);
        int statusResistance = (int)(ResolveRating / 15f / 100f);

        AfflictedStatusDamageResistance = 1 - GetMultiStatBonus(GroupTypes, BonusType.AFFLICTED_STATUS_DAMAGE_RESISTANCE).CalculateStat(statusResistance);
        AfflictedStatusThreshold = GetMultiStatBonus(GroupTypes, BonusType.AFFLICTED_STATUS_THRESHOLD).CalculateStat(1f + statusThreshold);
    }

    public override void GetTotalStatBonus(BonusType type, IEnumerable<GroupType> tags, Dictionary<BonusType, StatBonus> abilityBonusProperties, StatBonus inputStatBonus)
    {
        StatBonus resultBonus;
        if (inputStatBonus == null)
            resultBonus = new StatBonus();
        else
            resultBonus = inputStatBonus;

        List<StatBonus> bonuses = new List<StatBonus>();

        if (statBonuses.TryGetValue(type, out StatBonusCollection statBonus))
            bonuses.Add(statBonus.GetTotalStatBonus(tags));
        if (attributeStatBonuses.TryGetValue(type, out StatBonus attributeBonus))
            bonuses.Add(attributeBonus);
        if (archetypeStatBonuses.TryGetValue(type, out StatBonusCollection archetypeBonuses))
        {
            bonuses.Add(archetypeBonuses.GetTotalStatBonus(tags));
        }
        if (abilityBonusProperties != null && abilityBonusProperties.TryGetValue(type, out StatBonus abilityBonus))
            bonuses.Add(abilityBonus);
        if (temporaryBonuses.TryGetValue(type, out StatBonus temporaryBonus))
            bonuses.Add(temporaryBonus);

        if (bonuses.Count == 0)
        {
            return;
        }

        foreach (StatBonus bonus in bonuses)
        {
            if (bonus.HasFixedModifier)
            {
                resultBonus.AddBonus(ModifyType.FIXED_TO, bonus.FixedModifier);
                return;
            }
            resultBonus.AddBonus(ModifyType.FLAT_ADDITION, bonus.FlatModifier);
            resultBonus.AddBonus(ModifyType.ADDITIVE, bonus.AdditiveModifier);
            resultBonus.AddBonus(ModifyType.MULTIPLY, (bonus.CurrentMultiplier - 1f) * 100f);
        }

        return;
    }

    public void UpdateAbilities()
    {
        if (!GameManager.Instance.isInBattle)
            ClearTemporaryBonuses(false);

        foreach (AbilitySlot abilitySlot in abilitySlotList)
        {
            if (abilitySlot.Ability == null)
                continue;

            ActorAbility actorAbility = abilitySlot.Ability;
            if ((actorAbility.abilityBase.abilityType == AbilityType.SELF_BUFF || actorAbility.abilityBase.abilityType == AbilityType.AURA))
            {
                actorAbility.UpdateAbilityLevel(GetAbilityLevel(actorAbility.abilityBase));
                actorAbility.UpdateAbilityStats(this);
            }
        }

        foreach (AbilitySlot abilitySlot in abilitySlotList)
        {
            if (abilitySlot.Ability == null)
                continue;

            ActorAbility actorAbility = abilitySlot.Ability;

            if (actorAbility.abilityBase.abilityType == AbilityType.SELF_BUFF || actorAbility.abilityBase.abilityType == AbilityType.AURA)
                continue;

            actorAbility.UpdateAbilityLevel(GetAbilityLevel(actorAbility.abilityBase));
            actorAbility.UpdateAbilityStats(this);
        }
    }

    public override int GetResistance(ElementType element)
    {
        return ElementData[element];
    }

    protected override HashSet<GroupType> GetGroupTypes()
    {
        return GetGroupTypes(true);
    }

    public HashSet<GroupType> GetGroupTypes(bool includeWeapons)
    {
        HashSet<GroupType> types = new HashSet<GroupType>() { GroupType.NO_GROUP };

        foreach (Equipment equipment in equipList)
        {
            if (equipment == null)
            {
                continue;
            }
            if (!includeWeapons && equipment is Weapon)
                continue;
            types.UnionWith(equipment.GetGroupTypes());
        }

        if (GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon && GetEquipmentInSlot(EquipSlotType.OFF_HAND) is Weapon)
            types.Add(GroupType.DUAL_WIELD);

        if (CurrentActor != null)
        {
            types.UnionWith(CurrentActor.GetActorTags());
        }

        return types;
    }

    public void SaveAbilitySlotData(int slot, SaveData.HeroSaveData.HeroAbilitySlotSaveData data)
    {
        if (GetAbilityFromSlot(slot) == null)
            return;
        AbilitySlot abilitySlot = abilitySlotList[slot];
        data.abilityId = abilitySlot.Ability.abilityBase.idName;
        data.sourceId = abilitySlot.source.SourceId;
        data.sourceType = abilitySlot.source.AbilitySourceType;
    }

    private void FindAndEquipAbility(string abilityId, Guid sourceId, AbilitySourceType sourceType, int slot)
    {
        AbilityBase ability = ResourceManager.Instance.GetAbilityBase(abilityId);
        if (sourceType == AbilitySourceType.ARCHETYPE)
        {
            foreach (HeroArchetypeData archetypeData in archetypeList)
            {
                if (archetypeData == null || archetypeData.Id != sourceId || !archetypeData.ContainsAbility(ability))
                    continue;
                else
                {
                    EquipAbility(ability, slot, archetypeData);
                    return;
                }
            }
        }
        else if (sourceType == AbilitySourceType.ABILITY_CORE)
        {
            foreach (AbilityCoreItem abilityCore in GameManager.Instance.PlayerStats.AbilityInventory)
            {
                Debug.Log(abilityCore.SourceId + " " + sourceId);
                if (abilityCore.SourceId != sourceId)
                    continue;
                else
                {
                    Debug.Log("TEST");
                    EquipAbility(ability, slot, abilityCore);
                    return;
                }
            }
        }
    }

    public override HashSet<BonusType> BonusesIntersection(IEnumerable<BonusType> abilityBonuses, IEnumerable<BonusType> bonuses)
    {
        HashSet<BonusType> actorBonuses = new HashSet<BonusType>();
        actorBonuses.UnionWith(statBonuses.Keys);
        actorBonuses.UnionWith(temporaryBonuses.Keys);
        actorBonuses.UnionWith(archetypeStatBonuses.Keys);
        if (abilityBonuses != null)
            actorBonuses.UnionWith(abilityBonuses);
        actorBonuses.IntersectWith(bonuses);
        return actorBonuses;
    }

    private class AbilitySlot
    {
        public ActorAbility Ability { get; private set; }
        public IAbilitySource source;
        public readonly int slot;

        public AbilitySlot(int slot)
        {
            this.slot = slot;
        }

        public void SetAbilityToSlot(AbilityBase abilityBase, IAbilitySource source)
        {
            int layer;
            switch (abilityBase.targetType)
            {
                case AbilityTargetType.ENEMY:
                    layer = LayerMask.NameToLayer("EnemyDetect");
                    break;

                case AbilityTargetType.ALLY:
                    layer = LayerMask.NameToLayer("AllyDetect");
                    break;

                default:
                    layer = LayerMask.NameToLayer("BothDetect");
                    break;
            }

            Ability = new ActorAbility(abilityBase, layer);
            if (slot == 1)
            {
                Ability.SetAsSecondaryAbility();
            }
            this.source = source;
        }

        public void ClearAbility()
        {
            Ability = null;
            source = null;
        }
    }
}