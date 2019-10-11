using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SaveData
{
    [NonSerialized]
    private Dictionary<Guid, EquipSaveData> equipDict = new Dictionary<Guid, EquipSaveData>();

    [NonSerialized]
    private Dictionary<Guid, HeroSaveData> heroDict = new Dictionary<Guid, HeroSaveData>();

    private readonly List<ConsumablesContainer> consumableList = new List<ConsumablesContainer>();
    private readonly List<EquipSaveData> equipList = new List<EquipSaveData>();
    private readonly List<AbilityCoreSaveData> abilityCoreList = new List<AbilityCoreSaveData>();
    private readonly List<HeroSaveData> heroList = new List<HeroSaveData>();
    private readonly List<ArchetypeItemSaveData> archetypeItemList = new List<ArchetypeItemSaveData>();
    private readonly Guid[][] heroTeamList = new Guid[PlayerStats.HERO_TEAM_MAX_NUM][];
    public int expStock;

    public void SaveAll()
    {
        SaveEquipmentData();
        SaveAbilityCoreData();
        SaveArchetypeItemData();
        SaveHeroData();
        SavePlayerData();
    }

    public void LoadAll()
    {
        LoadEquipmentData();
        LoadAbilityCoreData();
        LoadArchetypeItemData();
        LoadHeroData();
        LoadPlayerData();
    }

    public void SavePlayerData()
    {
        PlayerStats ps = GameManager.Instance.PlayerStats;

        foreach (KeyValuePair<ConsumableType, int> pair in ps.consumables)
        {
            ConsumablesContainer c = consumableList.Find(x => x.consumable == pair.Key);
            if (c != null)
                c.value = pair.Value;
            else
                consumableList.Add(new ConsumablesContainer(pair.Key, pair.Value));
        }

        expStock = ps.ExpStock;

        Array.Clear(heroTeamList, 0, PlayerStats.HERO_TEAM_MAX_NUM);
        for (int i = 0; i < PlayerStats.HERO_TEAM_MAX_NUM; i++)
        {
            heroTeamList[i] = new Guid[PlayerStats.HERO_TEAM_MAX_NUM];
            for (int j = 0; j < PlayerStats.HERO_TEAM_MAX_HEROES; j++)
            {
                if (ps.heroTeams[i][j] != null)
                    heroTeamList[i][j] = ps.heroTeams[i][j].Id;
                else
                    heroTeamList[i][j] = Guid.Empty;
            }
        }
    }

    public void LoadPlayerData()
    {
        PlayerStats ps = GameManager.Instance.PlayerStats;

        foreach (ConsumablesContainer c in consumableList)
            ps.consumables[c.consumable] = c.value;
        ps.SetExpStock(expStock);

        if (heroTeamList != null)

        for (int i = 0; i < PlayerStats.HERO_TEAM_MAX_NUM; i++)
        {
            if (heroTeamList[i] != null)
            {
                for (int j = 0; j < PlayerStats.HERO_TEAM_MAX_HEROES; j++)
                {
                    List<HeroData> heroes = ps.HeroList.ToList();
                    if (heroTeamList[i][j] != Guid.Empty)
                    {
                        HeroData hero = heroes.Find(x => x.Id == heroTeamList[i][j]);
                        if (hero != null)
                        {
                            ps.heroTeams[i][j] = hero;
                            hero.assignedTeam = i;
                            continue;
                        }
                    }
                    ps.heroTeams[i][j] = null;
                }
            }
        }
    }

    public void SaveAbilityCoreData()
    {
        abilityCoreList.Clear();
        foreach (AbilityCoreItem abilityCore in GameManager.Instance.PlayerStats.AbilityInventory)
        {
            abilityCoreList.Add(new AbilityCoreSaveData(abilityCore.Id, abilityCore.Base.idName, abilityCore.Name));
        }
    }

    public void LoadAbilityCoreData()
    {
        GameManager.Instance.PlayerStats.ClearAbilityInventory();
        foreach (AbilityCoreSaveData coreData in abilityCoreList)
        {
            AbilityBase abilityBase = ResourceManager.Instance.GetAbilityBase(coreData.baseId);
            if (abilityBase == null)
                continue;
            GameManager.Instance.PlayerStats.AddAbilityToInventory(new AbilityCoreItem(coreData.id, abilityBase, coreData.name));
        }
    }

    public void SaveArchetypeItemData()
    {
        archetypeItemList.Clear();
        foreach (ArchetypeItem archetypeItem in GameManager.Instance.PlayerStats.ArchetypeInventory)
        {
            archetypeItemList.Add(new ArchetypeItemSaveData(archetypeItem.Id, archetypeItem.Base.idName));
        }
    }

    public void LoadArchetypeItemData()
    {
        GameManager.Instance.PlayerStats.ClearArchetypeItemInventory();
        foreach (ArchetypeItemSaveData archetypeData in archetypeItemList)
        {
            GameManager.Instance.PlayerStats.AddArchetypeToInventory(new ArchetypeItem(archetypeData.id, archetypeData.baseId));
        }
    }

    public void SaveEquipmentData()
    {
        foreach (Equipment equipItem in GameManager.Instance.PlayerStats.EquipmentInventory)
        {
            EquipSaveData equipData;
            if (!equipDict.ContainsKey(equipItem.Id))
            {
                equipData = new EquipSaveData
                {
                    id = equipItem.Id,
                    baseId = equipItem.Base.idName,
                    ilvl = (byte)equipItem.ItemLevel
                };
                equipDict[equipItem.Id] = equipData;
                equipList.Add(equipData);
            }
            else
                equipData = equipDict[equipItem.Id];

            equipData.name = equipItem.Name;
            equipData.rarity = equipItem.Rarity;
            if (equipItem.Rarity == RarityType.UNIQUE)
            {
                UniqueBase uniqueBase = equipItem.Base as UniqueBase;
                equipData.uniqueVersion = (byte)uniqueBase.uniqueVersion;
            }
            equipData.prefixes.Clear();
            foreach (Affix affix in equipItem.prefixes)
            {
                equipData.prefixes.Add(new AffixSaveData(affix.Base.idName, affix.GetAffixValues(), affix.GetEffectValues(), affix.IsCrafted));
            }

            equipData.suffixes.Clear();
            foreach (Affix affix in equipItem.suffixes)
            {
                equipData.suffixes.Add(new AffixSaveData(affix.Base.idName, affix.GetAffixValues(), affix.GetEffectValues(), affix.IsCrafted));
            }
        }
    }

    public void LoadEquipmentData()
    {
        GameManager.Instance.PlayerStats.ClearEquipmentInventory();
        foreach (EquipSaveData equipData in equipList)
        {
            Equipment equipment = null;
            if (equipData.rarity == RarityType.UNIQUE)
                equipment = Equipment.CreateUniqueFromBase(equipData.baseId, equipData.ilvl, equipData.uniqueVersion);
            else
                equipment = Equipment.CreateEquipmentFromBase(equipData.baseId, equipData.ilvl);

            if (equipment == null)
                continue;

            GameManager.Instance.PlayerStats.AddEquipmentToInventory(equipment);

            equipment.SetId(equipData.id);
            equipment.Name = equipData.name;
            equipment.SetRarity(equipData.rarity);

            if (equipData.rarity == RarityType.UNIQUE)
            {
                UniqueBase uniqueBase = equipment.Base as UniqueBase;
                for (int i = 0; i < uniqueBase.fixedUniqueAffixes.Count; i++)
                {
                    AffixBase affixBase = uniqueBase.fixedUniqueAffixes[i];
                    equipment.prefixes.Add(new Affix(affixBase, equipData.prefixes[i].affixValues, equipData.prefixes[i].triggerValues, false));
                }
            }
            else
                LoadEquipmentAffixes(equipData, equipment);

            equipment.UpdateItemStats();
        }
    }

    private static void LoadEquipmentAffixes(EquipSaveData equipData, Equipment equipment)
    {
        foreach (AffixSaveData affixData in equipData.prefixes)
        {
            Affix newAffix = new Affix(ResourceManager.Instance.GetAffixBase(affixData.baseId, AffixType.PREFIX), affixData.affixValues, affixData.triggerValues, affixData.isCrafted);
            if (newAffix != null)
                equipment.prefixes.Add(newAffix);
        }
        foreach (AffixSaveData affixData in equipData.suffixes)
        {
            Affix newAffix = new Affix(ResourceManager.Instance.GetAffixBase(affixData.baseId, AffixType.SUFFIX), affixData.affixValues, affixData.triggerValues, affixData.isCrafted);
            if (newAffix != null)
                equipment.suffixes.Add(newAffix);
        }
    }

    public void SaveHeroData()
    {
        foreach (HeroData hero in GameManager.Instance.PlayerStats.HeroList)
        {
            HeroSaveData heroSaveData;
            if (!heroDict.ContainsKey(hero.Id))
            {
                heroSaveData = new HeroSaveData
                {
                    id = hero.Id
                };
                heroDict[hero.Id] = heroSaveData;
                heroList.Add(heroSaveData);
            }
            else
                heroSaveData = heroDict[hero.Id];

            heroSaveData.name = hero.Name;
            heroSaveData.level = hero.Level;
            heroSaveData.experience = hero.Experience;
            heroSaveData.baseHealth = hero.BaseHealth;
            heroSaveData.baseSoulPoints = hero.BaseSoulPoints;
            heroSaveData.baseStrength = hero.BaseStrength;
            heroSaveData.baseIntelligence = hero.BaseIntelligence;
            heroSaveData.baseAgility = hero.BaseAgility;
            heroSaveData.baseWill = hero.BaseWill;

            foreach (EquipSlotType equipSlot in Enum.GetValues(typeof(EquipSlotType)))
            {
                if (equipSlot == EquipSlotType.RING)
                    continue;

                if (hero.GetEquipmentInSlot(equipSlot) != null)
                {
                    heroSaveData.equipList[(int)equipSlot] = hero.GetEquipmentInSlot(equipSlot).Id;
                }
                else
                {
                    heroSaveData.equipList[(int)equipSlot] = Guid.Empty;
                }
            }

            if (hero.PrimaryArchetype != null)
            {
                HeroArchetypeData archetypeData = hero.PrimaryArchetype;
                HeroArchetypeSaveData archetypeSaveData = new HeroArchetypeSaveData()
                {
                    id = archetypeData.Id,
                    archetypeId = archetypeData.Base.idName
                };
                archetypeSaveData.nodeLevelData = new List<HeroArchetypeSaveData.NodeLevelSaveData>();
                foreach (KeyValuePair<int, int> pair in archetypeData.NodeLevels)
                {
                    archetypeSaveData.nodeLevelData.Add(new HeroArchetypeSaveData.NodeLevelSaveData(pair.Key, pair.Value));
                }
                heroSaveData.primaryArchetypeData = archetypeSaveData;
            }

            if (hero.SecondaryArchetype != null)
            {
                HeroArchetypeData archetypeData = hero.SecondaryArchetype;
                HeroArchetypeSaveData archetypeSaveData = new HeroArchetypeSaveData()
                {
                    id = archetypeData.Id,
                    archetypeId = archetypeData.Base.idName
                };
                archetypeSaveData.nodeLevelData = new List<HeroArchetypeSaveData.NodeLevelSaveData>();
                foreach (KeyValuePair<int, int> pair in archetypeData.NodeLevels)
                {
                    archetypeSaveData.nodeLevelData.Add(new HeroArchetypeSaveData.NodeLevelSaveData(pair.Key, pair.Value));
                }
                heroSaveData.secondaryArchetypeData = archetypeSaveData;
            }

            if (hero.GetAbilityFromSlot(0) != null)
            {
                if (heroSaveData.firstAbilitySlot == null)
                    heroSaveData.firstAbilitySlot = new HeroSaveData.HeroAbilitySlotSaveData();
                hero.SaveAbilitySlotData(0, heroSaveData.firstAbilitySlot);
            }
            else
            {
                heroSaveData.firstAbilitySlot = null;
            }

            if (hero.GetAbilityFromSlot(1) != null)
            {
                if (heroSaveData.secondAbilitySlot == null)
                    heroSaveData.secondAbilitySlot = new HeroSaveData.HeroAbilitySlotSaveData();
                hero.SaveAbilitySlotData(1, heroSaveData.secondAbilitySlot);
            }
            else
            {
                heroSaveData.secondAbilitySlot = null;
            }
        }
    }

    public void LoadHeroData()
    {
        GameManager.Instance.PlayerStats.ClearHeroList();
        if (heroDict == null)
            heroDict = new Dictionary<Guid, HeroSaveData>();
        else
            heroDict.Clear();
        foreach (HeroSaveData heroSaveData in heroList)
        {
            GameManager.Instance.PlayerStats.AddHeroToList(new HeroData(heroSaveData));
        }
    }

    [Serializable]
    public class HeroSaveData
    {
        public Guid id;
        public string name;
        public int level;
        public int experience;
        public float baseHealth;
        public float baseSoulPoints;
        public float baseStrength;
        public float baseIntelligence;
        public float baseAgility;
        public float baseWill;

        public Guid[] equipList = new Guid[10];

        public HeroArchetypeSaveData primaryArchetypeData;
        public HeroArchetypeSaveData secondaryArchetypeData;

        public HeroAbilitySlotSaveData firstAbilitySlot;
        public HeroAbilitySlotSaveData secondAbilitySlot;

        [Serializable]
        public class HeroAbilitySlotSaveData
        {
            public Guid sourceId;
            public string abilityId;
            public AbilitySourceType sourceType;
        }
    }

    [Serializable]
    public class HeroArchetypeSaveData
    {
        public Guid id;
        public string archetypeId;
        public List<NodeLevelSaveData> nodeLevelData = new List<NodeLevelSaveData>();

        [Serializable]
        public class NodeLevelSaveData
        {
            public int nodeId;
            public int level;

            public NodeLevelSaveData(int nodeId, int level)
            {
                this.nodeId = nodeId;
                this.level = level;
            }
        }
    }

    [Serializable]
    private class EquipSaveData
    {
        public Guid id;
        public string name;
        public string baseId;
        public byte ilvl;
        public byte uniqueVersion;
        public RarityType rarity;
        public List<AffixSaveData> prefixes = new List<AffixSaveData>();
        public List<AffixSaveData> suffixes = new List<AffixSaveData>();
    }

    [Serializable]
    private class AffixSaveData
    {
        public string baseId;
        public bool isCrafted;
        public List<float> affixValues = new List<float>();
        public List<float> triggerValues = new List<float>();

        public AffixSaveData(string id, IList<float> values, IList<float> triggervalues, bool isCrafted)
        {
            baseId = id;
            affixValues = values.ToList();
            this.isCrafted = isCrafted;
            if (triggervalues != null)
                triggerValues = triggervalues.ToList();
        }
    }

    [Serializable]
    private class AbilityCoreSaveData
    {
        public Guid id;
        public string baseId;
        public string name;

        public AbilityCoreSaveData(Guid id, string baseId, string name)
        {
            this.id = id;
            this.baseId = baseId ?? throw new ArgumentNullException(nameof(baseId));
            this.name = name ?? throw new ArgumentNullException(nameof(name));
        }
    }

    [Serializable]
    private class ArchetypeItemSaveData
    {
        public Guid id;
        public string baseId;

        public ArchetypeItemSaveData(Guid id, string baseId)
        {
            this.id = id;
            this.baseId = baseId ?? throw new ArgumentNullException(nameof(baseId));
        }
    }

    [Serializable]
    private class ConsumablesContainer
    {
        public ConsumableType consumable;
        public int value;

        public ConsumablesContainer(ConsumableType type, int quantity)
        {
            consumable = type;
            value = quantity;
        }
    }
}