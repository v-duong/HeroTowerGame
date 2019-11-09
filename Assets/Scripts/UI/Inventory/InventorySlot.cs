using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Item item;
    public Image slotImage;
    public Image nameImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI groupText;
    public TextMeshProUGUI slotText;
    public Action<Item> onClickAction;
    public bool multiSelectMode = false;
    public bool alreadySelected = false;

    public void UpdateSlot()
    {
        infoText.text = "";
        groupText.text = "";
        slotText.text = "";
        if (item == null)
        {
            nameText.text = "REMOVE";
            slotImage.color = Color.cyan;
            return;
        }
        nameText.text = item.Name + "\n";

        switch (item.GetItemType())
        {
            case ItemType.ARMOR:
                Armor armor = item as Armor;
                groupText.text = LocalizationManager.Instance.GetLocalizationText_GroupType(armor.Base.group.ToString());
                slotText.text = LocalizationManager.Instance.GetLocalizationText_SlotType(armor.Base.equipSlot.ToString());
                infoText.text += "AR: " + armor.armor + "\n";
                infoText.text += "MS: " + armor.shield + "\n";
                infoText.text += "DR: " + armor.dodgeRating + "\n";
                infoText.text += "RR: " + armor.resolveRating + "\n";
                break;

            case ItemType.WEAPON:
                Weapon weapon = item as Weapon;
                groupText.text = LocalizationManager.Instance.GetLocalizationText_GroupType(weapon.Base.group.ToString());
                infoText.text += "PhyDPS: " + weapon.GetPhysicalDPS().ToString("n1") + "\n";
                infoText.text += "EleDPS: " + weapon.GetElementalDPS().ToString("n1") + "\n";
                infoText.text += "PrmDPS: " + weapon.GetPrimordialDPS().ToString("n1") + "\n";
                break;

            case ItemType.ACCESSORY:
                Accessory accessory = item as Accessory;
                groupText.text = LocalizationManager.Instance.GetLocalizationText_GroupType(accessory.Base.group.ToString());
                break;
            default:
                break;
        }

        slotImage.color = Helpers.ReturnRarityColor(item.Rarity);
        nameImage.color = Helpers.ReturnRarityColor(item.Rarity);
    }

    public void OnItemSlotClick()
    {
        if (onClickAction != null)
        {
            if (multiSelectMode)
            {
                alreadySelected = !alreadySelected;
                if (alreadySelected)
                    slotImage.color = Color.green;
                else
                    slotImage.color = Helpers.ReturnRarityColor(item.Rarity);
            }
            onClickAction?.Invoke(item);
            return;
        }
        else
        {
            if (item == null)
            {
                return;
            }
            switch (item.GetItemType())
            {
                case ItemType.ARMOR:
                case ItemType.WEAPON:
                case ItemType.ACCESSORY:
                    break;

                default:
                    return;
            }

            UIManager.Instance.OpenEquipmentDetailWindow(false, (Equipment)item, null);
        }
    }
}