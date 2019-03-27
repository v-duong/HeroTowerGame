using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroEquipmentSlot : MonoBehaviour
{
    [SerializeField]
    private EquipSlotType slot;


    public void OnSlotClick()
    {
        Debug.Log(slot);
        HeroData hero = UIManager.Instance.HeroDetailWindow.hero;
        UIManager.Instance.InvWindowRect.gameObject.SetActive(true);
        if (slot == EquipSlotType.RING_SLOT_1 || slot == EquipSlotType.RING_SLOT_2)
        {
            UIManager.Instance.InvScrollContent.ShowEquipmentBySlotType(EquipSlotType.RING);
        }
        else
        {
            UIManager.Instance.InvScrollContent.ShowEquipmentBySlotType(slot);
        }
        
        
        //hero.EquipToSlot();
    }
}
