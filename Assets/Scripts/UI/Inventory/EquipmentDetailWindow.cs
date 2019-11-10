﻿using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentDetailWindow : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI affixText;

    [SerializeField]
    private TextMeshProUGUI seperatorText;

    [SerializeField]
    private TextMeshProUGUI infoText;

    [SerializeField]
    public GameObject UpgradeButtonParent;

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
        infoText.text = "";
        infoText.text += LocalizationManager.Instance.GetLocalizationText("groupType." + equip.Base.group) + "\n";
        infoText.text += "Requirements: Lv" + equip.levelRequirement;
        if (equip.strRequirement > 0)
        {
            infoText.text += ", " + equip.strRequirement + " Str";
        }
        if (equip.intRequirement > 0)
        {
            infoText.text += ", " + equip.intRequirement + " Int";
        }
        if (equip.agiRequirement > 0)
        {
            infoText.text += ", " + equip.agiRequirement + " Agi";
        }
        if (equip.willRequirement > 0)
        {
            infoText.text += ", " + equip.willRequirement + " Will";
        }
        infoText.text += "\n\n";

        if (equip.GetItemType() == ItemType.ARMOR)
        {
            UpdateWindowEquipment_Armor((Armor)equip);
        }
        else if (equip.GetItemType() == ItemType.WEAPON)
        {
            UpdateWindowEquipment_Weapon((Weapon)equip);
        }

        affixText.text = "";

        if (equip.innate.Count > 0)
        {
            affixText.text += "Innate\n";
            foreach (Affix a in equip.innate)
            {
                affixText.text += "○" + Affix.BuildAffixString(a.Base, 5, a.GetAffixValues(), a.GetEffectValues());
            }
            affixText.text += "\n";
        }

        if (equip.prefixes.Count > 0)
        {
            if (equip.Rarity == RarityType.UNIQUE)
                affixText.text += "Affixes\n";
            else
                affixText.text += "Prefix\n";
            foreach (Affix a in equip.prefixes)
            {
                affixText.text += "○" + Affix.BuildAffixString(a.Base, 5, a.GetAffixValues(), a.GetEffectValues());
            }
        }

        affixText.text += "\n";

        if (equip.suffixes.Count > 0)
        {
            affixText.text += "Suffix\n";
            foreach (Affix a in equip.suffixes)
            {
                affixText.text += "○" + Affix.BuildAffixString(a.Base, 5, a.GetAffixValues(), a.GetEffectValues());
            }
        }

        if (hero != null)
        {
            EquipButtonParent.gameObject.SetActive(true);
            EquipButtonParent.GetComponentInChildren<Button>().interactable = hero.CanEquipItem(equip);
        }
        else
        {
            EquipButtonParent.gameObject.SetActive(false);
            EquipButtonParent.GetComponentInChildren<Button>().interactable = true;
        }
    }

    public void UpdateWindowEquipment_Armor(Armor armorItem)
    {
        if (armorItem.armor != 0)
            infoText.text += "Armor: " + armorItem.armor + "\n";
        if (armorItem.shield != 0)
            infoText.text += "Shield: " + armorItem.shield + "\n";
        if (armorItem.dodgeRating != 0)
            infoText.text += "Dodge Rating: " + armorItem.dodgeRating + "\n";
        if (armorItem.resolveRating != 0)
            infoText.text += "Resolve: " + armorItem.resolveRating + "\n";
    }

    public void UpdateWindowEquipment_Weapon(Weapon weaponItem)
    {
        bool hasElemental = false;
        bool hasPrimordial = false;
        List<string> elementalDamage = new List<string>();
        List<string> elementalDps = new List<string>();
        List<string> primDamage = new List<string>();
        List<string> primDps = new List<string>();
        string physDPS;
        string physDamage = "";
        MinMaxRange range;
        float dps;

        for (int i = 1; i < (int)ElementType.DIVINE; i++)
        {
            range = weaponItem.GetWeaponDamage((ElementType)i);
            if (!range.IsZero())
            {
                hasElemental = true;
                dps = (range.min + range.max) / 2f * weaponItem.AttackSpeed;
                elementalDps.Add(LocalizationManager.BuildElementalDamageString(dps.ToString("F1"), (ElementType)i));
                elementalDamage.Add(LocalizationManager.BuildElementalDamageString(range.min + "-" + range.max, (ElementType)i));
            }
        }

        range = weaponItem.GetWeaponDamage(ElementType.DIVINE);
        if (!range.IsZero())
        {
            hasPrimordial = true;
            dps = (range.min + range.max) / 2f * weaponItem.AttackSpeed;
            primDps.Add(LocalizationManager.BuildElementalDamageString(dps.ToString("F1"), ElementType.DIVINE));
            primDamage.Add(LocalizationManager.BuildElementalDamageString(range.min + "-" + range.max, ElementType.DIVINE));
        }

        range = weaponItem.GetWeaponDamage(ElementType.VOID);
        if (!range.IsZero())
        {
            hasPrimordial = true;
            dps = (range.min + range.max) / 2f * weaponItem.AttackSpeed;
            primDps.Add(LocalizationManager.BuildElementalDamageString(dps.ToString("F1"), ElementType.VOID));
            primDamage.Add(LocalizationManager.BuildElementalDamageString(range.min + "-" + range.max, ElementType.VOID));
        }

        if (weaponItem.PhysicalDamage.min != 0 && weaponItem.PhysicalDamage.max != 0)
        {
            dps = (weaponItem.PhysicalDamage.min + weaponItem.PhysicalDamage.max) / 2f * weaponItem.AttackSpeed;
            infoText.text += "Phys. DPS: " + dps.ToString("F1") + "\n";
            physDamage = "Phys. Damage: " + weaponItem.PhysicalDamage.min + "-" + weaponItem.PhysicalDamage.max + "\n";
        }
        if (hasElemental)
        {
            infoText.text += "Ele. DPS: " + String.Join(", ", elementalDps) + "\n";
        }
        if (hasPrimordial)
        {
            infoText.text += "Prim. DPS: " + String.Join(", ", primDps) + "\n";
        }
        infoText.text += "\n";
        infoText.text += physDamage;
        if (hasElemental)
        {
            infoText.text += "Ele. Damage: " + String.Join(", ", elementalDamage) + "\n";
        }
        if (hasPrimordial)
        {
            infoText.text += "Prim. Damage: " + String.Join(", ", primDamage) + "\n";
        }
        infoText.text += "\n";
        infoText.text += "Critical Chance: " + weaponItem.CriticalChance.ToString("F2") + "%\n";
        infoText.text += "Attacks per Second: " + weaponItem.AttackSpeed.ToString("F2") + "\n";
        infoText.text += "Range: " + weaponItem.WeaponRange.ToString("F2") + "\n";
    }

    public void OnEquipClick()
    {
        callback?.Invoke(equip);
    }
}