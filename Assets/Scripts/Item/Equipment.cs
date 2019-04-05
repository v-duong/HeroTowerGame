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
        Name = e.name;
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

    protected static void GetLocalModValues(int[] flatMods, double[] additiveMods, List<Affix> affixes, Dictionary<BonusType, int> localBonusTypes)
    {
        foreach (Affix affix in affixes)
        {
            foreach (AffixBonusProperty prop in affix.Base.affixBonuses)
            {
                if (localBonusTypes.ContainsKey(prop.bonusType))
                {
                    if (prop.modifyType == ModifyType.FLAT_ADDITION)
                        flatMods[localBonusTypes[prop.bonusType]] += affix.GetAffixValue(prop.bonusType);
                    else if (prop.modifyType == ModifyType.ADDITIVE)
                        additiveMods[localBonusTypes[prop.bonusType]] += ((double)affix.GetAffixValue(prop.bonusType) / 100);
                }
            }
        }
    }
}
