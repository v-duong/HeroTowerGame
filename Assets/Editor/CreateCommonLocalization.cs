using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateCommonLocalization
{
    public static string[] locales = { "en-US" };

    [MenuItem("Localization/Build Common")]
    public static void BuildCommonLocalization()
    {
        foreach (string locale in locales)
        {
            SortedDictionary<string, string> localization = new SortedDictionary<string, string>();

            string filepath = "Assets/Resources/json/localization/common." + locale + ".json";
            string json = System.IO.File.ReadAllText(filepath);
            localization = JsonConvert.DeserializeObject<SortedDictionary<string, string>>(json);
            HashSet<string> keys = new HashSet<string>(localization.Keys);
            Debug.Log(localization.Count);
            
            List<string> bonusTypes = new List<string>(Enum.GetNames(typeof(BonusType)));

            foreach (string x in bonusTypes)
            {
                string localizationKey = "bonusType." + x;
                if (!localization.ContainsKey(localizationKey))
                    localization.Add(localizationKey, "");
                else
                    keys.Remove(localizationKey);
            }

            List<string> groupTypes = new List<string>(Enum.GetNames(typeof(GroupType)));

            foreach (string x in groupTypes)
            {
                string localizationKey = "groupType." + x;
                if (!localization.ContainsKey(localizationKey))
                    localization.Add(localizationKey, "");
                else
                    keys.Remove(localizationKey);
            }

            foreach (string key in keys)
            {
                localization.Remove(key);
            }

            string o = JsonConvert.SerializeObject(localization);
            System.IO.File.WriteAllText(filepath, o);
            
        }
    }
}