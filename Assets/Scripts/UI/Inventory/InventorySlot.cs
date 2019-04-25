using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public AffixedItem item;
    public Image slotImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI infoText;

    public void UpdateSlot()
    {
        slotImage.color = Helpers.ReturnRarityColor(item.Rarity);
        nameText.text = item.Name;
        infoText.text = "";
        

        switch(item.GetItemType())
        {
            case EquipmentType.ARMOR:
                Armor armor = item as Armor;
                infoText.text += "AR: " + armor.armor + "\n";
                infoText.text += "MS: " + armor.shield + "\n";
                infoText.text += "DR: " + armor.dodgeRating + "\n";
                infoText.text += "RR: " + armor.resolveRating + "\n";
                break;
            case EquipmentType.WEAPON:
                Weapon weapon = item as Weapon;
                double dps = (weapon.PhysicalDamage.min + weapon.PhysicalDamage.max) / 2d * weapon.AttackSpeed;
                infoText.text += "PDPS: " + dps.ToString("F2") + "\n";
                break;
            default:
                break;
        }

    }

    public void OnItemSlotClick()
    {
        EquipmentDetailWindow itemWindow = UIManager.Instance.EquipDetailWindow;
        itemWindow.SetTransform(0);
        if (UIManager.Instance.IsEquipSelectMode)
        {
            itemWindow.EquipButtonParent.SetActive(true);
            itemWindow.HideButtons();
        } else
        {
            itemWindow.EquipButtonParent.SetActive(false);
            itemWindow.ShowButtons();
        }
        itemWindow.gameObject.SetActive(true);
        itemWindow.equip = (Equipment)item;
        itemWindow.inventorySlot = this;
        itemWindow.UpdateWindowEquipment();
    }
}
