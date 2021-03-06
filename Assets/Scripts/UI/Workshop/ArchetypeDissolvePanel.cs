﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchetypeDissolvePanel : MonoBehaviour
{
    public Button confirmButton;
    private bool alreadyOpenedOnce = false;
    private bool hasHitConfirm = false;
    public TextMeshProUGUI textBox;
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI slotsText;
    private List<Item> selectedArchetypes = new List<Item>();

    private void OnEnable()
    {
        selectedArchetypes.Clear();
        confirmButton.interactable = false;
        alreadyOpenedOnce = false;
        buttonText.text = "Select Archetypes";
        textBox.text = "Current Fragments: " + GameManager.Instance.PlayerStats.ArchetypeFragments + "<sprite=9>";
        slotsText.text = "Archetype Slots: " + GameManager.Instance.PlayerStats.ArchetypeInventory.Count + "/" + PlayerStats.maxArchetypeInventory;
    }

    public void ArchetypeSelectOnClick()
    {
        if (!alreadyOpenedOnce || !hasHitConfirm)
        {
            UIManager.Instance.InvScrollContent.SetMultiSelectList(selectedArchetypes);
            alreadyOpenedOnce = true;
        }

        UIManager.Instance.OpenInventoryWindow(false, false, false, true);
        UIManager.Instance.InvScrollContent.showItemValues = true;
        UIManager.Instance.InvScrollContent.ShowAllArchetypes(true, false);
        UIManager.Instance.InvScrollContent.SetConfirmCallback(ArchetypeSelectOnClick_Callback);
        hasHitConfirm = false;
    }

    public void ArchetypeSelectOnClick_Callback(List<Item> items)
    {
        UIManager.Instance.CloseCurrentWindow();
        hasHitConfirm = true;
        int fragmentCount = 0;
        foreach (Item item in items)
        {
            if (item is ArchetypeItem archetypeItem)
            {
                fragmentCount += archetypeItem.GetItemValue();
            }
            else
            {
                items.Remove(item);
            }
        }

        selectedArchetypes.Clear();
        selectedArchetypes.AddRange(items);

        if (selectedArchetypes.Count > 0)
            confirmButton.interactable = true;
        else
            confirmButton.interactable = false;

        buttonText.text = items.Count + " Archetypes Selected\n+" + fragmentCount + " <sprite=9>";
        textBox.text = "Current Fragments: " + GameManager.Instance.PlayerStats.ArchetypeFragments + "<sprite=9>\n";
        textBox.text += "Fragments After: " + (GameManager.Instance.PlayerStats.ArchetypeFragments + fragmentCount) + "<sprite=9>";
    }

    public void DissolveArchetypeConfirm()
    {
        int fragmentCount = 0;

        if (selectedArchetypes == null || selectedArchetypes.Count == 0)
            return;

        foreach (ArchetypeItem item in selectedArchetypes)
        {
            fragmentCount += item.GetItemValue();
            GameManager.Instance.PlayerStats.RemoveArchetypeFromInventory(item);
        }

        
        
        GameManager.Instance.PlayerStats.ModifyArchetypeFragments(fragmentCount);
        selectedArchetypes.Clear();
        UIManager.Instance.InvScrollContent.ResetMultiSelectList();

        SaveManager.Save();
        buttonText.text = "Select Archetypes";
        textBox.text = "Current Fragments: " + GameManager.Instance.PlayerStats.ArchetypeFragments + "<sprite=9>";
        slotsText.text = "Archetype Slots: " + GameManager.Instance.PlayerStats.ArchetypeInventory.Count + "/" + PlayerStats.maxArchetypeInventory;
        confirmButton.interactable = false;
    }
}
