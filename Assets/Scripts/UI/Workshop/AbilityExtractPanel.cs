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

        ExtractableAbilitySlot slot;
        foreach (AbilityBase ability in selectedArchetypeItem.Base.GetArchetypeAbilities())
        {
            Debug.Log(ability.idName + " " + unusedSlots.Count);
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
            slot.textField.text = ability.idName;
            slot.GetComponent<Button>().onClick.RemoveAllListeners();
            slot.GetComponent<Button>().onClick.AddListener(delegate { ExtractableSlotOnClick(ability); });
            slotsInUse.Add(slot);
        }
        confirmButton.interactable = true;
    }

    public void ExtractableSlotOnClick(AbilityBase abilityBase)
    {
        selectedAbilityBase = abilityBase;
    }

    public void ConfirmButtonOnClick()
    {
        if (selectedArchetypeItem == null || selectedAbilityBase == null)
            return;
        AbilityCoreItem abilityStorageItem = AbilityCoreItem.CreateAbilityItemFromArchetype(selectedArchetypeItem, selectedAbilityBase);
        Debug.Log(abilityStorageItem);
        if (abilityStorageItem == null)
            return;
        GameManager.Instance.PlayerStats.AddAbilityToInventory(abilityStorageItem);
        ResetPanel();

        confirmButton.interactable = false;
    }
}