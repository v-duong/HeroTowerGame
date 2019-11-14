using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityExtractPanel : MonoBehaviour
{
    public ExtractableAbilitySlot slotPrefab;
    public Button archetypeSlot;
    public Button confirmButton;
    public GameObject scrollViewContent;
    private ArchetypeItem selectedArchetypeItem;
    private AbilityBase selectedAbilityBase;
    public List<ExtractableAbilitySlot> slotsInUse = new List<ExtractableAbilitySlot>();
    public Queue<ExtractableAbilitySlot> unusedSlots = new Queue<ExtractableAbilitySlot>();

    private void OnEnable()
    {
        ResetPanel();
    }

    private void ResetPanel()
    {
        archetypeSlot.GetComponentInChildren<TextMeshProUGUI>().text = "SELECT ARCHETYPE";
        selectedArchetypeItem = null;
        selectedAbilityBase = null;
        confirmButton.interactable = false;
        ClearSlotsInUse();
    }

    private void ClearSlotsInUse()
    {
        foreach (ExtractableAbilitySlot abilitySlot in slotsInUse)
        {
            abilitySlot.gameObject.SetActive(false);
            abilitySlot.gameObject.transform.SetParent(null, true);
            unusedSlots.Enqueue(abilitySlot);
        }
        slotsInUse.Clear();
    }

    public void ArchetypeSlotOnClick()
    {
        UIManager.Instance.OpenInventoryWindow(false, false, false);
        UIManager.Instance.InvScrollContent.ShowAllArchetypes(true, true);
        UIManager.Instance.InvScrollContent.SetCallback(ArchetypeSlotOnClick_Callback);
    }

    public void ArchetypeSlotOnClick_Callback(Item item)
    {
        UIManager.Instance.CloseCurrentWindow();
        if (item != null && item.GetItemType() != ItemType.ARCHETYPE)
        {
            return;
        }

        ClearSlotsInUse();

        if (item == null)
        {
            selectedArchetypeItem = null;
            return;
        }
        selectedArchetypeItem = item as ArchetypeItem;
        archetypeSlot.GetComponentInChildren<TextMeshProUGUI>().text = selectedArchetypeItem.Name;

        foreach (AbilityBase ability in selectedArchetypeItem.Base.GetArchetypeAbilities(true))
        {
            AddExtractSlot(ability);
        }
        confirmButton.interactable = true;
    }

    private ExtractableAbilitySlot AddExtractSlot(AbilityBase ability)
    {
        ExtractableAbilitySlot slot;
        if (unusedSlots.Count > 0)
        {
            slot = unusedSlots.Dequeue();
            slot.gameObject.SetActive(true);
        }
        else
        {
            slot = Instantiate(slotPrefab, scrollViewContent.transform);
        }
        slot.transform.SetParent(scrollViewContent.transform, true);
        slot.GetComponent<Button>().onClick.RemoveAllListeners();
            slot.textField.text = ability.idName;
        slot.GetComponent<Button>().onClick.AddListener(delegate { ExtractableSlotOnClick(ability); });
        slotsInUse.Add(slot);
        return slot;
    }

    public void ExtractableSlotOnClick(AbilityBase abilityBase)
    {
        selectedAbilityBase = abilityBase;
    }

    public void ConfirmButtonOnClick()
    {
        if (selectedArchetypeItem == null)
            return;
        if (!GameManager.Instance.PlayerStats.ArchetypeInventory.Contains(selectedArchetypeItem))
            return;
        if (selectedAbilityBase == null)
        {
            GameManager.Instance.PlayerStats.RemoveArchetypeFromInventory(selectedArchetypeItem);
            GameManager.Instance.PlayerStats.ModifyArchetypeFragments(selectedArchetypeItem.GetItemValue());
        }
        else
        {
            AbilityCoreItem abilityStorageItem = AbilityCoreItem.CreateAbilityItemFromArchetype(selectedArchetypeItem, selectedAbilityBase);
            if (abilityStorageItem == null)
                return;
            GameManager.Instance.PlayerStats.AddAbilityToInventory(abilityStorageItem);
        }

        ResetPanel();

        confirmButton.interactable = false;
    }
}