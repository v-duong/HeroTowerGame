﻿using System;
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
    private bool showAffixDetails = false;

    [SerializeField]
    private ItemCraftingSlot itemSlot;

    [SerializeField]
    private TextMeshProUGUI innates;

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
    private Image topPanelBackground;

    [SerializeField]
    private TextMeshProUGUI playerFragmentsText;

    [SerializeField]
    private TextMeshProUGUI showAffixDetailsButtonText;

    [SerializeField]
    private List<UIKeyButton> craftingButtons;

    [SerializeField]
    private CraftingPanelAffixHeader innateHeader;

    [SerializeField]
    private CraftingPanelAffixHeader prefixHeader;

    [SerializeField]
    private CraftingPanelAffixHeader suffixHeader;

    private void OnDisable()
    {
        selectedOption = null;
        currentSelectedButton = null;
        currentItem = null;
        showAffixDetails = false;
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
        itemSlot.levelText.text = "";
        //itemSlot.itemBaseText.text = "";
        itemSlot.itemRequirementText.text = "";
        prefixes.text = "";
        suffixes.text = "";
        innates.text = "";
        leftInfo.text = "";
        rightInfo.text = "";
        itemValue.text = "";
        innateHeader.gameObject.SetActive(false);
        prefixHeader.gameObject.SetActive(false);
        suffixHeader.gameObject.SetActive(false);
        playerFragmentsText.text = GameManager.Instance.PlayerStats.ItemFragments.ToString("N0") + " <sprite=10>";
        UpdateButtons();

        if (currentItem == null)
        {
            itemSlot.text.text = "Select an Item";
            itemSlot.GetComponentInChildren<Image>().color = Helpers.ReturnRarityColor(RarityType.NORMAL);
            topPanelBackground.color = Helpers.ReturnRarityColor(RarityType.NORMAL);
            return;
        }

        itemSlot.text.text = "<b><smallcaps>" + currentItem.Name + "</smallcaps></b>";
        itemSlot.levelText.text = "ItemLevel<b><size=120%>" + currentItem.ItemLevel + "</size></b>";
        itemSlot.GetComponentInChildren<Image>().color = Helpers.ReturnRarityColor(currentItem.Rarity);
        topPanelBackground.color = Helpers.ReturnRarityColor(currentItem.Rarity);

        if (currentItem.Rarity == RarityType.UNIQUE)
        {
            prefixHeader.SetHeaderValues(0, 0, "Affixes", true);
            prefixHeader.gameObject.SetActive(true);
            suffixHeader.gameObject.SetActive(false);
        }
        else
        {
            prefixHeader.gameObject.SetActive(true);
            suffixHeader.gameObject.SetActive(true);
            int affixCap = currentItem.GetAffixCap();
            //prefixes.text = "<b>Prefixes</b> (" + currentItem.prefixes.Count + " / " + affixCap + ")\n";
            //suffixes.text = "<b>Suffixes</b> (" + currentItem.suffixes.Count + " / " + affixCap + ")\n";
            prefixHeader.SetHeaderValues(currentItem.prefixes.Count, affixCap, "Prefixes");
            suffixHeader.SetHeaderValues(currentItem.suffixes.Count, affixCap, "Suffixes");
        }

        if (currentItem is Equipment equip)
        {
            if (equip.innate.Count > 0)
            {
                innateHeader.gameObject.SetActive(true);
                //innates.text = "<b>Innate</b>\n";
                foreach (Affix a in equip.innate)
                {
                    innates.text += "○" + Affix.BuildAffixString(a.Base, Helpers.AFFIX_STRING_SPACING, a, a.GetAffixValues(), a.GetEffectValues());
                }
            }
            else
            {
                innateHeader.gameObject.SetActive(false);
            }


            //itemSlot.itemBaseText.text = equip.Base.idName;
            if (equip.Rarity != RarityType.UNIQUE)
                itemSlot.text.text += "\n<i><size=80%>" + equip.Base.idName + "</size></i>";
            itemSlot.itemRequirementText.text = LocalizationManager.Instance.GetRequirementText(equip);
        }

        foreach (Affix a in currentItem.prefixes)
        {
            string prefixText = "○" + Affix.BuildAffixString(a.Base, Helpers.AFFIX_STRING_SPACING, a, a.GetAffixValues(), a.GetEffectValues(), showAffixDetails, showAffixDetails && currentItem.Rarity != RarityType.UNIQUE);
            prefixes.text += prefixText;
        }

        foreach (Affix a in currentItem.suffixes)
        {
            suffixes.text += "○" + Affix.BuildAffixString(a.Base, Helpers.AFFIX_STRING_SPACING, a, a.GetAffixValues(), a.GetEffectValues(), showAffixDetails, showAffixDetails && currentItem.Rarity != RarityType.UNIQUE);
        }

        itemValue.text = "Item Value\n" + currentItem.GetItemValue() + " <sprite=10>";

        if (currentItem.GetItemType() == ItemType.WEAPON)
        {
            UpdateInfo_Weapon(currentItem as Weapon);
        }
    }

    public void UpdateButtons()
    {
        if (showAffixDetails)
        {
            showAffixDetailsButtonText.text = "Hide Affix Details";
        }
        else
        {
            showAffixDetailsButtonText.text = "Show Affix Details";
        }

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
                elementalDamage.Add("<nobr>" + LocalizationManager.BuildElementalDamageString(range.min + "-" + range.max, (ElementType)i) + "</nobr>");
            }
        }

        for (int i = (int)ElementType.DIVINE; i <= (int)ElementType.VOID; i++)
        {
            range = weaponItem.GetWeaponDamage((ElementType)i);
            if (!range.IsZero())
            {
                hasPrimordial = true;
                dps = (range.min + range.max) / 2f * weaponItem.AttackSpeed;
                primDps.Add(LocalizationManager.BuildElementalDamageString(dps.ToString("F2"), (ElementType)i));
                primDamage.Add("<nobr>" + LocalizationManager.BuildElementalDamageString(range.min + "-" + range.max, (ElementType)i) + "</nobr>");
            }
        }

        if (weaponItem.PhysicalDamage.min != 0 && weaponItem.PhysicalDamage.max != 0)
        {
            dps = (weaponItem.PhysicalDamage.min + weaponItem.PhysicalDamage.max) / 2f * weaponItem.AttackSpeed;
            leftInfo.text += "pDPS: " + dps.ToString("F2") + "\n";
            physDamage = "pDmg: " + weaponItem.PhysicalDamage.min + "-" + weaponItem.PhysicalDamage.max + "\n";
        }
        if (hasElemental)
        {
            leftInfo.text += "eDPS: " + string.Join(", ", elementalDps) + "\n";
        }
        if (hasPrimordial)
        {
            leftInfo.text += "aDPS: " + string.Join(", ", primDps) + "\n";
        }
        leftInfo.text += "\n";
        leftInfo.text += physDamage;
        if (hasElemental)
        {
            leftInfo.text += "eDmg: " + string.Join(", ", elementalDamage) + "\n";
        }
        if (hasPrimordial)
        {
            leftInfo.text += "aDmg: " + string.Join(", ", primDamage) + "\n";
        }
        rightInfo.text += "Critical Chance\n<b>" + weaponItem.CriticalChance.ToString("F2") + "%</b>\n";
        rightInfo.text += "Attacks / s\n<b>" + weaponItem.AttackSpeed.ToString("F2") + "/s</b>\n";
        rightInfo.text += "Range\n<b>" + weaponItem.WeaponRange.ToString("F2") + "</b>\n";
    }

    public void ShowAllPossibleAffixes()
    {
        if (currentItem == null)
            return;
        PopUpWindow popUpWindow = UIManager.Instance.PopUpWindow;
        popUpWindow.OpenTextWindow("");
        popUpWindow.SetButtonValues(null, null, "Close", delegate { UIManager.Instance.CloseCurrentWindow(); });
        popUpWindow.textField.text = "";
        popUpWindow.textField.fontSize = 18;
        popUpWindow.textField.paragraphSpacing = 8;
        popUpWindow.textField.alignment = TextAlignmentOptions.Left;

        WeightList<AffixBase> possibleAffixes;
        Dictionary<GroupType, float> weightModifiers = null;

        if (currentItem.GetAffixCap() > currentItem.prefixes.Count)
        {
            possibleAffixes = currentItem.GetAllPossiblePrefixes(weightModifiers);
            if (possibleAffixes.Count > 0)
            {
                popUpWindow.textField.text += "<b>Prefixes</b>\n";
                foreach (var affixBaseWeight in possibleAffixes)
                {
                    float affixPercent = (float)affixBaseWeight.weight / possibleAffixes.Sum * 100f;
                    popUpWindow.textField.text += affixPercent.ToString("F1") + "%" + Affix.BuildAffixString(affixBaseWeight.item, Helpers.AFFIX_STRING_SPACING + 3);
                    popUpWindow.textField.text += "<line-height=0.2em>\n</line-height>";
                }
                popUpWindow.textField.text += "\n";
            }
        }

        if (currentItem.GetAffixCap() > currentItem.suffixes.Count)
        {
            possibleAffixes = currentItem.GetAllPossibleSuffixes(weightModifiers);
            if (possibleAffixes.Count > 0)
            {
                popUpWindow.textField.text += "<b>Suffixes</b>\n";
                foreach (var affixBaseWeight in possibleAffixes)
                {
                    float affixPercent = (float)affixBaseWeight.weight / possibleAffixes.Sum * 100f;
                    popUpWindow.textField.text += affixPercent.ToString("F1") + "%" + Affix.BuildAffixString(affixBaseWeight.item, Helpers.AFFIX_STRING_SPACING + 3);
                    popUpWindow.textField.text += "<line-height=0.1em>\n</line-height>";
                }
            }
        }
    }

    public void ShowAffixDetailsToggle()
    {
        showAffixDetails = !showAffixDetails;
        UpdatePanels();
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
        GameManager.Instance.PlayerStats.ModifyItemFragments(-AffixedItem.GetRemoveAffixCost(currentItem));
        currentItem.RemoveRandomAffix();
        currentItem.RemoveAllAffixLocks();
        UpdatePanels();
    }

    public void UpgradeRarityOnClick()
    {
        if (currentItem == null)
            return;
        GameManager.Instance.PlayerStats.ModifyItemFragments(-AffixedItem.GetUpgradeCost(currentItem));
        currentItem.UpgradeRarity();
        UpdatePanels();
    }

    public void ToNormalOnClick()
    {
        if (currentItem == null)
            return;
        GameManager.Instance.PlayerStats.ModifyItemFragments(-AffixedItem.GetToNormalCost(currentItem));
        currentItem.SetRarityToNormal();
        currentItem.RemoveAllAffixLocks();
        UpdatePanels();
    }

    public void LockAffixOnClick()
    {
        if (currentItem == null)
            return;
        PopUpWindow popUpWindow = UIManager.Instance.PopUpWindow;
        popUpWindow.OpenVerticalWindow();

        popUpWindow.SetButtonValues(null, null, "Cancel", delegate { UIManager.Instance.CloseCurrentWindow(); });

        foreach (Affix affix in currentItem.prefixes.Concat(currentItem.suffixes))
        {
            if (affix.IsLocked)
                continue;
            Button button = Instantiate(UIManager.Instance.buttonPrefab);
            button.GetComponentInChildren<TextMeshProUGUI>().fontSize = 18;
            button.GetComponentInChildren<TextMeshProUGUI>().text = Affix.BuildAffixString(affix.Base, 0, null, affix.GetAffixValues(), affix.GetEffectValues());
            button.onClick.AddListener(delegate { LockAffixCallback(affix); });
            popUpWindow.AddObjectToViewport(button.gameObject);
        }
    }

    public void LockAffixCallback(Affix affix)
    {
        UIManager.Instance.CloseCurrentWindow();
        currentItem.RemoveAllAffixLocks();
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