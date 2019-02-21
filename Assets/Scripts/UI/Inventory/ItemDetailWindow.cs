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
        this.GetComponent<Image>().color = Helpers.ReturnRarityColor(item.Rarity);
        nameText.text = item.Name;

        infoText.text = "";
        infoText.text += item.Base.group + "\n";
        if (item.armor != 0)
            infoText.text += "Armor: " + item.armor + "\n";
        if (item.shield != 0)
            infoText.text += "Shield: " + item.shield + "\n";
        if (item.dodgeRating != 0)
            infoText.text += "Dodge Rating: " + item.dodgeRating + "\n";
        if (item.resolveRating != 0)
            infoText.text += "Resolve: " + item.resolveRating + "\n";


        affixText.text = "";

        if (item.prefixes.Count > 0)
        {
            affixText.text += "Prefix\n";
            foreach (Affix a in item.prefixes)
            {
                foreach (AffixBonusBase b in a.Base.affixBonuses)
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
                foreach (AffixBonusBase b in a.Base.affixBonuses)
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
