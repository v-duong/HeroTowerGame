using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HeroEquipmentSlot : MonoBehaviour
{
    [SerializeField]
    private EquipSlotType slot;
    public Text slotText;

    public EquipSlotType EquipSlot => slot;

    public void OnSlotClick()
    {
        UIManager ui = UIManager.Instance;
        HeroData hero = HeroDetailWindow.hero;
        ui.OpenInventoryWindow(false, false);
        ui.IsEquipSelectMode = true;
        ui.SlotContext = slot;
        if (slot == EquipSlotType.RING_SLOT_1 || slot == EquipSlotType.RING_SLOT_2)
        {
            ui.InvScrollContent.ShowEquipmentBySlotType(EquipSlotType.RING);
        }
        else
        {
            ui.InvScrollContent.ShowEquipmentBySlotType(slot);
        }
    }

    
}
