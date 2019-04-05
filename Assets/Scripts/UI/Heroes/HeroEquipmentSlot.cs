using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroEquipmentSlot : MonoBehaviour
{
    [SerializeField]
    private EquipSlotType slot;


    public void OnSlotClick()
    {
        UIManager ui = UIManager.Instance;
        Debug.Log(slot);
        HeroData hero = ui.HeroDetailWindow.hero;
        ui.InvWindowRect.gameObject.SetActive(true);

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
