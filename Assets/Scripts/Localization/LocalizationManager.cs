using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }

    private static readonly string defaultLang = "en-US";

    private static Dictionary<string, string> commonLocalizationData = new Dictionary<string, string>();
    private static Dictionary<string, string> archetypeLocalizationData = new Dictionary<string, string>();
    private static Dictionary<string, string> equipmentLocalizationData = new Dictionary<string, string>();
    private static Dictionary<string, string> uniqueLocalizationData = new Dictionary<string, string>();
    private static Dictionary<string, string> abilityLocalizationData = new Dictionary<string, string>();
    private static Dictionary<string, string> enemyLocalizationData = new Dictionary<string, string>();
    private static Dictionary<string, string> helpLocalizationData = new Dictionary<string, string>();
    private static ItemGenLocalization itemGenLocalization;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        LoadLocalization();
    }

    private static void LoadLocalization(string locale = "en-US")
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

        path = "json/localization/unique." + locale;
        uniqueLocalizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(path).text);

        path = "json/localization/enemy." + locale;
        enemyLocalizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(path).text);

        path = "json/localization/help." + locale;
        helpLocalizationData = JsonConvert.DeserializeObject<Dictionary<string, string>>(Resources.Load<TextAsset>(path).text);

        path = "json/localization/itemgen." + locale;
        itemGenLocalization = JsonConvert.DeserializeObject<ItemGenLocalization>(Resources.Load<TextAsset>(path).text);
    }

    public string GetLocalizationText_HelpString(string helpId)
    {
        if (helpLocalizationData.TryGetValue(helpId, out string value))
        {
            MatchCollection regexMatches = Regex.Matches(value, @"{([^}]*)}");
            if (regexMatches.Count > 0)
            {
                foreach(Match y in regexMatches)
                {
                    if (y.Groups[1].Value == helpId)
                        continue;
                    value = value.Replace(y.Groups[0].Value, GetLocalizationText_HelpString(y.Groups[1].Value));
                }
            }
            return value;
        }
        else
            return "";
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

    public string GetLocalizationText(AbilityTargetType targetType)
    {
        string stringId = "targetType." + targetType.ToString();
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

    public string GetLocalizationText(EquipSlotType equipSlot)
    {
        return GetLocalizationText("slotType." + equipSlot.ToString());
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
                s = "<color=#e03131>" + s + "</color>";
                break;

            case ElementType.COLD:
                s = "<color=#33b7e8>" + s + "</color>";
                break;

            case ElementType.LIGHTNING:
                s = "<color=#9da800>" + s + "</color>";
                break;

            case ElementType.EARTH:
                s = "<color=#7c5916>" + s + "</color>";
                break;

            case ElementType.DIVINE:
                s = "<color=#f29e02>" + s + "</color>";
                break;

            case ElementType.VOID:
                s = "<color=#56407c>" + s + "</color>";
                break;

            default:
                break;
        }

        s = "<sprite=" + (int)element + "> " + s;
        return s;
    }

    public string GetLocalizationText_AbilityBaseDamage(int level, AbilityBase ability)
    {
        string s = "";
        string weaponDamageText, baseDamageText;

        if (ability.abilityType == AbilityType.AURA || ability.abilityType == AbilityType.SELF_BUFF)
        {
            baseDamageText = GetLocalizationText("UI_ADD_DAMAGE");
            foreach (KeyValuePair<ElementType, AbilityDamageBase> damage in ability.damageLevels)
            {
                var d = damage.Value.damage[level];
                s += string.Format(baseDamageText, BuildElementalDamageString("<b>" + d.min + "~" + d.max + "</b>", damage.Key)) + "\n";
            }
        }
        else
        {
            if (ability.abilityType == AbilityType.ATTACK)
            {
                weaponDamageText = GetLocalizationText("UI_DEAL_DAMAGE_WEAPON");
                float d = ability.weaponMultiplier + ability.weaponMultiplierScaling * level;
                s += string.Format(weaponDamageText, d) + "\n";
            }

            baseDamageText = GetLocalizationText("UI_DEAL_DAMAGE_FIXED");
            foreach (KeyValuePair<ElementType, AbilityDamageBase> damage in ability.damageLevels)
            {
                var d = damage.Value.damage[level];
                s += string.Format(baseDamageText, BuildElementalDamageString("<b>" + d.min + "~" + d.max + "</b>", damage.Key)) + "\n";
            }

            if (ability.hitCount > 1)
            {
                s += "Hits " + ability.hitCount + "x at " + ability.hitDamageModifier.ToString("P1") + " Damage";
            }
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
            s += string.Format(damageText, BuildElementalDamageString( "<b>" + damageData.Value.calculatedRange.min + "~" + damageData.Value.calculatedRange.max + "</b>", damageData.Key)) + "\n";
        }
        return s;
    }

    public string GetLocalizationText(EquipmentBase equipment)
    {
        string stringId = equipment.idName;
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

    public string[] GetLocalizationText(UniqueBase unique)
    {
        string stringId = unique.idName;
        string[] output = new string[2];
        if (uniqueLocalizationData.TryGetValue("unique." + stringId, out string value))
        {
            if (value == "")
                output[0] = stringId;
            else
                output[0] = value;
        }
        else
        {
            output[0] = stringId;
        }

        if (uniqueLocalizationData.TryGetValue("unique." + stringId +".text", out string desc))
        {
                output[1] = desc;
        }

        return output;
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

    public string GetLocalizationText(GroupType groupType)
    {
        string stringId = groupType.ToString();
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

    public string GetLocalizationText_GroupTypeRestriction(GroupType groupType)
    {
        string stringId = groupType.ToString();
        if (commonLocalizationData.TryGetValue("groupType." + stringId + ".restriction", out string value))
        {
            if (value == "")
                return stringId;

            if (value.Contains("{plural}"))
                value = value.Replace("{plural}", GetLocalizationText_GroupTypePlural(stringId));
            else if (value.Contains("{single}"))
                value = value.Replace("{single}", GetLocalizationText(groupType));

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
        Debug.LogWarning(b + " NOT FOUND");
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
                effectString = effectString.Replace("{TARGET}", GetLocalizationText(triggeredEffect.effectTargetType));
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
                    effectString += " for " + triggeredEffect.effectDuration.ToString("#.#") + "s";
                }
                break;
        }

        string restrictionString = "";
        if (triggeredEffect.restriction != GroupType.NO_GROUP)
            restrictionString = GetLocalizationText_GroupTypeRestriction(triggeredEffect.restriction);

        if (triggeredEffect.triggerChance < 1)
        {
            s = triggeredEffect.triggerChance.ToString("P0") + " Chance to " + s;
        }

        s = string.Format(s, restrictionString, effectString) + '\n';

        return s;
    }

    public string GetLocalizationText_BonusType(BonusType type, ModifyType modifyType, float value, GroupType restriction)
    {
        string output = GetBonusTypeString(type);

        if (restriction != GroupType.NO_GROUP)
        {
            output = GetLocalizationText_GroupTypeRestriction(restriction) + ", " + output;
        }

        if (type >= (BonusType)HeroArchetypeData.SpecialBonusStart)
        {
            return output +"\n";
        }

        output += " <nobr>";

        switch (modifyType)
        {
            case ModifyType.FLAT_ADDITION:
                if (value > 0)
                    output += "+" + value;
                else
                    output +=  value;
                break;

            case ModifyType.ADDITIVE:
                if (value > 0)
                    output += "+" + value + "%";
                else
                    output += value + "%";
                break;

            case ModifyType.MULTIPLY:
                output += "x" + (1 + value / 100d).ToString("0.00##");
                break;

            case ModifyType.FIXED_TO:
                output += "is " + value;
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
            output = GetLocalizationText_GroupTypeRestriction(restriction) + ", " + output;
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

    public string GetRequirementText(Equipment equip)
    {
        string s = "Requires ";
        if (equip.levelRequirement > 0)
        {
            s += "Lv" + equip.levelRequirement;
        }
        if (equip.strRequirement > 0)
        {
            s += ", " + equip.strRequirement + " Str";
        }
        if (equip.intRequirement > 0)
        {
            s += ", " + equip.intRequirement + " Int";
        }
        if (equip.agiRequirement > 0)
        {
            s += ", " + equip.agiRequirement + " Agi";
        }
        if (equip.willRequirement > 0)
        {
            s += ", " + equip.willRequirement + " Will";
        }
        return s.Trim(' ', ',');
    }

    public string GetBonusTypeString(BonusType type)
    {
        if (commonLocalizationData.Count == 0)
            LocalizationManager.LoadLocalization();

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