using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchetypeDissolvePanel : MonoBehaviour
{
    public Button confirmButton;
    private bool alreadyOpenedOnce = false;
    private bool hasHitConfirm = false;
    public TextMeshProUGUI textBox;
    private List<Item> selectedArchetypes;

    private void OnEnable()
    {
        alreadyOpenedOnce = false;
        textBox.text = "None Selected";
    }

    public void ArchetypeSelectOnClick()
    {
        if (!alreadyOpenedOnce || !hasHitConfirm)
        {
            UIManager.Instance.InvScrollContent.ResetMultiSelectList();
            alreadyOpenedOnce = true;
        }

        UIManager.Instance.OpenInventoryWindow(false, false, false, true);
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
                fragmentCount += ArchetypeItem.GetFragmentWorth(archetypeItem.Base.stars);
            }
            else
            {
                items.Remove(item);
            }
        }

        selectedArchetypes = items;

        if (selectedArchetypes.Count > 0)
            confirmButton.interactable = true;
        else
            confirmButton.interactable = false;

        textBox.text = items.Count + " Archetypes Selected\n" + fragmentCount + " Fragments";
    }

    public void DissolveArchetypeConfirm()
    {
        int fragmentCount = 0;

        if (selectedArchetypes == null || selectedArchetypes.Count == 0)
            return;

        foreach (ArchetypeItem item in selectedArchetypes)
        {
            fragmentCount += ArchetypeItem.GetFragmentWorth(item.Base.stars);
            GameManager.Instance.PlayerStats.RemoveArchetypeFromInventory(item);
        }

        textBox.text = "None Selected";
        GameManager.Instance.PlayerStats.ModifyArchetypeFragments(fragmentCount);
        selectedArchetypes = null;

        confirmButton.interactable = false;
    }
}