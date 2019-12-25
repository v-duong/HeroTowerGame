using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

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
    private EnemyActor enemyPrefab;

    [SerializeField]
    private GameObject abilityContainerPrefab;

    [SerializeField]
    private FloatingDamageText damageTextPrefab;

    [SerializeField]
    private TargetingCircle targetingCirclePrefab;

    [SerializeField]
    private SpriteAtlas heroesSpriteAtlas;

    private Dictionary<string, AbilityBase> abilityList;
    private Dictionary<string, EquipmentBase> equipmentList;
    private Dictionary<string, UniqueBase> uniqueList;
    private Dictionary<string, AffixBase> prefixList;
    private Dictionary<string, AffixBase> suffixList;
    private Dictionary<string, AffixBase> innateList;
    private Dictionary<string, AffixBase> enchantmentList;
    private Dictionary<string, AffixBase> monsterModList;
    private Dictionary<string, ArchetypeBase> archetypeList;
    private Dictionary<string, EnemyBase> enemyList;
    private Dictionary<string, StageInfoBase> stageList;

    private Dictionary<string, Sprite> abilitySpriteList;
    private Dictionary<string, Sprite> enemySpriteList;
    private Dictionary<string, SpriteAtlas> loadedSpriteAtlases;

    private Sprite[] heroSprites;

    public List<ArchetypeBase> ArchetypeBasesList => archetypeList.Values.ToList();

    public int AbilityCount { get; private set; }
    public int EquipmentCount { get; private set; }
    public int PrefixCount { get; private set; }
    public int SuffixCount { get; private set; }
    public int ArchetypeCount { get; private set; }
    public GameObject HeroPrefab => heroPrefab;
    public EnemyActor EnemyPrefab => enemyPrefab;
    public GameObject AbilityContainerPrefab => abilityContainerPrefab;
    public FloatingDamageText DamageTextPrefab => damageTextPrefab;
    public TargetingCircle TargetingCirclePrefab => targetingCirclePrefab;

    private AssetBundle jsonBundle;
    private AssetBundle particles;

    public MaterialPropertyBlock normalMaterialBlock;
    public MaterialPropertyBlock uncommonMaterialBlock;
    public MaterialPropertyBlock rareMaterialBlock;
    public MaterialPropertyBlock bossMaterialBlock;

    public AbilityBase GetAbilityBase(string id)
    {
        if (abilityList == null)
            LoadAbilities();
        if (id == null)
            return null;
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

    public UniqueBase GetUniqueBase(string id)
    {
        if (uniqueList == null)
            LoadUniques();
        if (uniqueList.ContainsKey(id))
            return uniqueList[id];
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

    public UniqueBase GetRandomUniqueBase(int ilvl, GroupType? group = null, EquipSlotType? slot = null)
    {
        if (uniqueList == null)
            LoadUniques();

        WeightList<UniqueBase> possibleUniqueList = new WeightList<UniqueBase>();

        foreach (UniqueBase unique in uniqueList.Values)
        {
            if (group != null && unique.group != group)
                continue;

            if (slot != null && unique.equipSlot != slot)
                continue;

            if (unique.dropLevel <= ilvl)
            {
                possibleUniqueList.Add(unique, unique.spawnWeight);
            }
        }
        if (possibleUniqueList.Count == 0)
            return null;
        return possibleUniqueList.ReturnWeightedRandom();
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
                if (monsterModList == null)
                    monsterModList = LoadAffixes(type);
                return monsterModList[id];

            default:
                return null;
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="type">Affix Category to Generate</param>
    /// <param name="ilvl">Level of Target</param>
    /// <param name="targetTypeTags">List of target's tags to determine base spawn weights</param>
    /// <param name="affixBonusTypeStrings">List of strings to determine which affixes have already been applied to target</param>
    /// <param name="weightModifiers">Values to multiply base spawn weight by if they match the group tags</param>
    /// <returns></returns>
    public AffixBase GetRandomAffixBase(AffixType type, int ilvl, HashSet<GroupType> targetTypeTags, List<string> affixBonusTypeStrings, Dictionary<GroupType, float> weightModifiers = null, float affixLevelSkewFactor = 1f)
    {
        WeightList<AffixBase> possibleAffixList = GetPossibleAffixes(type, ilvl, targetTypeTags, affixBonusTypeStrings, weightModifiers, affixLevelSkewFactor);
        if (possibleAffixList.Count == 0)
            return null;
        return possibleAffixList.ReturnWeightedRandom();
    }

    public WeightList<AffixBase> GetPossibleAffixes(AffixType type, int ilvl, HashSet<GroupType> targetTypeTags, List<string> affixBonusTypeStrings, Dictionary<GroupType, float> weightModifiers, float affixLevelSkewFactor)
    {
        if (targetTypeTags == null)
        {
            targetTypeTags = new HashSet<GroupType>() { GroupType.NO_GROUP };
        }
        if (weightModifiers == null)
        {
            weightModifiers = new Dictionary<GroupType, float>();
        }

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

        WeightList<AffixBase> possibleAffixList = new WeightList<AffixBase>();

        foreach (AffixBase affixBase in affixList.Values)
        {
            // check if affix type has already been applied to target
            if (affixBonusTypeStrings != null && affixBonusTypeStrings.Count > 0)
                if (affixBonusTypeStrings.Contains(affixBase.AffixBonusTypeString))
                    continue;

            //check if affix is drop only
            if (affixBase.groupTypes.Contains(GroupType.DROP_ONLY) && !targetTypeTags.Contains(GroupType.DROP_ONLY))
                continue;

            float weightMultiplier = 1.0f;
            int baseWeight = 0;

            if (affixBase.spawnLevel <= ilvl)
            {
                foreach (AffixWeight affixWeight in affixBase.spawnWeight)
                {
                    if (targetTypeTags.Contains(affixWeight.type) || affixWeight.type == GroupType.NO_GROUP)
                    {
                        baseWeight = affixWeight.weight;
                        break;
                    }
                }
                if (baseWeight > 0)
                {
                    foreach (GroupType groupTag in affixBase.groupTypes)
                    {
                        if (weightModifiers.ContainsKey(groupTag))
                        {
                            weightMultiplier *= weightModifiers[groupTag];
                        }
                    }

                    if (affixLevelSkewFactor != 1f)
                        weightMultiplier *= Mathf.Lerp(1 / affixLevelSkewFactor, affixLevelSkewFactor, affixBase.spawnLevel / ilvl);

                    if (weightMultiplier == 0)
                        continue;
                    possibleAffixList.Add(affixBase, (int)(baseWeight * weightMultiplier));
                }
            }
        }

        return possibleAffixList;
    }

    public AbilityParticleSystem GetAbilityParticleSystem(string abilityId)
    {
        if (string.IsNullOrEmpty(abilityId))
            return null;
        GameObject targetPrefab = particles.LoadAsset(abilityId) as GameObject;
        if (targetPrefab == null)
            return null;

        GameObject abs = Instantiate(targetPrefab);

        return abs.GetComponent<AbilityParticleSystem>();
    }

    public void LoadEnemySpritesToBeUsed(HashSet<EnemyBase> enemyBases)
    {
        enemySpriteList = new Dictionary<string, Sprite>();
        loadedSpriteAtlases = new Dictionary<string, SpriteAtlas>();
        if (enemyBases.Count == 0)
            return;
        var spriteBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "enemies"));
        SpriteAtlas[] atlases = spriteBundle.LoadAllAssets<SpriteAtlas>();
        foreach (EnemyBase enemyBase in enemyBases)
        {
            foreach (SpriteAtlas atlas in atlases)
            {
                Sprite sprite = atlas.GetSprite(enemyBase.idName.ToLower());

                if (sprite == null && !string.IsNullOrEmpty(enemyBase.spriteName))
                {
                    sprite = atlas.GetSprite(enemyBase.spriteName.ToLower());
                }

                if (sprite != null)
                {
                    enemySpriteList.Add(enemyBase.idName, sprite);
                    if (!loadedSpriteAtlases.ContainsKey(atlas.name))
                        loadedSpriteAtlases.Add(atlas.name, atlas);
                    break;
                }
            }
        }

        foreach (SpriteAtlas atlas in atlases)
        {
            if (!loadedSpriteAtlases.ContainsKey(atlas.name))
                Resources.UnloadAsset(atlas);
        }

        spriteBundle.Unload(false);
    }

    public void LoadAbilitySpritesToBeUsed(HashSet<AbilityBase> abilityBases)
    {
        abilitySpriteList = new Dictionary<string, Sprite>();
        loadedSpriteAtlases = new Dictionary<string, SpriteAtlas>();
        if (abilityBases.Count == 0)
            return;
        var spriteBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "abilitysprites"));
        SpriteAtlas[] atlases = spriteBundle.LoadAllAssets<SpriteAtlas>();
        foreach (AbilityBase abilityBase in abilityBases)
            foreach (SpriteAtlas atlas in atlases)
            {
                Sprite sprite = atlas.GetSprite(abilityBase.idName);

                if (!string.IsNullOrEmpty(abilityBase.effectSprite))
                {
                    sprite = atlas.GetSprite(abilityBase.effectSprite);
                }

                if (sprite != null)
                {
                    abilitySpriteList.Add(abilityBase.idName, sprite);
                    if (!loadedSpriteAtlases.ContainsKey(atlas.name))
                        loadedSpriteAtlases.Add(atlas.name, atlas);
                    break;
                }
            }

        foreach (SpriteAtlas atlas in atlases)
        {
            if (!loadedSpriteAtlases.ContainsKey(atlas.name))
                Resources.UnloadAsset(atlas);
        }

        spriteBundle.Unload(false);
    }

    public void LoadAtlasByName(string name)
    {
        var spriteBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "abilitysprites"));
        SpriteAtlas atlas = spriteBundle.LoadAsset<SpriteAtlas>(name);

        spriteBundle.Unload(false);
    }

    public Sprite GetAbilitySprite(string abilityName)
    {
        if (abilitySpriteList.TryGetValue(abilityName, out Sprite ret))
            return ret;
        else
            return null;
    }

    public Sprite GetEnemySprite(string enemyName)
    {
        if (enemySpriteList.TryGetValue(enemyName, out Sprite ret))
            return ret;
        else
            return null;
    }

    public SpriteAtlas GetSpriteAtlas(string atlasName)
    {
        return loadedSpriteAtlases[atlasName];
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

    private void LoadUniques()
    {
        uniqueList = new Dictionary<string, UniqueBase>();

        List<UniqueBase> temp = DeserializeFromPath_Resources<List<UniqueBase>>("json/items/unique");
        foreach (UniqueBase unique in temp)
        {
            uniqueList.Add(unique.idName, unique);
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
            string name = "stage" + stage.act + "-" + stage.stage;
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
            affix.SetAffixBonusTypeString();
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
        DontDestroyOnLoad(this.gameObject);
        Initialize();
    }

    private void RequestAtlas(string name, System.Action<SpriteAtlas> callback)
    {
        Debug.Log("name: " + name);
        if (loadedSpriteAtlases == null)
            loadedSpriteAtlases = new Dictionary<string, SpriteAtlas>();

        if (!loadedSpriteAtlases.ContainsKey(name))
        {
            LoadAtlasByName(name);
        }

        SpriteAtlas atlas = loadedSpriteAtlases[name];
        callback(atlas);
    }

    public Sprite GetHeroSprite(string name)
    {
        return heroesSpriteAtlas.GetSprite(name);
    }

    public Sprite[] GetHeroSprites()
    {
        if (heroSprites == null)
        {
            heroSprites = new Sprite[heroesSpriteAtlas.spriteCount];
            heroesSpriteAtlas.GetSprites(heroSprites);
        }
        return heroSprites;
    }

    private void Initialize()
    {
        //jsonBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath,"jsonfiles"));
        particles = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "particlesystems"));
        SpriteAtlasManager.atlasRequested += RequestAtlas;
        LoadAbilities();
        LoadEquipment();
        prefixList = LoadAffixes(AffixType.PREFIX);
        suffixList = LoadAffixes(AffixType.SUFFIX);
        innateList = LoadAffixes(AffixType.INNATE);
        enchantmentList = LoadAffixes(AffixType.ENCHANTMENT);
        monsterModList = LoadAffixes(AffixType.MONSTERMOD);
        LoadUniques();
        LoadArchetypes();
        LoadEnemies();
        LoadStages();

        AbilityCount = abilityList.Count;
        EquipmentCount = equipmentList.Count;
        PrefixCount = prefixList.Count;
        SuffixCount = suffixList.Count;
        ArchetypeCount = archetypeList.Count;

        normalMaterialBlock = new MaterialPropertyBlock();
        uncommonMaterialBlock = new MaterialPropertyBlock();
        rareMaterialBlock = new MaterialPropertyBlock();
        bossMaterialBlock = new MaterialPropertyBlock();

        normalMaterialBlock.SetInt("_EnableGlow", 0);
        uncommonMaterialBlock.SetInt("_EnableGlow", 1);
        rareMaterialBlock.SetInt("_EnableGlow", 1);
        bossMaterialBlock.SetInt("_EnableGlow", 1);

        uncommonMaterialBlock.SetColor("_GlowColor", new Color32(11, 234, 251, 255));
        rareMaterialBlock.SetColor("_GlowColor", new Color32(245, 246, 13, 255));
        bossMaterialBlock.SetColor("_GlowColor", new Color32(225, 73, 46, 255));

    }
}