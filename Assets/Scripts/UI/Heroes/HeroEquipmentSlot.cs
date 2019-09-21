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
        ui.OpenInventoryWindow(false, false, false);
        if (slot == EquipSlotType.RING_SLOT_1 || slot == EquipSlotType.RING_SLOT_2)
        {
            ui.InvScrollContent.ShowEquipmentFiltered(x => x.IsEquipped == false && x.Base.equipSlot == EquipSlotType.RING, true, true);
        }
        else if (slot == EquipSlotType.OFF_HAND)
        {
            GroupType weaponType = GroupType.ONE_HANDED_WEAPON;

            if (hero.GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon mainWeapon)
            {
                if (mainWeapon.GetGroupTypes().Contains(GroupType.MELEE_WEAPON))
                    weaponType = GroupType.MELEE_WEAPON;
                else if (mainWeapon.GetGroupTypes().Contains(GroupType.RANGED_WEAPON))
                    weaponType = GroupType.RANGED_WEAPON;
            }

            ui.InvScrollContent.ShowEquipmentFiltered(
                x => (x.IsEquipped == false) &&
                ((x.Base.equipSlot == EquipSlotType.OFF_HAND) ||
                (x.GetGroupTypes().Contains(GroupType.ONE_HANDED_WEAPON) && x.GetGroupTypes().Contains(weaponType))), true, true);
        }
        else
        {
            ui.InvScrollContent.ShowEquipmentFiltered(x => x.IsEquipped == false && x.Base.equipSlot == slot, true, true);
        }

        ui.InvScrollContent.SetCallback(ItemEquipCallback);
    }

    public void ItemEquipCallback(Item item)
    {
        if (item is Equipment || item is null)
        {
            UIManager.Instance.CloseCurrentWindow();
            UIManager.Instance.HeroDetailWindow.ItemEquip(item, slot);
        }
    }
}