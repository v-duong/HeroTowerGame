using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Equipment : Item
{
    public EquipmentBase Base { get { return ResourceManager.Instance.GetEquipmentBase(BaseId); } }
    private string BaseId { get; set; }
    public float costModifier;
    public int strRequirement;
    public int intRequirement;
    public int agiRequirement;
    public int willRequirement;
    public HeroData equippedToHero;
    public List<Affix> innate;
    

    public Equipment(EquipmentBase e, int ilvl)
    {
        BaseId = e.idName;
        costModifier = e.sellValue;
        strRequirement = e.strengthReq;
        intRequirement = e.intelligenceReq;
        agiRequirement = e.agilityReq;
        willRequirement = e.willReq;
        Rarity = RarityType.NORMAL;
        ItemLevel = ilvl;
        prefixes = new List<Affix>();
        suffixes = new List<Affix>();
        innate = new List<Affix>();
        itemType = e.group;
        equippedToHero = null;
        if (e.hasInnate)
        {
            Affix newInnate = new Affix(ResourceManager.Instance.GetAffixBase(e.innateAffixId, AffixType.INNATE));
            innate.Add(newInnate);
        }
    }

    public override bool UpgradeRarity()
    {
        if (Rarity == RarityType.EPIC)
            return false;
        else
            Rarity++;
        AddRandomAffix();
        return true;
    }

    protected static void GetLocalModValues(Dictionary<BonusType, HeroStatBonus> dic, List<Affix> affixes, ItemType itemType)
    {
        int startValue = 0;
        switch(itemType)
        {
            case ItemType.ARMOR:
                startValue = Armor.LocalBonusStart;
                break;
            case ItemType.ARCHETYPE:
                startValue = Archetype.LocalBonusStart;
                break;
            case ItemType.WEAPON:
                startValue = Weapon.LocalBonusStart;
                break;
            default:
                return;
        }

        foreach (Affix affix in affixes)
        {
            foreach (AffixBonusProperty prop in affix.Base.affixBonuses)
            {
                if ((int)prop.bonusType >= startValue && (int)prop.bonusType < startValue + 0x100)
                {
                    if (!dic.ContainsKey(prop.bonusType))
                        dic.Add(prop.bonusType, new HeroStatBonus());
                    if (prop.modifyType == ModifyType.FLAT_ADDITION)
                        dic[prop.bonusType].AddToFlat(affix.GetAffixValue(prop.bonusType));
                    else if (prop.modifyType == ModifyType.ADDITIVE)
                        dic[prop.bonusType].AddToAdditive(affix.GetAffixValue(prop.bonusType));
                }
            }
        }
    }

    protected static int CalculateStat(int stat, BonusType bonusType, Dictionary<BonusType, HeroStatBonus> dic)
    {
        HeroStatBonus bonus;

        if(dic.TryGetValue(bonusType, out bonus))
        {
            return bonus.CalculateStat(stat);
        } else
        {
            return stat;
        }
    }

    protected static double CalculateStat(double stat, BonusType bonusType, Dictionary<BonusType, HeroStatBonus> dic)
    {
        HeroStatBonus bonus;

        if (dic.TryGetValue(bonusType, out bonus))
        {
            return bonus.CalculateStat(stat);
        }
        else
        {
            return stat;
        }
    }
}
