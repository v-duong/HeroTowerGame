using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    private static readonly string defaultLang = "en-US";

    private static Dictionary<string, string> commonLocalizationData = new Dictionary<string, string>();
    private static Dictionary<string, string> archetypeLocalizationData = new Dictionary<string, string>();
    private static Dictionary<string, string> equipmentLocalizationData = new Dictionary<string, string>();
    private static Dictionary<string, string> abilityLocalizationData = new Dictionary<string, string>();
    private static Dictionary<string, string> enemyLocalizationData = new Dictionary<string, string>();
    private static ItemGenLocalization itemGenLocalization;

    private void Awake()
    {
        Instance = this;
        LoadLocalization();
    }

    private void LoadLocalization(string locale = "en-US")
    {
        if (locale == null)
            locale = defaultLang;
        string path = "json/localization/common." + locale;
        commonLocalizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(path).text);

        path = "json/localization/ability." + locale;
        abilityLocalizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(path).text);

        path = "json/localization/archetype." + locale;
        archetypeLocalizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(path).text);

        path = "json/localization/equipment." + locale;
        equipmentLocalizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(path).text);

        path = "json/localization/enemy." + locale;
        enemyLocalizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(path).text);

        path = "json/localization/itemgen." + locale;
        itemGenLocalization = JsonConvert.DeserializeObject<ItemGenLocalization>(Resources.Load<TextAsset>(path).text);

    }

    public string GetLocalizationText(string stringId)
    {
        if (commonLocalizationData.TryGetValue(stringId, out string value))
        {
            if (value == "")
                return stringId;
            return value;
        }
        else
        {
            return stringId;
        }
    }

    public string[] GetLocalizationText_Ability(string stringId)
    {
        string[] output = new string[3];
        string value = "";
        if (abilityLocalizationData.TryGetValue("ability." + stringId + ".name", out value))
        {
            if (value == "")
                output[0] = stringId;
            else
                output[0] = value;
        }

        if (abilityLocalizationData.TryGetValue("ability." + stringId + ".text", out value))
        {
            output[1] = value;
        }

        return output;
    }

    public string GetLocalizationText_Element(ElementType type)
    {
        return GetLocalizationText("elementType." + type.ToString());
    }

    public static string BuildElementalDamageString(string s, ElementType element)
    {
        switch (element)
        {
            case ElementType.FIRE:
                return "<color=#e03131>" + s + "</color>";
            case ElementType.COLD:
                return "<color=#33b7e8>" + s + "</color>";
            case ElementType.LIGHTNING:
                return "<color=#9da800>" + s + "</color>";
            case ElementType.EARTH:
                return "<color=#7c5916>" + s + "</color>";
            case ElementType.DIVINE:
                return "<color=#f29e02>" + s + "</color>";
            case ElementType.VOID:
                return "<color=#56407c>" + s + "</color>";
            default:
                return s;
    }
}

    public string GetLocalizationText_AbilityBaseDamage(int level, AbilityBase ability)
    {
        string s = "";
        string damageText;
        if (ability.abilityType == AbilityType.SPELL)
        {
            damageText = GetLocalizationText("UI_DEAL_DAMAGE_FIXED");
            foreach (KeyValuePair<ElementType, AbilityDamageBase> damage in ability.damageLevels)
            {
                var d = damage.Value.damage[level];
                s += string.Format(damageText, d.min, d.max, GetLocalizationText_Element(damage.Key)) + "\n";
            }
        }
        else if (ability.abilityType == AbilityType.ATTACK)
        {
            damageText = GetLocalizationText("UI_DEAL_DAMAGE_WEAPON");
            float d = ability.weaponMultiplier + ability.weaponMultiplierScaling * level;
            s += string.Format(damageText, d) + "\n";
        }
        return s;
    }

    public string GetLocalizationText_AbilityCalculatedDamage(Dictionary<ElementType, MinMaxRange> damageDict)
    {
        string s = "";
        string damageText = GetLocalizationText("UI_DEAL_DAMAGE_FIXED");
        foreach (KeyValuePair<ElementType, MinMaxRange> damage in damageDict)
        {
            s += string.Format(damageText, damage.Value.min, damage.Value.max, GetLocalizationText_Element(damage.Key)) + "\n";
        }
        return s;
    }

    public string GetLocalizationText_Equipment(string stringId)
    {
        if (equipmentLocalizationData.TryGetValue("equipment." + stringId, out string value))
        {
            if (value == "")
                return stringId;
            return value;
        }
        else
        {
            return stringId;
        }
    }

    public string GetLocalizationText_Archetype(string stringId)
    {
        if (archetypeLocalizationData.TryGetValue("archetype." + stringId, out string value))
        {
            if (value == "")
                return stringId;
            return value;
        }
        else
        {
            return stringId;
        }
    }

    public string GetLocalizationText_Enemy(string stringId, string field)
    {
        if (enemyLocalizationData.TryGetValue("enemy." + stringId + field, out string value))
        {
            if (value == "")
                return stringId;
            return value;
        }
        else
        {
            return stringId;
        }
    }

    public string GetLocalizationText_BonusType(BonusType type, ModifyType modifyType, float value)
    {
        string output = "";
        if (commonLocalizationData.TryGetValue("bonusType." + type.ToString(), out output))
        {
            if (output == "")
            {
                output = type.ToString();
            }
        }
        else
        {
            output = type.ToString();
        }

        if (modifyType == ModifyType.FLAT_ADDITION)
        {
            output += " +" + value + "\n";
        }
        else if (modifyType == ModifyType.ADDITIVE)
        {
            output += " +" + value + "%" + "\n";
        }
        else if (modifyType == ModifyType.MULTIPLY)
        {
            output += " x" + (1 + value / 100d).ToString("F2") + "\n";
        }
        else if (modifyType == ModifyType.FIXED_TO)
        {
            output += " is" + value + "\n";
        }

        return output;
    }

    public string GenerateRandomItemName(ICollection<GroupType> tags)
    {
        if (itemGenLocalization == null)
        {
            LoadLocalization();
        }
        List<string> prefixes = new List<string>(itemGenLocalization.CommonPrefixes);
        List<string> suffixes = new List<string>(itemGenLocalization.CommonSuffixes);

        List<string> temp;
        /*
        if (itemGenLocalization.prefix.TryGetValue(type, out temp))
        {
            prefixes.AddRange(temp);
        }
        */
        foreach (GroupType type in tags)
        {
            if (itemGenLocalization.suffix.TryGetValue(type, out temp))
            {
                suffixes.AddRange(temp);
            }
        }

        string s = "";

        s += prefixes[Random.Range(0, prefixes.Count)] + " ";
        s += suffixes[Random.Range(0, suffixes.Count)];

        return s;
    }

    private class ItemGenLocalization
    {
        public Dictionary<GroupType, List<string>> prefix;
        public Dictionary<GroupType, List<string>> suffix;

        public List<string> CommonPrefixes => prefix[GroupType.NO_GROUP];
        public List<string> CommonSuffixes => suffix[GroupType.NO_GROUP];
    }
}