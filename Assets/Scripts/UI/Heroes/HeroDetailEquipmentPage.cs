using System.Collections.Generic;
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
            }
            else
            {
                slot.slotBase.item = e;
                slot.slotBase.UpdateSlot(true);
            }

            if (slot.EquipSlot == EquipSlotType.WEAPON)
            {
                if (e is Weapon && hero.GetEquipmentGroupTypes(e).Contains(GroupType.TWO_HANDED_WEAPON) && !hero.HasSpecialBonus(BonusType.TWO_HANDED_WEAPONS_ARE_ONE_HANDED))
                {
                    OffHandSlot.GetComponent<Button>().interactable = false;
                    OffHandSlot.slotBase.item = e;
                    OffHandSlot.slotBase.UpdateSlot(true);
                    skipOffhandUpdate = true;
                }
                else
                {
                    OffHandSlot.GetComponent<Button>().interactable = true;
                }
            }
        }
    }
}