using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    private static readonly string defaultLang = "en-US";

    private static Dictionary<string, string> localizationData = new Dictionary<string, string>();
    private static ItemGenLocalization itemGenLocalization;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadLocalization();
    }

    private void LoadLocalization(string locale = "en-US")
    {
        if (locale == null)
            locale = defaultLang;
        string path = "json/localization/common." + locale;
        localizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(path).text);

        path = "json/localization/itemgen." + locale;
        itemGenLocalization = JsonConvert.DeserializeObject<ItemGenLocalization>(Resources.Load<TextAsset>(path).text);
    }

    public string GetLocalizationText(string stringId)
    {
        if (localizationData.TryGetValue(stringId, out string value))
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

    public string GetLocalizationText_Equipment(string stringId)
    {
        if (localizationData.TryGetValue("equipment." + stringId, out string value))
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

    public string GetLocalizationText_BonusType(BonusType type, ModifyType modifyType, double value)
    {
        string output = "";
        if (localizationData.TryGetValue("bonusType." + type.ToString(), out output))
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
        else if (modifyType == ModifyType.SET)
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