using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    //private readonly static int PrefixOffset = 10000;
    //private readonly static int SuffixOffset = 20000;
    //private readonly static int EnchantmentOffset = 30000;
    //private readonly static int InnateOffset = 40000;

    [SerializeField]
    private GameObject heroPrefab;

    [SerializeField]
    private GameObject enemyPrefab;

    [SerializeField]
    private GameObject abilityContainerPrefab;

    private Dictionary<string, AbilityBase> abilityList;
    private Dictionary<string, EquipmentBase> equipmentList;
    private Dictionary<string, AffixBase> prefixList;
    private Dictionary<string, AffixBase> suffixList;
    private Dictionary<string, AffixBase> innateList;
    private Dictionary<string, AffixBase> enchantmentList;
    private Dictionary<string, AffixBase> monsterModList;
    private Dictionary<string, ArchetypeBase> archetypeList;
    private Dictionary<string, EnemyBase> enemyList;
    private Dictionary<string, StageInfoBase> stageList;

    public int AbilityCount { get; private set; }
    public int EquipmentCount { get; private set; }
    public int PrefixCount { get; private set; }
    public int SuffixCount { get; private set; }
    public int ArchetypeCount { get; private set; }
    private AssetBundle jsonBundle;
    public GameObject HeroPrefab => heroPrefab;
    public GameObject EnemyPrefab => enemyPrefab;
    public GameObject AbilityContainerPrefab => abilityContainerPrefab;

    public AbilityBase GetAbilityBase(string id)
    {
        if (abilityList == null)
            LoadAbilities();
        if (abilityList.ContainsKey(id))
            return abilityList[id];
        else
            return null;
    }

    public ArchetypeBase GetArchetypeBase(string id)
    {
        if (archetypeList == null)
            LoadArchetypes();
        if (archetypeList.ContainsKey(id))
            return archetypeList[id];
        else
            return null;
    }

    public EquipmentBase GetEquipmentBase(string id)
    {
        if (equipmentList == null)
            LoadEquipment();
        if (equipmentList.ContainsKey(id))
            return equipmentList[id];
        else
            return null;
    }

    public StageInfoBase GetStageInfo(string id)
    {
        if (stageList == null)
            LoadStages();
        if (stageList.ContainsKey(id))
            return stageList[id];
        else
            return null;
    }

    public List<StageInfoBase> GetStagesByAct(int act)
    {
        if (stageList == null)
            LoadStages();
        List<StageInfoBase> returnList = new List<StageInfoBase>();
        returnList = stageList.Values.Where(x => x.act == act).ToList();
        return returnList;
    }

    public EnemyBase GetEnemyBase(string id)
    {
        if (enemyList == null)
            LoadEnemies();
        if (enemyList.ContainsKey(id))
            return enemyList[id];
        else
            return null;
    }

    public ArchetypeBase GetRandomArchetypeBase(int ilvl)
    {
        if (archetypeList == null)
            LoadArchetypes();

        WeightList<ArchetypeBase> possibleArchetypeList = new WeightList<ArchetypeBase>();

        foreach (ArchetypeBase archetype in archetypeList.Values)
        {
            if (archetype.dropLevel <= ilvl)
            {
                possibleArchetypeList.Add(archetype, archetype.spawnWeight);
            }
        }
        if (possibleArchetypeList.Count == 0)
            return null;
        return possibleArchetypeList.ReturnWeightedRandom();
    }

    public EquipmentBase GetRandomEquipmentBase(int ilvl, GroupType? group = null, EquipSlotType? slot = null)
    {
        if (equipmentList == null)
            LoadEquipment();

        WeightList<EquipmentBase> possibleEquipList = new WeightList<EquipmentBase>();

        foreach (EquipmentBase equipment in equipmentList.Values)
        {
            if (group != null && equipment.group != group)
                continue;

            if (slot != null && equipment.equipSlot != slot)
                continue;

            if (equipment.dropLevel <= ilvl)
            {
                possibleEquipList.Add(equipment, equipment.spawnWeight);
            }
        }
        if (possibleEquipList.Count == 0)
            return null;
        return possibleEquipList.ReturnWeightedRandom();
    }

    public AffixBase GetAffixBase(string id, AffixType type)
    {
        switch (type)
        {
            case AffixType.PREFIX:
                if (prefixList == null)
                    prefixList = LoadAffixes(type);
                return prefixList[id];

            case AffixType.SUFFIX:
                if (suffixList == null)
                    suffixList = LoadAffixes(type);
                return suffixList[id];

            case AffixType.INNATE:
                if (innateList == null)
                    innateList = LoadAffixes(type);
                return innateList[id];

            case AffixType.ENCHANTMENT:
                if (enchantmentList == null)
                    enchantmentList = LoadAffixes(type);
                return enchantmentList[id];

            case AffixType.MONSTERMOD:
                if (enchantmentList == null)
                    enchantmentList = LoadAffixes(type);
                return enchantmentList[id];

            default:
                return null;
        }
    }

    public AffixBase GetRandomAffixBase(AffixType type, int ilvl = 0, HashSet<GroupType> tags = null, List<string> bonusTagList = null)
    {
        Dictionary<string, AffixBase> affixList;

        switch (type)
        {
            case AffixType.PREFIX:
                affixList = prefixList;
                break;

            case AffixType.SUFFIX:
                affixList = suffixList;
                break;

            case AffixType.INNATE:
                affixList = innateList;
                break;

            case AffixType.ENCHANTMENT:
                affixList = enchantmentList;
                break;

            case AffixType.MONSTERMOD:
                affixList = monsterModList;
                break;

            default:
                affixList = null;
                break;
        }

        if (tags == null)
        {
            tags = new HashSet<GroupType>() { GroupType.NO_GROUP };
        }

        WeightList<AffixBase> possibleAffixList = new WeightList<AffixBase>();

        foreach (AffixBase affixBase in affixList.Values)
        {
            if (bonusTagList != null && bonusTagList.Count > 0)
                if (bonusTagList.Contains(affixBase.BonusTagType))
                    continue;
            if (affixBase.spawnLevel <= ilvl)
            {
                foreach (AffixWeight affixWeight in affixBase.spawnWeight)
                {
                    if (tags.Contains(affixWeight.type) || affixWeight.type == GroupType.NO_GROUP)
                    {
                        if (affixWeight.weight == 0)
                            break;
                        possibleAffixList.Add(affixBase, affixWeight.weight);
                        break;
                    }
                }
            }
        }
        if (possibleAffixList.Count == 0)
            return null;
        return possibleAffixList.ReturnWeightedRandom();
    }

    private void LoadAbilities()
    {
        abilityList = new Dictionary<string, AbilityBase>();

        List<AbilityBase> temp = DeserializeFromPath_Resources<List<AbilityBase>>("json/abilities/abilities");
        foreach (AbilityBase ability in temp)
        {
            abilityList.Add(ability.idName, ability);
        }
    }

    private void LoadEquipment()
    {
        equipmentList = new Dictionary<string, EquipmentBase>();

        List<EquipmentBase> temp = DeserializeFromPath_Resources<List<EquipmentBase>>("json/items/armor");
        foreach (EquipmentBase equip in temp)
        {
            equipmentList.Add(equip.idName, equip);
        }

        temp = DeserializeFromPath_Resources<List<EquipmentBase>>("json/items/weapon");
        foreach (EquipmentBase equip in temp)
        {
            equipmentList.Add(equip.idName, equip);
        }

        temp = DeserializeFromPath_Resources<List<EquipmentBase>>("json/items/accessory");
        foreach (EquipmentBase equip in temp)
        {
            equipmentList.Add(equip.idName, equip);
        }
    }

    private void LoadEnemies()
    {
        enemyList = new Dictionary<string, EnemyBase>();

        List<EnemyBase> temp = DeserializeFromPath_Resources<List<EnemyBase>>("json/enemies/enemies");
        foreach (EnemyBase enemy in temp)
        {
            enemyList.Add(enemy.idName, enemy);
        }
    }

    private void LoadArchetypes()
    {
        archetypeList = new Dictionary<string, ArchetypeBase>();

        List<ArchetypeBase> temp = DeserializeFromPath_Resources<List<ArchetypeBase>>("json/archetypes/archetypes");
        foreach (ArchetypeBase arche in temp)
        {
            archetypeList.Add(arche.idName, arche);
        }
    }

    private void LoadStages()
    {
        stageList = new Dictionary<string, StageInfoBase>();

        List<StageInfoBase> temp = DeserializeFromPath_Resources<List<StageInfoBase>>("json/stages/stages");
        foreach (StageInfoBase stage in temp)
        {
            string name = "stage" + stage.act + "-" + stage.stage + stage.difficulty;
            stageList.Add(name, stage);
        }
    }

    private Dictionary<string, AffixBase> LoadAffixes(AffixType type, int offset = 0)
    {
        string s;
        Dictionary<string, AffixBase> affixes = new Dictionary<string, AffixBase>();

        switch (type)
        {
            case AffixType.PREFIX:
                s = "prefix";
                break;

            case AffixType.SUFFIX:
                s = "suffix";
                break;

            case AffixType.INNATE:
                s = "innate";
                break;

            case AffixType.ENCHANTMENT:
                s = "enchantment";
                break;

            case AffixType.MONSTERMOD:
                s = "monstermod";
                break;

            default:
                return null;
        }

        List<AffixBase> temp = DeserializeFromPath_Resources<List<AffixBase>>("json/affixes/" + s);
        //List<AffixBase> temp = DeserializeFromPath_Bundle<List<AffixBase>>(s);
        foreach (AffixBase affix in temp)
        {
            affix.SetBonusTagType();
            affixes.Add(affix.idName, affix);
        }
        return affixes;
    }

    private T DeserializeFromPath_Resources<T>(string path)
    {
        return JsonConvert.DeserializeObject<T>(Resources.Load<TextAsset>(path).text);
    }

    private T DeserializeFromPath_Bundle<T>(string path)
    {
        return JsonConvert.DeserializeObject<T>(jsonBundle.LoadAsset<TextAsset>(path).text);
    }

    private void Awake()
    {
        Instance = this;
        Initialize();
    }

    private void Initialize()
    {
        //jsonBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath,"jsonfiles"));

        LoadAbilities();
        LoadEquipment();
        prefixList = LoadAffixes(AffixType.PREFIX);
        suffixList = LoadAffixes(AffixType.SUFFIX);
        innateList = LoadAffixes(AffixType.INNATE);
        enchantmentList = LoadAffixes(AffixType.ENCHANTMENT);
        monsterModList = LoadAffixes(AffixType.MONSTERMOD);
        LoadArchetypes();
        LoadEnemies();
        LoadStages();

        AbilityCount = abilityList.Count;
        EquipmentCount = equipmentList.Count;
        PrefixCount = prefixList.Count;
        SuffixCount = suffixList.Count;
        ArchetypeCount = archetypeList.Count;
    }
}