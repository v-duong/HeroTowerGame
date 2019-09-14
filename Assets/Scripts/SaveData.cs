using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class SaveData
{
    [NonSerialized]
    private Dictionary<Guid, EquipSaveData> equipDict = new Dictionary<Guid, EquipSaveData>();

    [NonSerialized]
    private Dictionary<Guid, HeroSaveData> heroDict = new Dictionary<Guid, HeroSaveData>();

    public List<ConsumablesContainer> consumableList = new List<ConsumablesContainer>();
    public List<EquipSaveData> equipList = new List<EquipSaveData>();
    public List<AbilityCoreSaveData> abilityCoreList = new List<AbilityCoreSaveData>();
    public List<HeroSaveData> heroList = new List<HeroSaveData>();
    public int expStock;

    public void SaveAll()
    {
        SavePlayerData();
        SaveEquipmentData();
        SaveAbilityCoreData();
        SaveHeroData();
    }

    public void LoadAll()
    {
        LoadPlayerData();
        LoadEquipmentData();
        LoadAbilityCoreData();
        LoadHeroData();
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
    }

    public void LoadPlayerData()
    {
        PlayerStats ps = GameManager.Instance.PlayerStats;

        foreach (ConsumablesContainer c in consumableList)
            ps.consumables[c.consumable] = c.value;
        ps.SetExpStock(expStock);
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
            GameManager.Instance.PlayerStats.AddAbilityToInventory(new AbilityCoreItem(coreData.id, coreData.baseId, coreData.name));
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
                equipData.prefixes.Add(new AffixSaveData(affix.Base.idName, affix.GetAffixValues()));
            }

            equipData.suffixes.Clear();
            foreach (Affix affix in equipItem.suffixes)
            {
                equipData.suffixes.Add(new AffixSaveData(affix.Base.idName, affix.GetAffixValues()));
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
                    equipment.prefixes.Add(new Affix(affixBase, equipData.prefixes[i].affixValues));
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
            Affix newAffix = new Affix(ResourceManager.Instance.GetAffixBase(affixData.baseId, AffixType.PREFIX), affixData.affixValues);
            if (newAffix != null)
                equipment.prefixes.Add(newAffix);
        }
        foreach (AffixSaveData affixData in equipData.suffixes)
        {
            Affix newAffix = new Affix(ResourceManager.Instance.GetAffixBase(affixData.baseId, AffixType.SUFFIX), affixData.affixValues);
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
    public class EquipSaveData
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
    public class AffixSaveData
    {
        public string baseId;
        public List<float> affixValues = new List<float>();

        public AffixSaveData(string id, IList<float> values)
        {
            baseId = id;
            affixValues = values.ToList();
        }
    }

    [Serializable]
    public class AbilityCoreSaveData
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
    public class ConsumablesContainer
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