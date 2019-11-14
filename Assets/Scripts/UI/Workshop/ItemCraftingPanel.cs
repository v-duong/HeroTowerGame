using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCraftingPanel : MonoBehaviour
{
    public AffixedItem currentItem;
    private Func<bool> selectedOption = null;
    private Button currentSelectedButton = null;

    [SerializeField]
    private ItemCraftingSlot itemSlot;

    [SerializeField]
    private TextMeshProUGUI prefixes;

    [SerializeField]
    private TextMeshProUGUI suffixes;

    [SerializeField]
    private TextMeshProUGUI itemValue;

    [SerializeField]
    private TextMeshProUGUI leftInfo;

    [SerializeField]
    private TextMeshProUGUI rightInfo;

    [SerializeField]
    private TextMeshProUGUI playerFragmentsText;

    [SerializeField]
    private List<UIKeyButton> craftingButtons;

    private void OnDisable()
    {
        selectedOption = null;
        currentSelectedButton = null;
        currentItem = null;
    }

    private void OnEnable()
    {
        UpdatePanels();
    }

    public void ItemSlotOnClick()
    {
        UIManager.Instance.OpenInventoryWindow(false, false, false);
        UIManager.Instance.InvScrollContent.ShowEquipmentFiltered(x => x.IsEquipped == false, true, true);
        UIManager.Instance.InvScrollContent.SetCallback(ItemSlotOnClick_Callback);
    }

    public void ItemSlotOnClick_Callback(Item item)
    {
        if (item?.GetItemType() == ItemType.ARCHETYPE)
        {
            return;
        }
        currentItem = item as AffixedItem;
        UpdatePanels();
        UIManager.Instance.CloseCurrentWindow();
    }

    private void UpdatePanels()
    {
        itemSlot.text.text = "";
        prefixes.text = "";
        suffixes.text = "";
        leftInfo.text = "";
        rightInfo.text = "";
        playerFragmentsText.text = GameManager.Instance.PlayerStats.ItemFragments.ToString("N0") + " Frags";
        UpdateButtons();

        if (currentItem == null)
        {
            itemSlot.GetComponentInChildren<Image>().color = Helpers.ReturnRarityColor(RarityType.NORMAL);
            return;
        }

        itemSlot.text.text = currentItem.Name;
        itemSlot.GetComponentInChildren<Image>().color = Helpers.ReturnRarityColor(currentItem.Rarity);

        if (currentItem.Rarity == RarityType.UNIQUE)
        {
            prefixes.text = "Affixes\n";
        }
        else
        {
            int affixCap = currentItem.GetAffixCap();
            prefixes.text = "Prefixes (" + currentItem.prefixes.Count + " / " + affixCap + ")\n";

            suffixes.text = "Suffixes (" + currentItem.suffixes.Count + " / " + affixCap + ")\n";
        }

        foreach (Affix a in currentItem.prefixes)
        {
            prefixes.text += "○" + Affix.BuildAffixString(a.Base, 5, a, a.GetAffixValues(), a.GetEffectValues());
            //prefixes.text += "<align=\"right\">" + "T" + a.Base.tier + "</align>";
        }

        foreach (Affix a in currentItem.suffixes)
        {
            suffixes.text += "○" + Affix.BuildAffixString(a.Base, 5, a, a.GetAffixValues(), a.GetEffectValues());
        }

        itemValue.text = currentItem.GetItemValue() + " Fragments";

        if (currentItem.GetItemType() == ItemType.WEAPON)
        {
            UpdateInfo_Weapon(currentItem as Weapon);
        }
    }

    public void UpdateButtons()
    {
        foreach (UIKeyButton button in craftingButtons)
        {
            button.GetComponent<CraftingButton>().UpdateButton(currentItem);
        }
    }

    public void UpdateInfo_Weapon(Weapon weaponItem)
    {
        bool hasElemental = false;
        bool hasPrimordial = false;
        List<string> elementalDamage = new List<string>();
        List<string> elementalDps = new List<string>();
        List<string> primDamage = new List<string>();
        List<string> primDps = new List<string>();
        string physDamage = "";
        MinMaxRange range;
        float dps;

        for (int i = 1; i < (int)ElementType.DIVINE; i++)
        {
            range = weaponItem.GetWeaponDamage((ElementType)i);
            if (!range.IsZero())
            {
                hasElemental = true;
                dps = (range.min + range.max) / 2f * weaponItem.AttackSpeed;
                elementalDps.Add(LocalizationManager.BuildElementalDamageString(dps.ToString("F2"), (ElementType)i));
                elementalDamage.Add(LocalizationManager.BuildElementalDamageString(range.min + "-" + range.max, (ElementType)i));
            }
        }

        range = weaponItem.GetWeaponDamage(ElementType.DIVINE);
        if (!range.IsZero())
        {
            hasPrimordial = true;
            dps = (range.min + range.max) / 2f * weaponItem.AttackSpeed;
            primDps.Add(LocalizationManager.BuildElementalDamageString(dps.ToString("F2"), ElementType.DIVINE));
            primDamage.Add(LocalizationManager.BuildElementalDamageString(range.min + "-" + range.max, ElementType.DIVINE));
        }

        range = weaponItem.GetWeaponDamage(ElementType.VOID);
        if (!range.IsZero())
        {
            hasPrimordial = true;
            dps = (range.min + range.max) / 2f * weaponItem.AttackSpeed;
            primDps.Add(LocalizationManager.BuildElementalDamageString(dps.ToString("F2"), ElementType.VOID));
            primDamage.Add(LocalizationManager.BuildElementalDamageString(range.min + "-" + range.max, ElementType.VOID));
        }

        if (weaponItem.PhysicalDamage.min != 0 && weaponItem.PhysicalDamage.max != 0)
        {
            dps = (weaponItem.PhysicalDamage.min + weaponItem.PhysicalDamage.max) / 2f * weaponItem.AttackSpeed;
            leftInfo.text += "Phys. DPS: " + dps.ToString("F2") + "\n";
            physDamage = "Phys. Damage: " + weaponItem.PhysicalDamage.min + "-" + weaponItem.PhysicalDamage.max + "\n";
        }
        if (hasElemental)
        {
            leftInfo.text += "Ele. DPS: " + string.Join(", ", elementalDps) + "\n";
        }
        if (hasPrimordial)
        {
            leftInfo.text += "Prim. DPS: " + string.Join(", ", primDps) + "\n";
        }
        leftInfo.text += "\n";
        leftInfo.text += physDamage;
        if (hasElemental)
        {
            leftInfo.text += "Ele. Damage: " + string.Join(", ", elementalDamage) + "\n";
        }
        if (hasPrimordial)
        {
            leftInfo.text += "Prim. Damage: " + string.Join(", ", primDamage) + "\n";
        }
        rightInfo.text += "Critical Chance: " + weaponItem.CriticalChance.ToString("F2") + "%\n";
        rightInfo.text += "Attacks per Second: " + weaponItem.AttackSpeed.ToString("F2") + "\n";
        rightInfo.text += "Range: " + weaponItem.WeaponRange.ToString("F2") + "\n";
    }

    public void ShowAllPossibleAffixes()
    {
        if (currentItem == null)
            return;
        PopUpWindow popUpWindow = UIManager.Instance.PopUpWindow;
        popUpWindow.OpenTextWindow();
        popUpWindow.SetButtonValues("Close", delegate { UIManager.Instance.CloseCurrentWindow(); });
        popUpWindow.textField.text = "";
        popUpWindow.textField.fontSize = 16;
        popUpWindow.textField.lineSpacing = 16;

        WeightList<AffixBase> possibleAffixes;
        Dictionary<GroupType, float> weightModifiers = null;

        if (currentItem.GetAffixCap() > currentItem.prefixes.Count)
        {
            possibleAffixes = currentItem.GetAllPossiblePrefixes(weightModifiers);
            if (possibleAffixes.Count > 0)
            {
                popUpWindow.textField.text += "Prefixes\n";
                foreach (var affixBaseWeight in possibleAffixes)
                {
                    float affixPercent = (float)affixBaseWeight.weight / possibleAffixes.Sum * 100f;
                    popUpWindow.textField.text += affixPercent.ToString("F1") + "%" + Affix.BuildAffixString(affixBaseWeight.item, 15);
                }
                popUpWindow.textField.text += "\n";
            }
        }

        if (currentItem.GetAffixCap() > currentItem.suffixes.Count)
        {
            possibleAffixes = currentItem.GetAllPossibleSuffixes(weightModifiers);
            if (possibleAffixes.Count > 0)
            {
                popUpWindow.textField.text += "Suffixes\n";
                foreach (var affixBaseWeight in possibleAffixes)
                {
                    float affixPercent = (float)affixBaseWeight.weight / possibleAffixes.Sum * 100f;
                    popUpWindow.textField.text += affixPercent.ToString("F1") + "%" + Affix.BuildAffixString(affixBaseWeight.item, 15);
                }
            }
        }
    }

    public void RerollAffixOnClick()
    {
        if (currentItem == null)
            return;
        currentItem.RerollAffixesAtRarity();
        currentItem.RemoveAllAffixLocks();
        GameManager.Instance.PlayerStats.ModifyItemFragments(-AffixedItem.GetRerollAffixCost(currentItem));
        UpdatePanels();
    }

    public void RerollValuesOnClick()
    {
        if (currentItem == null)
            return;
        currentItem.RerollValues();
        currentItem.RemoveAllAffixLocks();
        GameManager.Instance.PlayerStats.ModifyItemFragments(-AffixedItem.GetRerollValuesCost(currentItem));
        UpdatePanels();
    }

    public void AddAffixOnClick()
    {
        if (currentItem == null)
            return;
        currentItem.AddRandomAffix();
        GameManager.Instance.PlayerStats.ModifyItemFragments(-AffixedItem.GetAddAffixCost(currentItem));
        UpdatePanels();
    }

    public void RemoveAffixOnClick()
    {
        if (currentItem == null)
            return;
        currentItem.RemoveRandomAffix();
        currentItem.RemoveAllAffixLocks();
        GameManager.Instance.PlayerStats.ModifyItemFragments(-AffixedItem.GetRemoveAffixCost(currentItem));
        UpdatePanels();
    }

    public void UpgradeRarityOnClick()
    {
        if (currentItem == null)
            return;
        currentItem.UpgradeRarity();
        GameManager.Instance.PlayerStats.ModifyItemFragments(-AffixedItem.GetUpgradeCost(currentItem));
        UpdatePanels();
    }

    public void ToNormalOnClick()
    {
        if (currentItem == null)
            return;
        currentItem.SetRarityToNormal();
        currentItem.RemoveAllAffixLocks();
        GameManager.Instance.PlayerStats.ModifyItemFragments(-AffixedItem.GetToNormalCost(currentItem));
        UpdatePanels();
    }

    public void LockAffixOnClick()
    {
        if (currentItem == null)
            return;
        PopUpWindow popUpWindow = UIManager.Instance.PopUpWindow;
        popUpWindow.OpenVerticalWindow();
        popUpWindow.SetButtonValues("Cancel", delegate { UIManager.Instance.CloseCurrentWindow(); });

        foreach (Affix affix in currentItem.prefixes.Concat(currentItem.suffixes))
        {
            if (affix.IsLocked)
                continue;
            Button button = Instantiate(UIManager.Instance.buttonPrefab);
            button.GetComponentInChildren<TextMeshProUGUI>().fontSize = 16;
            button.GetComponentInChildren<TextMeshProUGUI>().text = Affix.BuildAffixString(affix.Base, 5, null, affix.GetAffixValues(), affix.GetEffectValues());
            button.onClick.AddListener(delegate { LockAffixCallback(affix); });
            popUpWindow.AddObjectToViewport(button.gameObject);
        }
    }

    public void LockAffixCallback(Affix affix)
    {
        UIManager.Instance.CloseCurrentWindow();
        affix.SetAffixLock(true);
        GameManager.Instance.PlayerStats.ModifyItemFragments(-AffixedItem.GetLockCost(currentItem));
        UpdatePanels();
    }

    public void ApplyActionOnClick()
    {
        if (currentItem is Equipment equip && equip.IsEquipped)
            return;
        selectedOption?.Invoke();
        UpdatePanels();
    }
}