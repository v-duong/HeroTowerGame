using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Item item;
    public Image slotImage;
    public Text nameText;
    public Text infoText;

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
                double dps = (weapon.physicalDamage.min + weapon.physicalDamage.max) / 2d * weapon.attackSpeed;
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
            itemWindow.UpgradeButtonParent.SetActive(false);
        } else
        {
            itemWindow.EquipButtonParent.SetActive(false);
            itemWindow.UpgradeButtonParent.SetActive(true);
        }
        itemWindow.gameObject.SetActive(true);
        itemWindow.item = (Equipment)item;
        itemWindow.inventorySlot = this;
        itemWindow.UpdateWindowEquipment();
    }
}
