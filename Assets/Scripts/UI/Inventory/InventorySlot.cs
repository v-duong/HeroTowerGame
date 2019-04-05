using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Item item;
    public Image slotImage;

    public void UpdateSlot()
    {
        slotImage.color = Helpers.ReturnRarityColor(item.Rarity);
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
