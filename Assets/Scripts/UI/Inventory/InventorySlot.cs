﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class InventorySlot : MonoBehaviour
{
    public Item item;
    public Image slotImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI infoText;
    public Action<Item> onClickAction;

    public void UpdateSlot()
    {
        infoText.text = "";
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
                nameText.text += armor.Base.group;
                infoText.text += "AR: " + armor.armor + "\n";
                infoText.text += "MS: " + armor.shield + "\n";
                infoText.text += "DR: " + armor.dodgeRating + "\n";
                infoText.text += "RR: " + armor.resolveRating + "\n";
                break;
            case ItemType.WEAPON:
                Weapon weapon = item as Weapon;
                nameText.text += weapon.Base.group;
                infoText.text += "PhyDPS: " + weapon.GetPhysicalDPS().ToString("F1") + "\n";
                infoText.text += "EleDPS: " + weapon.GetElementalDPS().ToString("F1") + "\n";
                infoText.text += "PrmDPS: " + weapon.GetPrimordialDPS().ToString("F1") + "\n";
                break;
            default:
                break;
        }

        slotImage.color = Helpers.ReturnRarityColor(item.Rarity);
    }

    public void OnItemSlotClick()
    {
        if (onClickAction != null)
        {
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
            EquipmentDetailWindow itemWindow = UIManager.Instance.EquipDetailWindow;
            itemWindow.SetTransform(0);
            UIManager.Instance.OpenWindow(itemWindow.gameObject, false);
            itemWindow.equip = (Equipment)item;
            itemWindow.inventorySlot = this;
            itemWindow.UpdateWindowEquipment();
        }
    }
}
