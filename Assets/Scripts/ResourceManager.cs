using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    private readonly static int PrefixOffset = 10000;
    private readonly static int SuffixOffset = 20000;
    private readonly static int EnchantmentOffset = 30000;
    private readonly static int InnateOffset = 40000;

    private Dictionary<int, AbilityBase> abilityList;
    private Dictionary<int, EquipmentBase> equipmentList;
    private Dictionary<int, AffixBase> prefixList;
    private Dictionary<int, AffixBase> suffixList;

    public int AbilityCount { get; private set; }
    public int EquipmentCount { get; private set; }
    public int PrefixCount { get; private set; }
    public int SuffixCount { get; private set; }

    public AbilityBase GetAbilityBase(int id)
    {
        if (abilityList == null)
            LoadAbilities();
        if (abilityList.ContainsKey(id))
            return abilityList[id];
        else
            return null;
    }

    public EquipmentBase GetEquipmentBase(int id)
    {
        if (equipmentList == null)
            LoadEquipment();
        if (equipmentList.ContainsKey(id))
            return equipmentList[id];
        else
            return null;
    }

    public AffixBase GetAffixBase(int id, AffixType type)
    {
        switch(type)
        {
            case AffixType.PREFIX:
                if (prefixList == null)
                    prefixList = LoadAffixes(type);
                return prefixList[id];
            case AffixType.SUFFIX:
                if (suffixList == null)
                    suffixList = LoadAffixes(type);
                return suffixList[id];
            default:
                return null;
        }
    }

    public AffixBase GetRandomAffixBase(AffixType type, int ilvl = 0, GroupType tag = GroupType.NO_GROUP, List<string> bonusTagList = null)
    {
        Dictionary<int, AffixBase> affixList;
        switch (type)
        {
            case AffixType.PREFIX:
                affixList = prefixList;
                break;
            case AffixType.SUFFIX:
                affixList = suffixList;
                break;
            default:
                affixList = null;
                break;
        }

        List<Helpers.WeightListItem<AffixBase>> possibleAffixList = new List<Helpers.WeightListItem<AffixBase>>();
        int sum = 0;

        foreach(AffixBase affixBase in affixList.Values)
        {
            if (bonusTagList != null && bonusTagList.Count > 0)
                if (bonusTagList.Contains(affixBase.BonusTagType))
                    continue;
            if (affixBase.spawnLevel <= ilvl)
            {
                foreach( AffixWeight affixWeight in affixBase.spawnWeight)
                {
                    if (tag == affixWeight.type || affixWeight.type == GroupType.NO_GROUP)
                    {
                        if (affixWeight.weight == 0)
                            break;
                        Debug.Log(affixBase.name + " " + affixWeight.type + " " + affixWeight.weight);
                        sum += affixWeight.weight;
                        possibleAffixList.Add(new Helpers.WeightListItem<AffixBase>(affixBase, affixWeight.weight));
                        continue;
                    }
                }
            }
        }
        if (possibleAffixList.Count == 0)
            return null;
        return Helpers.ReturnWeightedRandom<AffixBase>(possibleAffixList, sum);

    }

    private void LoadAbilities()
    {
        abilityList = new Dictionary<int, AbilityBase>();

        var j = Resources.Load<TextAsset>("json/abilities/abilities");
        List<AbilityBase> temp = JsonConvert.DeserializeObject<List<AbilityBase>>(j.text);
        foreach (AbilityBase ability in temp)
        {
            abilityList.Add(ability.id, ability);
        }
    }

    private void LoadEquipment()
    {
        equipmentList = new Dictionary<int, EquipmentBase>();

        var j = Resources.Load<TextAsset>("json/items/armor");
        List<EquipmentBase> temp = JsonConvert.DeserializeObject<List<EquipmentBase>>(j.text);
        foreach (EquipmentBase equip in temp)
        {
            equipmentList.Add(equip.id, equip);
        }
    }

    private Dictionary<int,AffixBase> LoadAffixes(AffixType type, int offset = 0)
    {
        string s;
        Dictionary<int, AffixBase>  affixes = new Dictionary<int, AffixBase>();

        switch(type)
        {
            case AffixType.PREFIX:
                s = "prefix";
                break;
            case AffixType.SUFFIX:
                s = "suffix";
                break;
            default:
                return null;
        }

        List<AffixBase> temp = DeserializeFromPath<List<AffixBase>>("json/affixes/" + s);
        foreach (AffixBase affix in temp)
        {
            affix.SetBonusTagType();
            affixes.Add(affix.id + offset, affix);
        }
        return affixes;
    }

    private T DeserializeFromPath<T>(string path)
    {
        return JsonConvert.DeserializeObject<T>(Resources.Load<TextAsset>( path ).text);
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Initialize()
    {
        LoadAbilities();
        LoadEquipment();
        prefixList = LoadAffixes(AffixType.PREFIX);
        suffixList = LoadAffixes(AffixType.SUFFIX);

        AbilityCount = abilityList.Count;
        EquipmentCount = equipmentList.Count;
        PrefixCount = prefixList.Count;
        SuffixCount = suffixList.Count;
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

}
