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

    private Dictionary<int, AbilityBase> abilities;
    private Dictionary<int, EquipmentBase> equipment;
    private Dictionary<int, AffixBase> prefixes;
    private Dictionary<int, AffixBase> suffixes;

    public AbilityBase GetAbilityBase(int id)
    {
        if (abilities == null)
            LoadAbilities();
        if (abilities.ContainsKey(id))
            return abilities[id];
        else
            return null;
    }

    public EquipmentBase GetEquipmentBase(int id)
    {
        if (equipment == null)
            LoadEquipment();
        if (equipment.ContainsKey(id))
            return equipment[id];
        else
            return null;
    }

    public AffixBase GetAffixBase(int id, AffixType type)
    {
        switch(type)
        {
            case AffixType.PREFIX:
                if (prefixes == null)
                    prefixes = LoadAffixes(type);
                return prefixes[id];
            case AffixType.SUFFIX:
                if (suffixes == null)
                    suffixes = LoadAffixes(type);
                return suffixes[id];
            default:
                return null;
        }
    }

    private int LoadAbilities()
    {
        abilities = new Dictionary<int, AbilityBase>();

        var j = Resources.Load<TextAsset>("json/abilities/abilities");
        List<AbilityBase> temp = JsonConvert.DeserializeObject<List<AbilityBase>>(j.text);
        foreach (AbilityBase ability in temp)
        {
            abilities.Add(ability.id, ability);
        }
        return abilities.Count;
    }

    private int LoadEquipment()
    {
        equipment = new Dictionary<int, EquipmentBase>();

        var j = Resources.Load<TextAsset>("json/items/item_bases_body");
        List<EquipmentBase> temp = JsonConvert.DeserializeObject<List<EquipmentBase>>(j.text);
        foreach (EquipmentBase equip in temp)
        {
            equipment.Add(equip.id, equip);
        }
        return equipment.Count;
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
        prefixes = LoadAffixes(AffixType.PREFIX);
        suffixes = LoadAffixes(AffixType.SUFFIX);
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
