using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailWindow : MonoBehaviour
{
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text affixText;
    [SerializeField]
    private Text infoText;
    public Equipment item;
    public InventorySlot inventorySlot;

    public void UpdateWindowEquipment()
    {
        Armor armorItem = (Armor)item;

        this.GetComponent<Image>().color = Helpers.ReturnRarityColor(armorItem.Rarity);
        nameText.text = armorItem.Name;

        infoText.text = "";
        infoText.text += armorItem.Base.group + "\n";
        if (armorItem.armor != 0)
            infoText.text += "Armor: " + armorItem.armor + "\n";
        if (armorItem.shield != 0)
            infoText.text += "Shield: " + armorItem.shield + "\n";
        if (armorItem.dodgeRating != 0)
            infoText.text += "Dodge Rating: " + armorItem.dodgeRating + "\n";
        if (armorItem.resolveRating != 0)
            infoText.text += "Resolve: " + armorItem.resolveRating + "\n";


        affixText.text = "";

        if (item.prefixes.Count > 0)
        {
            affixText.text += "Prefix\n";
            foreach (Affix a in item.prefixes)
            {
                foreach (AffixBonusProperty b in a.Base.affixBonuses)
                {
                    affixText.text += "\t" + a.Base.name + " " + b.bonusType + " " + a.GetAffixValue(b.bonusType) + " [" + b.minValue + ", " + b.maxValue + "]" + "\n";
                }
            }
        }

        affixText.text += "\n";

        if (item.suffixes.Count > 0)
        {
            affixText.text += "Suffix\n";
            foreach (Affix a in item.suffixes)
            {
                foreach (AffixBonusProperty b in a.Base.affixBonuses)
                {
                    affixText.text += "\t" + a.Base.name + " " + b.bonusType + " " + a.GetAffixValue(b.bonusType)  +" [" + b.minValue + ", " + b.maxValue + "]" + "\n";
                }
            }
        }
    }

    public void OnAddAffixClick()
    {
        item.AddRandomAffix();
        UpdateWindowEquipment();
    }

    public void OnUpgradeRarityClick()
    {
        item.UpgradeRarity();
        UpdateWindowEquipment();
        inventorySlot.UpdateSlot();
    }

    public void OnResetClick()
    {
        item.ClearAffixes();
        UpdateWindowEquipment();
        inventorySlot.UpdateSlot();
    }

    public void OnRemoveAffixClick()
    {
        item.RemoveRandomAffix();
        UpdateWindowEquipment();
    }

    public void OnRerollClick()
    {
        item.RerollValues();
        UpdateWindowEquipment();
    }

    public void OnRerollAffixClick()
    {
        item.RerollAffixesAtRarity();
        UpdateWindowEquipment();
    }
}
