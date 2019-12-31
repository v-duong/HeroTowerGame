using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HeroDetailEquipmentPage : MonoBehaviour, IUpdatablePanel
{
    public List<HeroEquipmentSlot> equipSlots;
    private HeroEquipmentSlot offHandSlot;

    private HeroEquipmentSlot OffHandSlot
    {
        get
        {
            if (offHandSlot == null)
            {
                offHandSlot = equipSlots.Find(x => x.EquipSlot == EquipSlotType.OFF_HAND);
            }
            return offHandSlot;
        }
    }

    private HeroData hero;

    private void OnEnable()
    {
        UpdateWindow();
    }

    public void UpdateWindow()
    {
        bool skipOffhandUpdate = false;
        hero = HeroDetailWindow.hero;
        foreach (HeroEquipmentSlot slot in equipSlots)
        {
            slot.slotText.text = LocalizationManager.Instance.GetLocalizationText("equipSlotType." + slot.EquipSlot);
            Equipment e = hero.GetEquipmentInSlot(slot.EquipSlot);

            if (slot.EquipSlot == EquipSlotType.OFF_HAND && skipOffhandUpdate)
                continue;

            if (e == null)
            {
                slot.slotBase.ClearSlot();
                int equippableCount = 0;
                if (slot.EquipSlot == EquipSlotType.OFF_HAND)
                {
                    equippableCount = GameManager.Instance.PlayerStats.EquipmentInventory.Where(x => hero.CanEquipItem(x) && HeroEquipmentSlot.OffhandFilter(x)).Count();
                }
                else if (slot.EquipSlot == EquipSlotType.RING_SLOT_1 || slot.EquipSlot == EquipSlotType.RING_SLOT_2)
                {
                    equippableCount = GameManager.Instance.PlayerStats.EquipmentInventory.Where(x => !x.IsEquipped && x.Base.equipSlot == EquipSlotType.RING && hero.CanEquipItem(x)).Count();
                }
                else
                {
                    equippableCount = GameManager.Instance.PlayerStats.EquipmentInventory.Where(x => !x.IsEquipped && x.Base.equipSlot == slot.EquipSlot && hero.CanEquipItem(x)).Count();
                }
                slot.slotBase.groupText.text = equippableCount + " Equippable Items";
            }
            else
            {
                slot.slotBase.item = e;
                slot.slotBase.UpdateSlot();
            }

            if (slot.EquipSlot == EquipSlotType.WEAPON && e is Weapon)
            {
                if (!hero.GetEquipmentGroupTypes(e).Contains(GroupType.TWO_HANDED_WEAPON) ||
                    hero.HasSpecialBonus(BonusType.TWO_HANDED_WEAPONS_ARE_ONE_HANDED) ||
                    (hero.HasSpecialBonus(BonusType.CAN_USE_SPEARS_WITH_SHIELDS) && e.GetGroupTypes().Contains(GroupType.SPEAR)))
                {
                    OffHandSlot.GetComponent<Button>().interactable = true;
                }
                else
                {
                    OffHandSlot.GetComponent<Button>().interactable = false;
                    OffHandSlot.slotBase.item = e;
                    OffHandSlot.slotBase.UpdateSlot();
                    skipOffhandUpdate = true;
                }
            }
        }
    }
}