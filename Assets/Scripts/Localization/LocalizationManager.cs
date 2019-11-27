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
        DontDestroyOnLoad(this.gameObject);
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

    public string GetLocalizationText_SlotType(string stringId)
    {
        return GetLocalizationText("slotType." + stringId);
    }

    public string[] GetLocalizationText_Ability(string stringId)
    {
        string[] output = new string[3];
        if (abilityLocalizationData.TryGetValue("ability." + stringId + ".name", out string value))
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

    public string GetLocalizationText_AbilityCalculatedDamage(Dictionary<ElementType, ActorAbility.AbilityDamageContainer> damageDict)
    {
        string s = "";
        string damageText = GetLocalizationText("UI_DEAL_DAMAGE_FIXED");
        foreach (KeyValuePair<ElementType, ActorAbility.AbilityDamageContainer> damageData in damageDict)
        {
            if (damageData.Value.calculatedRange.IsZero())
                continue;
            s += string.Format(damageText, damageData.Value.calculatedRange.min, damageData.Value.calculatedRange.max, GetLocalizationText_Element(damageData.Key)) + "\n";
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

    public string GetLocalizationText_ArchetypeName(string stringId)
    {
        if (archetypeLocalizationData.TryGetValue("archetype." + stringId + ".name", out string value))
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

    public string GetLocalizationText_GroupType(string stringId)
    {
        if (commonLocalizationData.TryGetValue("groupType." + stringId, out string value))
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

    public string GetLocalizationText_GroupTypeRestriction(string stringId)
    {
        if (commonLocalizationData.TryGetValue("groupType." + stringId + ".restriction", out string value))
        {
            if (value == "")
                return stringId;

            if (value.Contains("{plural}"))
                value = value.Replace("{plural}", GetLocalizationText_GroupTypePlural(stringId));
            else if (value.Contains("{single}"))
                value = value.Replace("{single}", GetLocalizationText_GroupType(stringId));

            return value;
        }
        else
        {
            return stringId;
        }
    }

    public string GetLocalizationText_GroupTypePlural(string stringId)
    {
        if (commonLocalizationData.TryGetValue("groupType." + stringId + ".plural", out string value))
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

    private static string ParseBonusTypeFallback(string b)
    {
        b = b.Replace("_", " ");
        return b.ToLower();
    }

    public string GetLocalizationText_TriggeredEffect(TriggeredEffectBonusProperty triggeredEffect, float value, float? maxValue = null)
    {
        commonLocalizationData.TryGetValue("triggerType." + triggeredEffect.triggerType.ToString(), out string s);
        string valueString, effectString, bonusString;

        if (maxValue != null)
        {
            valueString = "(" + value.ToString("F0") + "-" + ((float)maxValue).ToString("F0") + ")";
            bonusString = GetLocalizationText_BonusType(triggeredEffect.statBonusType, triggeredEffect.statModifyType, value, (float)maxValue, GroupType.NO_GROUP).TrimEnd('\n');
        }
        else
        {
            valueString = value.ToString("F0");
            bonusString = GetLocalizationText_BonusType(triggeredEffect.statBonusType, triggeredEffect.statModifyType, value, GroupType.NO_GROUP).TrimEnd('\n');
        }

        switch (triggeredEffect.triggerType)
        {
            case TriggerType.WHEN_HITTING:
                effectString = bonusString;
                break;

            default:
                effectString = GetLocalizationText("effectType.bonusProp." + triggeredEffect.effectType);
                effectString = effectString.Replace("{TARGET}", GetLocalizationText("targetType." + triggeredEffect.effectTargetType.ToString()));
                effectString = effectString.Replace("{VALUE}", valueString);

                if (triggeredEffect.effectType != EffectType.BUFF && triggeredEffect.effectType != EffectType.DEBUFF)
                {
                    effectString = effectString.Replace("{ELEMENT}", GetLocalizationText_Element(triggeredEffect.effectElement));
                }
                else
                {
                    effectString = effectString.Replace("{BONUS}", bonusString);
                }

                if (triggeredEffect.effectDuration > 0)
                {
                    effectString += " for " + triggeredEffect.effectDuration.ToString("N#.#") + "s";
                }
                break;
        }

        string restrictionString = "";
        if (triggeredEffect.restriction != GroupType.NO_GROUP)
            restrictionString = GetLocalizationText_GroupTypeRestriction(triggeredEffect.restriction.ToString());

        if (triggeredEffect.triggerChance < 1)
        {
            s = triggeredEffect.triggerChance.ToString("P0") + " to " + s;
        }

        s = string.Format(s, restrictionString, effectString) + '\n';

        return s;
    }

    public string GetLocalizationText_BonusType(BonusType type, ModifyType modifyType, float value, GroupType restriction)
    {
        string output = GetBonusTypeString(type);

        if (restriction != GroupType.NO_GROUP)
        {
            output = GetLocalizationText_GroupTypeRestriction(restriction.ToString()) + ", " + output;
        }

        if (type >= (BonusType)HeroArchetypeData.SpecialBonusStart)
        {
            return output;
        }

        output += "<nobr>";

        switch (modifyType)
        {
            case ModifyType.FLAT_ADDITION:
                if (value > 0)
                    output += " +" + value;
                else
                    output += " " + value;
                break;

            case ModifyType.ADDITIVE:
                if (value > 0)
                    output += " +" + value + "%";
                else
                    output += " " + value + "%";
                break;

            case ModifyType.MULTIPLY:
                output += " x" + (1 + value / 100d).ToString(".00##");
                break;

            case ModifyType.FIXED_TO:
                output += " is " + value;
                break;
        }

        output += "</nobr>\n";

        return output;
    }

    public string GetLocalizationText_BonusType(BonusType type, ModifyType modifyType, float minVal, float maxVal, GroupType restriction)
    {
        string output = GetBonusTypeString(type);

        if (restriction != GroupType.NO_GROUP)
        {
            output = GetLocalizationText_GroupTypeRestriction(restriction.ToString()) + ", " + output;
        }

        string valueString;
        if (minVal == maxVal)
            valueString = minVal.ToString();
        else
            valueString = "(" + minVal + "-" + maxVal + ")";

        switch (modifyType)
        {
            case ModifyType.FLAT_ADDITION:
                output += " +" + valueString + "\n";
                break;

            case ModifyType.ADDITIVE:
                output += " +" + valueString + "%" + "\n";
                break;

            case ModifyType.MULTIPLY:
                output += " x(" + (1 + minVal / 100d).ToString(".00##") + "-" + (1 + maxVal / 100d).ToString(".00##") + ")\n";
                break;

            case ModifyType.FIXED_TO:
                output += " is " + valueString + "\n";
                break;
        }

        return output;
    }

    private static string GetBonusTypeString(BonusType type)
    {
        if (commonLocalizationData.TryGetValue("bonusType." + type.ToString(), out string output))
        {
            if (output == "")
            {
                output = ParseBonusTypeFallback(type.ToString());
            }
        }
        else
        {
            output = ParseBonusTypeFallback(type.ToString());
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

        /*
        if (itemGenLocalization.prefix.TryGetValue(type, out temp))
        {
            prefixes.AddRange(temp);
        }
        */
        foreach (GroupType type in tags)
        {
            if (itemGenLocalization.suffix.TryGetValue(type, out List<string> temp))
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