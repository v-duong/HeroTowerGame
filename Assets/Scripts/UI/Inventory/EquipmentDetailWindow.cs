using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentDetailWindow : MonoBehaviour
{
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text affixText;
    [SerializeField]
    private Text infoText;
    [SerializeField]
    public GameObject UpgradeButtonParent;
    [SerializeField]
    public GameObject EquipButtonParent;
    public Equipment item;
    public InventorySlot inventorySlot;

    public void UpdateWindowEquipment()
    {
        infoText.text = "";
        infoText.text += item.Base.group + "\n";
        infoText.text += item.GetType() + "\n";

        if (item.GetItemType() == ItemType.ARMOR)
        {
            UpdateWindowEquipment_Armor((Armor)item);
        } else if (item.GetItemType() == ItemType.WEAPON)
        {
            UpdateWindowEquipment_Weapon((Weapon)item);
        }

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
                    affixText.text += "\t" + a.Base.name + " " + b.bonusType + " " + a.GetAffixValue(b.bonusType) + " [" + b.minValue + ", " + b.maxValue + "]" + "\n";
                }
            }
        }
    }

    public void UpdateWindowEquipment_Armor(Armor armorItem)
    {

        this.GetComponent<Image>().color = Helpers.ReturnRarityColor(armorItem.Rarity);
        nameText.text = armorItem.Name;

        if (armorItem.armor != 0)
            infoText.text += "Armor: " + armorItem.armor + "\n";
        if (armorItem.shield != 0)
            infoText.text += "Shield: " + armorItem.shield + "\n";
        if (armorItem.dodgeRating != 0)
            infoText.text += "Dodge Rating: " + armorItem.dodgeRating + "\n";
        if (armorItem.resolveRating != 0)
            infoText.text += "Resolve: " + armorItem.resolveRating + "\n";
    }

    public void UpdateWindowEquipment_Weapon(Weapon weaponItem)
    {
        this.GetComponent<Image>().color = Helpers.ReturnRarityColor(weaponItem.Rarity);
        nameText.text = weaponItem.Name;

        if (weaponItem.minDamage != 0 && weaponItem.maxDamage != 0)
            infoText.text += "Damage: " + weaponItem.minDamage + "-" + weaponItem.maxDamage + "\n";
        infoText.text += "Critical Chance: " + weaponItem.criticalChance + "\n";
        infoText.text += "Range: " + weaponItem.weaponRange + "\n";

    }

    public void ShowButtons()
    {
        UpgradeButtonParent.SetActive(true);
    }

    public void HideButtons()
    {
        UpgradeButtonParent.SetActive(false);
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

    public void SetTransform(int type)
    {
        UIManager ui = UIManager.Instance;
        if (type == 0)
        {
            RectTransform t = this.GetComponent<RectTransform>();
            t.sizeDelta = ui.itemWindowSize;
            t.anchoredPosition = new Vector2((ui.referenceResolution.x - ui.itemWindowSize.x) / 2, (ui.itemWindowSize.y - ui.referenceResolution.y) / 2);
        }
    }

    public void OnEquipClick()
    {
        UIManager ui = UIManager.Instance;
        ui.HeroDetailWindow.hero.EquipToSlot(item, ui.SlotContext);
        ui.CloseInventoryWindows();
        ui.HeroDetailWindow.UpdateWindow();
    }
}
