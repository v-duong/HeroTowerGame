using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityCoreDissolvePanel : MonoBehaviour
{
    public Button confirmButton;
    private bool alreadyOpenedOnce = false;
    private bool hasHitConfirm = false;
    public TextMeshProUGUI textBox;
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI slotsText;
    private List<Item> selectedAbilities = new List<Item>();

    private void OnEnable()
    {
        selectedAbilities.Clear();
        confirmButton.interactable = false;
        alreadyOpenedOnce = false;
        buttonText.text = "Select Ability Cores";
        textBox.text = "Current Fragments: " + GameManager.Instance.PlayerStats.ArchetypeFragments + "<sprite=9>";
        slotsText.text = "Ability Core Slots: " + GameManager.Instance.PlayerStats.AbilityInventory.Count + "/" + PlayerStats.maxAbilityInventory;
    }

    public void AbilitySelectOnClick()
    {
        if (!alreadyOpenedOnce || !hasHitConfirm)
        {
            UIManager.Instance.InvScrollContent.SetMultiSelectList(selectedAbilities);
            alreadyOpenedOnce = true;
        }

        UIManager.Instance.OpenInventoryWindow(false, false, false, true);
        UIManager.Instance.InvScrollContent.showItemValues = false;
        UIManager.Instance.InvScrollContent.ShowAbilityFiltered(x=>x.EquippedHero==null,true, false);
        UIManager.Instance.InvScrollContent.SetConfirmCallback(AbilitySelectOnClick_Callback);
        hasHitConfirm = false;
    }

    public void AbilitySelectOnClick_Callback(List<Item> items)
    {
        UIManager.Instance.CloseCurrentWindow();
        hasHitConfirm = true;
        int fragmentCount = 0;
        foreach (Item item in items)
        {
            if (item is AbilityCoreItem archetypeItem)
            {
                fragmentCount += 1;
            }
            else
            {
                items.Remove(item);
            }
        }

        selectedAbilities.Clear();
        selectedAbilities.AddRange(items);

        if (selectedAbilities.Count > 0)
            confirmButton.interactable = true;
        else
            confirmButton.interactable = false;

        buttonText.text = items.Count + " Ability Cores Selected\n+" + fragmentCount + " <sprite=9>";
        textBox.text = "Current Fragments: " + GameManager.Instance.PlayerStats.ArchetypeFragments + "<sprite=9>\n";
        textBox.text += "Fragments After: " + (GameManager.Instance.PlayerStats.ArchetypeFragments + fragmentCount) + "<sprite=9>";
    }

    public void ConfirmOnClick()
    {
        int fragmentCount = 0;

        if (selectedAbilities == null || selectedAbilities.Count == 0)
            return;

        foreach (AbilityCoreItem item in selectedAbilities)
        {
            fragmentCount += 1;
            GameManager.Instance.PlayerStats.RemoveAbilityFromInventory(item);
        }

        

        GameManager.Instance.PlayerStats.ModifyArchetypeFragments(fragmentCount);
        selectedAbilities.Clear();
        UIManager.Instance.InvScrollContent.ResetMultiSelectList();

        SaveManager.Save();
        buttonText.text = "Select Ability Cores";
        textBox.text = "Current Fragments: " + GameManager.Instance.PlayerStats.ArchetypeFragments + "<sprite=9>";
        slotsText.text = "Ability Core Slots: " + GameManager.Instance.PlayerStats.AbilityInventory.Count + "/" + PlayerStats.maxAbilityInventory;
        confirmButton.interactable = false;
    }
}
