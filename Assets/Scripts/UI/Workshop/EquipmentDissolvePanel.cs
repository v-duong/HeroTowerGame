using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentDissolvePanel : MonoBehaviour
{
    public Button confirmButton;
    private bool alreadyOpenedOnce = false;
    private bool hasHitConfirm = false;
    public TextMeshProUGUI textBox;
    private List<Item> selectedEquipment;

    private void OnEnable()
    {
        alreadyOpenedOnce = false;
        textBox.text = "None Selected";
    }

    public void EquipSelectOnClick()
    {
        if (!alreadyOpenedOnce || !hasHitConfirm)
        {
            UIManager.Instance.InvScrollContent.ResetMultiSelectList();
            alreadyOpenedOnce = true;
        }

        UIManager.Instance.OpenInventoryWindow(false, false, false, true);
        UIManager.Instance.InvScrollContent.showItemValues = true;
        UIManager.Instance.InvScrollContent.ShowEquipmentFiltered(x => !x.IsEquipped, true, false);
        UIManager.Instance.InvScrollContent.SetConfirmCallback(EquipmentSelectOnClick_Callback);
        hasHitConfirm = false;
    }

    public void EquipmentSelectOnClick_Callback(List<Item> items)
    {
        UIManager.Instance.CloseCurrentWindow();
        hasHitConfirm = true;
        int fragmentCount = 0;
        Dictionary<RarityType, int> rarityCount = new Dictionary<RarityType, int>
        {
            { RarityType.NORMAL, 0 },
            { RarityType.UNCOMMON, 0 },
            { RarityType.RARE, 0 },
            { RarityType.EPIC, 0 },
            { RarityType.UNIQUE, 0 },
        };

        foreach (Item item in items)
        {
            if (item is Equipment equip)
            {
                fragmentCount += equip.GetItemValue();
                rarityCount[equip.Rarity]++;
            }
            else
            {
                items.Remove(item);
            }
        }

        selectedEquipment = items;

        if (selectedEquipment.Count > 0)
            confirmButton.interactable = true;
        else
            confirmButton.interactable = false;

        textBox.text = items.Count + " Equipment Selected: \n";
        foreach (var keyValue in rarityCount)
        {
            if (keyValue.Value != 0)
                textBox.text += LocalizationManager.Instance.GetLocalizationText("rarityType." + keyValue.Key.ToString()) + " x" + keyValue.Value + "\n";
        }
        textBox.text += "\n" + fragmentCount + " Fragments";
    }

    public void DissolveEquipmentConfirm()
    {
        int fragmentCount = 0;

        if (selectedEquipment == null || selectedEquipment.Count == 0)
            return;

        foreach (Equipment item in selectedEquipment)
        {
            fragmentCount += item.GetItemValue();
            GameManager.Instance.PlayerStats.RemoveEquipmentFromInventory(item);
        }

        textBox.text = "None Selected";
        GameManager.Instance.PlayerStats.ModifyItemFragments(fragmentCount);
        selectedEquipment = null;

        confirmButton.interactable = false;
    }
}