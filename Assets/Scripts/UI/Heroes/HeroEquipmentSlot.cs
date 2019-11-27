﻿using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HeroEquipmentSlot : MonoBehaviour
{
    [SerializeField]
    private EquipSlotType slot;

    public TextMeshProUGUI slotText;

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
            ui.InvScrollContent.ShowEquipmentFiltered(OffhandFilter, true, true);
        }
        else
        {
            ui.InvScrollContent.ShowEquipmentFiltered(x => x.IsEquipped == false && x.Base.equipSlot == slot, true, true);
        }

        ui.InvScrollContent.SetCallback(ItemSlotCallback);
    }

    public bool OffhandFilter(Equipment e)
    {
        if (e.IsEquipped)
            return false;
        if (e.Base.equipSlot == EquipSlotType.OFF_HAND)
            return true;

        GroupType weaponType = GroupType.SHIELD;

        if (HeroDetailWindow.hero.GetEquipmentInSlot(EquipSlotType.WEAPON) is Weapon mainWeapon)
        {
            if (mainWeapon.GetGroupTypes().Contains(GroupType.MELEE_WEAPON))
                weaponType = GroupType.MELEE_WEAPON;
            else if (mainWeapon.GetGroupTypes().Contains(GroupType.RANGED_WEAPON))
                weaponType = GroupType.RANGED_WEAPON;
        }

        System.Collections.Generic.HashSet<GroupType> groups = e.GetGroupTypes();

        if (groups.Contains(weaponType))
        {
            if (HeroDetailWindow.hero.HasSpecialBonus(BonusType.TWO_HANDED_WEAPONS_ARE_ONE_HANDED) && (groups.Contains(GroupType.TWO_HANDED_WEAPON) || groups.Contains(GroupType.ONE_HANDED_WEAPON)))
                return true;
            else if (groups.Contains(GroupType.ONE_HANDED_WEAPON))
                return true;
            else if (groups.Contains(GroupType.TWO_HANDED_WEAPON))
                return false;
        }

        return false;
    }

    public void ItemSlotCallback(Item item)
    {
        if (item is null)
        {
            UIManager.Instance.CloseCurrentWindow();
            UIManager.Instance.HeroDetailWindow.ItemEquip(item, slot);
        }
        else if (item is Equipment)
        {
            UIManager.Instance.OpenEquipmentDetailWindow(false, (Equipment)item, ItemEquipCallback, HeroDetailWindow.hero);
        }
    }

    public void ItemEquipCallback(Item item)
    {
        if (item is Equipment)
        {
            UIManager.Instance.CloseCurrentWindow();
            UIManager.Instance.CloseCurrentWindow();
            UIManager.Instance.HeroDetailWindow.ItemEquip(item, slot);
        }
    }
}