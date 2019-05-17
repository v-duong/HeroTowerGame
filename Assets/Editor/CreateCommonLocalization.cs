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



            List<string> elementTypes = new List<string>(Enum.GetNames(typeof(ElementType)));

            foreach (string x in elementTypes)
            {
                Debug.Log(x);
                if (string.Compare(x ,"COUNT") == 0)
                    continue;
                string localizationKey = "elementType." + x;
                if (!localization.ContainsKey(localizationKey))
                    localization.Add(localizationKey, "");
                else
                    keys.Remove(localizationKey);
            }

            List<string> effectTypes = new List<string>(Enum.GetNames(typeof(EffectType)));

            foreach (string x in effectTypes)
            {
                string localizationKey = "effectType." + x;
                if (!localization.ContainsKey(localizationKey))
                    localization.Add(localizationKey, "");
                else
                    keys.Remove(localizationKey);
            }

            string filepath2 = "Assets/Resources/json/localization/UIKeys.txt";
            string uiKeysText = System.IO.File.ReadAllText(filepath2);
            
            string[] uiKeys = uiKeysText.Split(',');
            foreach (string x in uiKeys)
            {
                string localizationKey = x;
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