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
            ui.InvScrollContent.ShowEquipmentFiltered(x => x.Base.equipSlot == EquipSlotType.RING, true);
        }
        else if (slot == EquipSlotType.OFF_HAND)
        {
            ui.InvScrollContent.ShowEquipmentFiltered(
                x => (x.IsEquipped == false) &&
                ((x.Base.equipSlot == EquipSlotType.OFF_HAND) ||
                (x.Base.equipSlot == EquipSlotType.WEAPON && x.GetGroupTypes().Contains(GroupType.ONE_HANDED_WEAPON))), true);
        }
        else
        {
            ui.InvScrollContent.ShowEquipmentFiltered(x => x.Base.equipSlot == slot, true);
        }
    }
}