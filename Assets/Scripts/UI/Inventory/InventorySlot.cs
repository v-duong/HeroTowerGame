using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Item item;
    public Image slotImage;

    // Start is called before the first frame update
    void Start()
    {
        item = new Equipment(ResourceManager.Instance.GetEquipmentBase(0), 1);
        slotImage.color = Helpers.ReturnRarityColor(item.rarity);
    }

    public void UpdateSlot()
    {
        slotImage.color = Helpers.ReturnRarityColor(item.rarity);
    }
}
