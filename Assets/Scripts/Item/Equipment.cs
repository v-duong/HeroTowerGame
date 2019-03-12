using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Equipment : Item
{
    public EquipmentBase Base { get { return ResourceManager.Instance.GetEquipmentBase(BaseId); } }
    private string BaseId { get; set; }
    public float costModifier;
    public int strRequirement;
    public int intRequirement;
    public int agiRequirement;
    public int willRequirement;
    public HeroActor equippedToHero;
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
}
