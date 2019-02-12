using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    private Dictionary<int, AbilityBase> abilities;
    private Dictionary<int, EquipmentBase> equipment;
    private Dictionary<int, Affix> prefixes;
    private Dictionary<int, Affix> suffixes;

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

    private void Awake()
    {
        Instance = this;
    }

    private void Initialize()
    {
        LoadAbilities();
        LoadEquipment();
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
