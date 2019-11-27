using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Item item;
    public Image slotImage;
    public Image lockImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI infoText1;
    public TextMeshProUGUI infoText2;
    public TextMeshProUGUI groupText;
    public TextMeshProUGUI slotText;
    public TextMeshProUGUI baseItemText;
    public TextMeshProUGUI prefixText;
    public TextMeshProUGUI suffixText;
    public TextMeshProUGUI equippedToText;
    public GameObject affixParent;
    public Action<Item> onClickAction;
    public bool multiSelectMode = false;
    public bool alreadySelected = false;
    public bool showItemValue = false;

    public void UpdateSlot()
    {
        infoText1.text = "";
        infoText2.text = "";
        groupText.text = "";
        slotText.text = "";
        prefixText.text = "";
        suffixText.text = "";
        baseItemText.text = "";
        equippedToText.text = "";
        lockImage.gameObject.SetActive(false);
        if (item == null)
        {
            nameText.text = "REMOVE";
            slotImage.color = Color.cyan;
            return;
        }
        nameText.text = item.Name + "\n";

        switch (item.GetItemType())
        {
            case ItemType.ARMOR:
                Armor armor = item as Armor;
                groupText.text = LocalizationManager.Instance.GetLocalizationText_GroupType(armor.Base.group.ToString());
                slotText.text = LocalizationManager.Instance.GetLocalizationText_SlotType(armor.Base.equipSlot.ToString());
                infoText1.text += "AR: " + armor.armor + "\n";
                infoText1.text += "MS: " + armor.shield;
                infoText2.text += "DR: " + armor.dodgeRating + "\n";
                infoText2.text += "RR: " + armor.resolveRating;
                break;

            case ItemType.WEAPON:
                Weapon weapon = item as Weapon;
                groupText.text = LocalizationManager.Instance.GetLocalizationText_GroupType(weapon.Base.group.ToString());
                infoText1.text += "<sprite=0> DPS: " + weapon.GetPhysicalDPS().ToString("n1") + "\n";
                infoText1.text += "<sprite=7> DPS: " + weapon.GetElementalDPS().ToString("n1") + "\n";
                infoText1.text += "<sprite=8> DPS: " + weapon.GetPrimordialDPS().ToString("n1");
                infoText2.text += "Crit: " + weapon.CriticalChance.ToString("n2") + "%\n";
                infoText2.text += "APS: " + weapon.AttackSpeed.ToString("n2") + "/s\n";
                infoText2.text += "Range: " + weapon.WeaponRange.ToString("n2");
                break;

            case ItemType.ACCESSORY:
                Accessory accessory = item as Accessory;
                groupText.text = LocalizationManager.Instance.GetLocalizationText_GroupType(accessory.Base.group.ToString());
                break;

            default:
                break;
        }

        if (item is Equipment equip)
        {
            if (equip.IsEquipped)
                equippedToText.text = "Equipped to\n" + equip.equippedToHero.Name;

            baseItemText.text = "iLvl " + equip.ItemLevel;

            if (item.Rarity > RarityType.UNCOMMON && item.Rarity != RarityType.UNIQUE)
            {
                baseItemText.text += " " + equip.Base.idName;
            }
            /*
            if (equip.innate.Count > 0)
            {
                affixText.text += "<b>Innate</b>\n";
                foreach(var x in equip.innate)
                {
                    affixText.text += Affix.BuildAffixString(x.Base, 0, x, x.GetAffixValues(), x.GetEffectValues());
                }
            }
            */
            if (UIManager.Instance.InvScrollContent.showItemAffixes)
            {
                affixParent.SetActive(true);

                if (equip.prefixes.Count > 0)
                {
                    if (equip.Rarity == RarityType.UNIQUE)
                    {
                        prefixText.text += "<b>Affixes</b>\n";
                        suffixText.gameObject.SetActive(false);
                    }
                    else
                    {
                        prefixText.text += "<b>Prefixes</b>\n";
                    }
                    foreach (var x in equip.prefixes)
                    {
                        prefixText.text += "○ " + Affix.BuildAffixString(x.Base, 0, x, x.GetAffixValues(), x.GetEffectValues());
                    }
                }

                if (equip.suffixes.Count > 0)
                {
                    suffixText.gameObject.SetActive(true);
                    suffixText.text += "<b>Suffixes</b>\n";
                    foreach (var x in equip.suffixes)
                    {
                        suffixText.text += "○ " + Affix.BuildAffixString(x.Base, 0, x, x.GetAffixValues(), x.GetEffectValues());
                    }
                }
            } else
            {
                affixParent.SetActive(false);
            }
        }

        if (showItemValue)
            infoText1.text += "\n" + item.GetItemValue() + " Fragments";

        slotImage.color = Helpers.ReturnRarityColor(item.Rarity);
        //nameImage.color = Helpers.ReturnRarityColor(item.Rarity);
    }

    public void OnItemSlotClick()
    {
        if (onClickAction != null)
        {
            if (multiSelectMode)
            {
                alreadySelected = !alreadySelected;
                if (alreadySelected)
                    slotImage.color = Color.green;
                else
                    slotImage.color = Helpers.ReturnRarityColor(item.Rarity);
            }
            onClickAction?.Invoke(item);
            return;
        }
        else
        {
            if (item == null)
            {
                return;
            }
            switch (item.GetItemType())
            {
                case ItemType.ARMOR:
                case ItemType.WEAPON:
                case ItemType.ACCESSORY:
                    break;

                default:
                    return;
            }

            UIManager.Instance.OpenEquipmentDetailWindow(false, (Equipment)item, null);
        }
    }
}