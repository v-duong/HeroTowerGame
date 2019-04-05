using System.Collections.Generic;
using UnityEngine;

public class Armor : Equipment
{
    public int armor;
    public int shield;
    public int dodgeRating;
    public int resolveRating;
    private static readonly Dictionary<BonusType, int> localBonusTypes = new Dictionary<BonusType, int> {
        { BonusType.LOCAL_ARMOR, 0 },
        { BonusType.LOCAL_MAX_SHIELD, 1 },
        { BonusType.LOCAL_DODGE_RATING, 2 },
        { BonusType.LOCAL_RESOLVE_RATING, 3 }
    };

    public Armor(EquipmentBase e, int ilvl) : base(e, ilvl)
    {
        armor = e.armor;
        shield = e.shield;
        dodgeRating = e.dodgeRating;
        resolveRating = e.resolveRating;
    }

    public override ItemType GetItemType()
    {
        return ItemType.ARMOR;
    }

    public override bool UpdateItemStats()
    {

        int[] flatMods = new int[4];
        double[] additiveMods = new double[4] { 1, 1, 1, 1};
        GetLocalModValues(flatMods, additiveMods, prefixes, localBonusTypes);

        armor = Base.armor + flatMods[0];
        shield = Base.shield + flatMods[1];
        dodgeRating = Base.dodgeRating + flatMods[2];
        resolveRating = Base.resolveRating + flatMods[3];

        armor = (int)(armor * additiveMods[0]);
        shield = (int)(shield * additiveMods[1]);
        dodgeRating = (int)(dodgeRating * additiveMods[2]);
        resolveRating = (int)(resolveRating * additiveMods[3]);

        return true;
    }
}