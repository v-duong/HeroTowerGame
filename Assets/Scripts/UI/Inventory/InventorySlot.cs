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
        ItemDetailWindow itemWindow = UIManager.Instance.ItemWindow;
        itemWindow.gameObject.SetActive(true);
        itemWindow.item = item;
        itemWindow.inventorySlot = this;
        itemWindow.UpdateWindowEquipment();
    }
}
