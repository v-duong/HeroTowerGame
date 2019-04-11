using System.Collections.Generic;

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

    public bool IsEquipped
    {
        get
        {
            if (equippedToHero == null)
                return false;
            else
                return true;
        }
    }

    public Equipment(EquipmentBase e, int ilvl)
    {
        BaseId = e.idName;
        Name = LocalizationManager.Instance.GetLocalizationText("equipment." + e.idName);
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
        UpdateName();
        AddRandomAffix();
        return true;
    }

    public override void UpdateName()
    {
        if (Rarity == RarityType.RARE || Rarity == RarityType.EPIC)
        {
            Name = LocalizationManager.Instance.GenerateRandomItemName(GetGroupTypes());
        } else if (Rarity == RarityType.UNCOMMON)
        {

        } else
        {
            Name = LocalizationManager.Instance.GetLocalizationText_Equipment(Base.idName);
        }
    }

    protected static void GetLocalModValues(Dictionary<BonusType, HeroStatBonus> dic, List<Affix> affixes, EquipmentType itemType)
    {
        int startValue = 0;
        switch (itemType)
        {
            case global::EquipmentType.ARMOR:
                startValue = Armor.LocalBonusStart;
                break;

            case global::EquipmentType.ARCHETYPE:
                startValue = Archetype.LocalBonusStart;
                break;

            case global::EquipmentType.WEAPON:
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
        if (dic.TryGetValue(bonusType, out HeroStatBonus bonus))
        {
            return bonus.CalculateStat(stat);
        }
        else
        {
            return stat;
        }
    }

    protected static double CalculateStat(double stat, BonusType bonusType, Dictionary<BonusType, HeroStatBonus> dic)
    {
        if (dic.TryGetValue(bonusType, out HeroStatBonus bonus))
        {
            return bonus.CalculateStat(stat);
        }
        else
        {
            return stat;
        }
    }
}