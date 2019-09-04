using System;
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

        foreach (EquipSlotType equipSlot in Enum.GetValues(typeof(EquipSlotType)))
        {
            if (equipSlot == EquipSlotType.RING)
                continue;

            if (heroSaveData.equipList[(int)equipSlot] != Guid.Empty)
            {
                EquipToSlot(ps.GetEquipmentByGuid(heroSaveData.equipList[(int)equipSlot]), equipSlot);
            }
        }

        archetypeList[0] = new HeroArchetypeData(heroSaveData.primaryArchetypeData, this);

        if (heroSaveData.secondaryArchetypeData != null)
        {
            archetypeList[1] = new HeroArchetypeData(heroSaveData.secondaryArchetypeData, this);
        }

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
        UpdateActorData();
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
                break;
        }
    }

    public bool EquipAbility(AbilityBase ability, int slot, IAbilitySource source)
    {
        if (slot >= 3)
            return false;
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
        source.OnEquip(ability, this, slot);
        UpdateAbilities();
        return true;
    }

    public bool UnequipAbility(int slot)
    {
        if (slot >= 3)
            return false;
        IAbilitySource source = abilitySlotList[slot].source;
        source.OnUnequip(abilitySlotList[slot].Ability.abilityBase, this, slot);
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
        ApplyEquipmentBonuses(equip.prefixes);
        ApplyEquipmentBonuses(equip.suffixes);
        ApplyEquipmentBonuses(equip.innate);

        if (GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon && GetEquipmentInSlot(EquipSlotType.OFF_HAND) is Weapon && !isDualWielding)
        {
            isDualWielding = true;
            AddStatBonus(BonusType.GLOBAL_ATTACK_SPEED, GroupType.DUAL_WIELD, ModifyType.MULTIPLY, 10f);
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
        RemoveEquipmentBonuses(equip.prefixes);
        RemoveEquipmentBonuses(equip.suffixes);
        RemoveEquipmentBonuses(equip.innate);

        if (isDualWielding && !(GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon && GetEquipmentInSlot(EquipSlotType.OFF_HAND) is Weapon))
        {
            isDualWielding = false;
            RemoveStatBonus(BonusType.GLOBAL_ATTACK_SPEED, GroupType.DUAL_WIELD, ModifyType.MULTIPLY, 10f);
        }

        UpdateActorData();
        return true;
    }

    private void ApplyEquipmentBonuses(List<Affix> affixes)
    {
        foreach (Affix affix in affixes)
        {
            foreach (AffixBonusProperty b in affix.Base.affixBonuses)
            {
                if (b.bonusType < (BonusType)0x700) //ignore local mods
                    AddStatBonus(b.bonusType, b.restriction, b.modifyType, affix.GetAffixValue(b.bonusType));
            }
        }
    }

    private void RemoveEquipmentBonuses(List<Affix> affixes)
    {
        foreach (Affix affix in affixes)
        {
            foreach (AffixBonusProperty b in affix.Base.affixBonuses)
            {
                if (b.bonusType < (BonusType)0x700) //ignore local mods
                    RemoveStatBonus(b.bonusType, b.restriction, b.modifyType, affix.GetAffixValue(b.bonusType));
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
        Strength = (int)GetMultiStatBonus(GroupTypes, BonusType.STRENGTH).CalculateStat(BaseStrength);
        ApplyStrengthBonuses();

        Intelligence = (int)GetMultiStatBonus(GroupTypes, BonusType.INTELLIGENCE).CalculateStat(BaseIntelligence);
        ApplyIntelligenceBonuses();

        Agility = (int)GetMultiStatBonus(GroupTypes, BonusType.AGILITY).CalculateStat(BaseAgility);
        ApplyAgilityBonuses();

        Will = (int)GetMultiStatBonus(GroupTypes, BonusType.WILL).CalculateStat(BaseWill);
        ApplyWillBonuses();
    }

    private void ApplyStrengthBonuses()
    {
        /*
         * +1% Armor per 5 Str
         * +1% Attack Damage per 10 Str
         */
        int armorMod = (int)Math.Round(Strength / 5d, MidpointRounding.AwayFromZero);
        int attackDamageMod = (int)Math.Round(Strength / 10d, MidpointRounding.AwayFromZero);

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
        int shieldMod = (int)Math.Round(Intelligence / 5d, MidpointRounding.AwayFromZero);
        int spellDamageMod = (int)Math.Round(Intelligence / 10d, MidpointRounding.AwayFromZero);

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
        int dodgeRatingMod = (int)Math.Round(Agility / 5d, MidpointRounding.AwayFromZero);
        int attackSpeedMod = (int)Math.Round(Agility / 20d, MidpointRounding.AwayFromZero);
        int castSpeedMod = (int)Math.Round(Agility / 20d, MidpointRounding.AwayFromZero);

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
        int resolveRatingMod = (int)Math.Round(Will / 5d, MidpointRounding.AwayFromZero);
        int debuffDamageMod = (int)Math.Round(Will / 10d, MidpointRounding.AwayFromZero);

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

        AfflictedStatusDamageResistance = GetMultiStatBonus(GroupTypes, BonusType.AFFLICTED_STATUS_DAMAGE_RESISTANCE).CalculateStat(1f + statusResistance);
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
        foreach (AbilitySlot abilitySlot in abilitySlotList)
        {
            ActorAbility actorAbility = abilitySlot.Ability;
            if (actorAbility != null)
            {
                actorAbility.UpdateAbilityLevel(GetAbilityLevel(actorAbility.abilityBase));
                actorAbility.UpdateAbilityStats(this);
            }
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

    private class AbilitySlot
    {
        public ActorAbility Ability { get; private set; }
        public IAbilitySource source;
        private AbilitySourceType sourceType;
        public readonly int slot;

        public AbilitySlot(int slot)
        {
            this.slot = slot;
        }

        public void SetAbilityToSlot(AbilityBase abilityBase, IAbilitySource source)
        {
            int layer;
            if (abilityBase.targetType == AbilityTargetType.ENEMY)
            {
                layer = LayerMask.NameToLayer("EnemyDetect");
            }
            else if (abilityBase.targetType == AbilityTargetType.ALLY)
            {
                layer = LayerMask.NameToLayer("AllyDetect");
            }
            else
            {
                layer = LayerMask.NameToLayer("BothDetect");
            }

            Ability = new ActorAbility(abilityBase, layer);
            if (slot == 1)
            {
                Ability.SetAsSecondaryAbility();
            }
            this.source = source;
            if (source.GetType() == typeof(HeroArchetypeData))
                sourceType = AbilitySourceType.ARCHETYPE;
        }

        public void ClearAbility()
        {
            Ability = null;
            source = null;
        }
    }
}