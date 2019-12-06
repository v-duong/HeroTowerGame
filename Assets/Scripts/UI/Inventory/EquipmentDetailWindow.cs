using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentDetailWindow : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private CraftingPanelAffixHeader innateHeader;

    [SerializeField]
    private CraftingPanelAffixHeader prefixHeader;

    [SerializeField]
    private CraftingPanelAffixHeader suffixHeader;

    [SerializeField]
    private RectTransform scrollParent;

    [SerializeField]
    private TextMeshProUGUI innateText;

    [SerializeField]
    private TextMeshProUGUI prefixText;

    [SerializeField]
    private TextMeshProUGUI suffixText;

    [SerializeField]
    private TextMeshProUGUI topLineText;

    [SerializeField]
    private TextMeshProUGUI statsText;

    [SerializeField]
    private TextMeshProUGUI damageText;

    [SerializeField]
    public GameObject EquipButtonParent;

    public Equipment equip;

    public Image NameBackground;

    public Action<Item> callback;

    public void UpdateWindowEquipment(HeroData hero)
    {
        GetComponentInChildren<Outline>().effectColor = Helpers.ReturnRarityColor(equip.Rarity);
        NameBackground.color = Helpers.ReturnRarityColor(equip.Rarity);
        nameText.text = equip.Name;
        topLineText.text = "";
        damageText.text = "";
        statsText.text = "";

        string groupTypeString = LocalizationManager.Instance.GetLocalizationText("groupType." + equip.Base.group);
        string equipTypeString = LocalizationManager.Instance.GetLocalizationText("equipSlotType." + equip.Base.equipSlot);

        if (equip.Rarity != RarityType.UNIQUE)
            topLineText.text += LocalizationManager.Instance.GetLocalizationText_Equipment(equip.Base.idName) + "\n";

        if (groupTypeString.Equals(equipTypeString))
        {
            topLineText.text += groupTypeString + "\n";
        }
        else
        {
            topLineText.text += groupTypeString + " (" + equipTypeString + ")\n";
        }

        topLineText.text += "Item Level " + equip.ItemLevel + "\n";
        topLineText.text += LocalizationManager.Instance.GetRequirementText(equip);

        if (equip.GetItemType() == ItemType.ARMOR)
        {
            UpdateWindowEquipment_Armor((Armor)equip);
        }
        else if (equip.GetItemType() == ItemType.WEAPON)
        {
            UpdateWindowEquipment_Weapon((Weapon)equip);
        }

        innateText.text = "";
        prefixText.text = "";
        suffixText.text = "";

        if (equip.innate.Count > 0)
        {
            innateHeader.gameObject.SetActive(true);
            foreach (Affix a in equip.innate)
            {
                innateText.text += "○" + Affix.BuildAffixString(a.Base, Helpers.AFFIX_STRING_SPACING, a, a.GetAffixValues(), a.GetEffectValues());
            }
        }
        else
        {
            innateHeader.gameObject.SetActive(false);
        }

        if (equip.prefixes.Count > 0)
        {
            prefixHeader.gameObject.SetActive(true);
            if (equip.Rarity == RarityType.UNIQUE)
                prefixHeader.headerText.text = "Affixes";
            else
                prefixHeader.headerText.text = "Prefixes";
            foreach (Affix a in equip.prefixes)
            {
                prefixText.text += "○" + Affix.BuildAffixString(a.Base, Helpers.AFFIX_STRING_SPACING, a, a.GetAffixValues(), a.GetEffectValues());
            }
        }
        else
        {
            prefixHeader.gameObject.SetActive(false);
        }

        if (equip.suffixes.Count > 0)
        {
            suffixHeader.gameObject.SetActive(true);
            foreach (Affix a in equip.suffixes)
            {
                suffixText.text += "○" + Affix.BuildAffixString(a.Base, Helpers.AFFIX_STRING_SPACING, a, a.GetAffixValues(), a.GetEffectValues());
            }
        }
        else
        {
            suffixHeader.gameObject.SetActive(false);
        }

        if (hero != null)
        {
            scrollParent.offsetMin = new Vector2(scrollParent.offsetMin.x, 45);
            EquipButtonParent.gameObject.SetActive(true);
            EquipButtonParent.GetComponentInChildren<Button>().interactable = hero.CanEquipItem(equip);
        }
        else
        {
            scrollParent.offsetMin = new Vector2(scrollParent.offsetMin.x, 0);
            EquipButtonParent.gameObject.SetActive(false);
            EquipButtonParent.GetComponentInChildren<Button>().interactable = true;
        }
    }

    public void UpdateWindowEquipment_Armor(Armor armorItem)
    {
        if (armorItem.armor != 0)
            damageText.text += "Armor: " + armorItem.armor + "\n";
        if (armorItem.shield != 0)
            damageText.text += "Shield: " + armorItem.shield + "\n";
        if (armorItem.dodgeRating != 0)
            damageText.text += "Dodge Rating: " + armorItem.dodgeRating + "\n";
        if (armorItem.resolveRating != 0)
            damageText.text += "Resolve: " + armorItem.resolveRating + "\n";
    }

    public void UpdateWindowEquipment_Weapon(Weapon weaponItem)
    {
        bool hasElemental = false;
        bool hasPrimordial = false;
        List<string> elementalDamage = new List<string>();
        List<string> primDamage = new List<string>();
        MinMaxRange range;
        float dps;

        for (int i = 1; i < (int)ElementType.DIVINE; i++)
        {
            range = weaponItem.GetWeaponDamage((ElementType)i);
            if (!range.IsZero())
            {
                hasElemental = true;
                dps = (range.min + range.max) / 2f * weaponItem.AttackSpeed;
                elementalDamage.Add("<b>" + LocalizationManager.BuildElementalDamageString(range.min + "-" + range.max + " (" + dps.ToString("N1") + ")", (ElementType)i) + "</b>");
            }
        }

        for (int i = (int)ElementType.DIVINE; i <= (int)ElementType.VOID; i++)
        {
            range = weaponItem.GetWeaponDamage((ElementType)i);
            if (!range.IsZero())
            {
                hasPrimordial = true;
                dps = (range.min + range.max) / 2f * weaponItem.AttackSpeed;
                primDamage.Add("<b>" + LocalizationManager.BuildElementalDamageString(range.min + "-" + range.max + " (" + dps.ToString("N1") + ")", (ElementType)i) + "</b>");
            }
        }

        if (weaponItem.PhysicalDamage.min != 0 && weaponItem.PhysicalDamage.max != 0)
        {
            dps = (weaponItem.PhysicalDamage.min + weaponItem.PhysicalDamage.max) / 2f * weaponItem.AttackSpeed;
            damageText.text += "Physical Damage (DPS) \n<sprite=0><b> " + weaponItem.PhysicalDamage.min + "-" + weaponItem.PhysicalDamage.max + " (" + dps.ToString("N1") + ")</b>\n\n";
        }
        if (hasElemental)
        {
            damageText.text += "Elemental Damage (DPS) \n" + String.Join("\n", elementalDamage) + "\n\n";
        }
        if (hasPrimordial)
        {
            damageText.text += "Astral Damage (DPS) \n" + String.Join("\n", primDamage) + "\n\n";
        }
        statsText.text += "Critical Chance\n<b>" + weaponItem.CriticalChance.ToString("F2") + "%</b>\n";
        statsText.text += "Attacks per Second\n<b>" + weaponItem.AttackSpeed.ToString("F2") + "/s</b>\n";
        statsText.text += "Range\n<b>" + weaponItem.WeaponRange.ToString("F2") + "</b>";
    }

    public void OnEquipClick()
    {
        callback?.Invoke(equip);
    }
}