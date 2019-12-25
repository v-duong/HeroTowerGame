using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

            List<string> triggerTypes = new List<string>(Enum.GetNames(typeof(TriggerType)));

            foreach (string x in triggerTypes)
            {
                string localizationKey = "triggerType." + x;
                if (!localization.ContainsKey(localizationKey))
                    localization.Add(localizationKey, "");
                else
                    keys.Remove(localizationKey);
            }

            List<string> groupTypes = new List<string>(Enum.GetNames(typeof(GroupType)));

            foreach (string x in groupTypes)
            {
                string localizationKey = "groupType." + x;
                string pluralLocalizationKey = "groupType." + x + ".plural";
                string restrictionLocalizationKey = "groupType." + x + ".restriction";

                if (!localization.ContainsKey(localizationKey))
                    localization.Add(localizationKey, "");
                else
                    keys.Remove(localizationKey);

                if (!localization.ContainsKey(pluralLocalizationKey))
                    localization.Add(pluralLocalizationKey, "");
                else
                    keys.Remove(pluralLocalizationKey);

                if (!localization.ContainsKey(restrictionLocalizationKey))
                    localization.Add(restrictionLocalizationKey, "");
                else
                    keys.Remove(restrictionLocalizationKey);
            }

            List<string> elementTypes = new List<string>(Enum.GetNames(typeof(ElementType)));

            foreach (string x in elementTypes)
            {
                if (string.Compare(x, "COUNT") == 0)
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
                string bonusLocalizationKey = "effectType.bonusProp." + x;

                if (!localization.ContainsKey(localizationKey))
                    localization.Add(localizationKey, "");
                else
                    keys.Remove(localizationKey);

                if (!localization.ContainsKey(bonusLocalizationKey))
                    localization.Add(bonusLocalizationKey, "");
                else
                    keys.Remove(bonusLocalizationKey);
            }

            List<string> consumableTypes = new List<string>(Enum.GetNames(typeof(ConsumableType)));

            foreach (string x in consumableTypes)
            {
                string localizationKey = "consumableType." + x;
                if (!localization.ContainsKey(localizationKey))
                    localization.Add(localizationKey, "");
                else
                    keys.Remove(localizationKey);
            }


            List<string> slotTypes = new List<string>(Enum.GetNames(typeof(EquipSlotType)));

            foreach (string x in slotTypes)
            {
                string localizationKey = "slotType." + x;
                if (!localization.ContainsKey(localizationKey))
                    localization.Add(localizationKey, "");
                else
                    keys.Remove(localizationKey);
            }

            List<string> rarityTypes = new List<string>(Enum.GetNames(typeof(RarityType)));

            foreach (string x in rarityTypes)
            {
                string localizationKey = "rarityType." + x;
                if (!localization.ContainsKey(localizationKey))
                    localization.Add(localizationKey, "");
                else
                    keys.Remove(localizationKey);
            }

            List<string> targetTypes = new List<string>(Enum.GetNames(typeof(AbilityTargetType)));

            foreach (string x in targetTypes)
            {
                string localizationKey = "targetType." + x;
                if (!localization.ContainsKey(localizationKey))
                    localization.Add(localizationKey, "");
                else
                    keys.Remove(localizationKey);
            }

            List<string> primaryTargetings = new List<string>(Enum.GetNames(typeof(PrimaryTargetingType)));

            foreach (string x in primaryTargetings)
            {
                string localizationKey = "primaryTargetingType." + x;
                if (!localization.ContainsKey(localizationKey))
                    localization.Add(localizationKey, "");
                else
                    keys.Remove(localizationKey);
            }

            List<string> equipSlotTypes = new List<string>(Enum.GetNames(typeof(EquipSlotType)));

            foreach (string x in equipSlotTypes)
            {
                string localizationKey = "equipSlotType." + x;
                if (!localization.ContainsKey(localizationKey))
                    localization.Add(localizationKey, "");
                else
                    keys.Remove(localizationKey);
            }

            List<string> abilityTypes = new List<string>(Enum.GetNames(typeof(AbilityType)));

            foreach (string x in abilityTypes)
            {
                string localizationKey = "abilityType." + x;
                if (!localization.ContainsKey(localizationKey))
                    localization.Add(localizationKey, "");
                else
                    keys.Remove(localizationKey);
            }

            List<string> shotTypes = new List<string>(Enum.GetNames(typeof(AbilityShotType)));

            foreach (string x in shotTypes)
            {
                string localizationKey = "abilityShotType." + x;
                if (!localization.ContainsKey(localizationKey))
                    localization.Add(localizationKey, "");
                else
                    keys.Remove(localizationKey);
            }

            List<string> sourceTypes = new List<string>(Enum.GetNames(typeof(AbilitySourceType)));

            foreach (string x in sourceTypes)
            {
                string localizationKey = "sourceType." + x;
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

    [MenuItem("Localization/Build UI Strings")]
    public static void FindUIStrings()
    {
        Text[] texts = Resources.FindObjectsOfTypeAll<Text>();
        TextMeshProUGUI[] textMeshPros = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
        string filepath = "Assets/Resources/json/localization/UIKeys.txt";
        string keys = "UI_DEAL_DAMAGE_FIXED,UI_DEAL_DAMAGE_WEAPON,UI_ADD_DAMAGE,";

        foreach (Text t in texts)
        {
            if (t.text.Contains("UI_"))
                keys += t.text + ",";
        }

        foreach (TextMeshProUGUI t in textMeshPros)
        {
            if (t.text.Contains("UI_"))
                keys += t.text + ",";
        }
        keys = keys.TrimEnd(',');
        System.IO.File.WriteAllText(filepath, keys);
    }
}