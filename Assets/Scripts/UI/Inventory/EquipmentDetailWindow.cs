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

    public Equipment equip;
    public InventorySlot inventorySlot;

    public Image NameBackground;

    public void UpdateWindowEquipment()
    {
        this.GetComponent<Outline>().effectColor = Helpers.ReturnRarityColor(equip.Rarity);
        NameBackground.color = Helpers.ReturnRarityColor(equip.Rarity);
        nameText.text = equip.Name;
        infoText.text = "";
        infoText.text += LocalizationManager.Instance.GetLocalizationText("groupType." + equip.Base.group) + "\n";
        infoText.text += "Requirements: Lv" + equip.levelRequirement;
        if (equip.strRequirement > 0)
        {
            infoText.text += ", " + equip.strRequirement + " Str";
        }
        if (equip.intRequirement > 0)
        {
            infoText.text += ", " + equip.intRequirement + " Int";
        }
        if (equip.agiRequirement > 0)
        {
            infoText.text += ", " + equip.agiRequirement + " Agi";
        }
        if (equip.willRequirement > 0)
        {
            infoText.text += ", " + equip.willRequirement + " Will";
        }
        infoText.text += "\n";

        if (equip.GetItemType() == EquipmentType.ARMOR)
        {
            UpdateWindowEquipment_Armor((Armor)equip);
        }
        else if (equip.GetItemType() == EquipmentType.WEAPON)
        {
            UpdateWindowEquipment_Weapon((Weapon)equip);
        }

        affixText.text = "";

        if (equip.prefixes.Count > 0)
        {
            affixText.text += "Prefix\n";
            foreach (Affix a in equip.prefixes)
            {
                affixText.text += BuildAffixString(a);
            }
        }

        affixText.text += "\n";

        if (equip.suffixes.Count > 0)
        {
            affixText.text += "Suffix\n";
            foreach (Affix a in equip.suffixes)
            {
                affixText.text += BuildAffixString(a);
            }
        }
    }

    private static string BuildAffixString(Affix a)
    {
        string s = "○ ";
        foreach (AffixBonusProperty b in a.Base.affixBonuses)
        {
            if (b.bonusType.ToString().Contains("DAMAGE_MAX")) {
                continue;
            }
            if (b.bonusType.ToString().Contains("DAMAGE_MIN"))
            {

                s += "\t" + LocalizationManager.Instance.GetLocalizationText("bonusType." + b.bonusType) + " ";
                s += "+" + a.GetAffixValue(b.bonusType) + "-" + a.GetAffixValue(b.bonusType + 1) + "\n";
            }
            else
            {
                s += "\t" + LocalizationManager.Instance.GetLocalizationText_BonusType(b.bonusType, b.modifyType, a.GetAffixValue(b.bonusType));
            }
        }
        return s;
    }

    public void UpdateWindowEquipment_Armor(Armor armorItem)
    {
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
        if (weaponItem.physicalDamage.min != 0 && weaponItem.physicalDamage.max != 0)
        {
            double dps = (weaponItem.physicalDamage.min + weaponItem.physicalDamage.max) / 2d * weaponItem.attackSpeed;
            infoText.text += "Physical DPS: " + dps.ToString("F2") + "\n";
            infoText.text += "Damage: " + weaponItem.physicalDamage.min + "-" + weaponItem.physicalDamage.max + "\n";
        }
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
        equip.AddRandomAffix();
        UpdateWindowEquipment();
        inventorySlot.UpdateSlot();
    }

    public void OnUpgradeRarityClick()
    {
        equip.UpgradeRarity();
        UpdateWindowEquipment();
        inventorySlot.UpdateSlot();
    }

    public void OnResetClick()
    {
        equip.ClearAffixes();
        UpdateWindowEquipment();
        inventorySlot.UpdateSlot();
    }

    public void OnRemoveAffixClick()
    {
        equip.RemoveRandomAffix();
        UpdateWindowEquipment();
        inventorySlot.UpdateSlot();
    }

    public void OnRerollClick()
    {
        equip.RerollValues();
        UpdateWindowEquipment();
        inventorySlot.UpdateSlot();
    }

    public void OnRerollAffixClick()
    {
        equip.RerollAffixesAtRarity();
        UpdateWindowEquipment();
        inventorySlot.UpdateSlot();
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
        ui.HeroDetailWindow.hero.EquipToSlot(equip, ui.SlotContext);
        this.gameObject.SetActive(false);
        ui.CloseCurrentWindow();
        ui.HeroDetailWindow.UpdateWindow();
    }
}