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
        nameText.text = LocalizationManager.Instance.GetLocalizationText("equipment." + item.Base.idName) ?? item.Base.idName;
        infoText.text = "";
        infoText.text += item.Base.group + "\n";

        if (item.GetItemType() == ItemType.ARMOR)
        {
            UpdateWindowEquipment_Armor((Armor)item);
        }
        else if (item.GetItemType() == ItemType.WEAPON)
        {
            UpdateWindowEquipment_Weapon((Weapon)item);
        }

        affixText.text = "";

        if (item.prefixes.Count > 0)
        {
            affixText.text += "Prefix\n";
            foreach (Affix a in item.prefixes)
            {
                affixText.text += BuildAffixString(a);
            }
        }

        affixText.text += "\n";

        if (item.suffixes.Count > 0)
        {
            affixText.text += "Suffix\n";
            foreach (Affix a in item.suffixes)
            {
                affixText.text += BuildAffixString(a);
            }
        }
    }

    private static string BuildAffixString(Affix a)
    {
        string s = "";
        foreach (AffixBonusProperty b in a.Base.affixBonuses)
        {
            if (b.bonusType.ToString().Contains("DAMAGE_MAX")) {
                continue;
            }
            if (b.bonusType.ToString().Contains("DAMAGE_MIN"))
            {
                s += "\t" + (LocalizationManager.Instance.GetLocalizationText("bonusType." + b.bonusType) ?? b.bonusType.ToString()) + " ";
                s += "+" + a.GetAffixValue(b.bonusType) + "-" + a.GetAffixValue(b.bonusType+1) + "\n";
            }
            else
            {
                s += "\t" + (LocalizationManager.Instance.GetLocalizationText("bonusType." + b.bonusType) ?? b.bonusType.ToString()) + " ";

                if (b.modifyType == ModifyType.FLAT_ADDITION)
                {
                    s += "+" + a.GetAffixValue(b.bonusType) + "\n";
                }
                else if (b.modifyType == ModifyType.ADDITIVE)
                {
                    s += "+" + a.GetAffixValue(b.bonusType) + "%" + "\n";
                }
                else if (b.modifyType == ModifyType.MULTIPLY)
                {
                    s += "x" + (1 + a.GetAffixValue(b.bonusType) / 100d).ToString("F2") + "\n";
                }
                else if (b.modifyType == ModifyType.SET)
                {
                    s += "is" + a.GetAffixValue(b.bonusType) + "\n";
                }
            }
        }
        return s;
    }

    public void UpdateWindowEquipment_Armor(Armor armorItem)
    {
        this.GetComponent<Image>().color = Helpers.ReturnRarityColor(armorItem.Rarity);

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

        if (weaponItem.physicalDamage.min != 0 && weaponItem.physicalDamage.max != 0)
            infoText.text += "Damage: " + weaponItem.physicalDamage.min + "-" + weaponItem.physicalDamage.max + "\n";
        infoText.text += "Critical Chance: " + weaponItem.criticalChance.ToString("F2") + "%\n";
        infoText.text += "Attacks per Second: " + weaponItem.attackSpeed.ToString("F2") + "\n";
        infoText.text += "Range: " + weaponItem.weaponRange.ToString("F2") + "\n";
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