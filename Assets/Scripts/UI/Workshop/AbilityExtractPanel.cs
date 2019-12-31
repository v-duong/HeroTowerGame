using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityExtractPanel : MonoBehaviour
{
    public HeroUIAbilitySlot abilitySlot;
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
        abilitySlot.ClearSlot();
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
        abilitySlot.ClearSlot();
        archetypeSlot.GetComponentInChildren<TextMeshProUGUI>().text = "Select Archetype";
        confirmButton.interactable = false;
        ClearSlotsInUse();

        if (item == null || (item != null && item.GetItemType() != ItemType.ARCHETYPE))
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

        AddExtractSlot(selectedArchetypeItem.Base.GetSoulAbility());
    }

    private ExtractableAbilitySlot AddExtractSlot(AbilityBase ability)
    {
        if (ability == null)
            return null;

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
        slot.textField.text = ability.LocalizedName;
        slot.GetComponent<Button>().onClick.AddListener(delegate { ExtractableSlotOnClick(ability); });
        slotsInUse.Add(slot);
        return slot;
    }

    public void ExtractableSlotOnClick(AbilityBase abilityBase)
    {
        confirmButton.interactable = true;
        selectedAbilityBase = abilityBase;
        abilitySlot.SetSlot(abilityBase, null, 0);
    }

    public void ConfirmButtonOnClick()
    {
        if (selectedArchetypeItem == null || selectedAbilityBase == null)
            return;
        if (!GameManager.Instance.PlayerStats.ArchetypeInventory.Contains(selectedArchetypeItem))
            return;

        AbilityCoreItem abilityStorageItem = AbilityCoreItem.CreateAbilityItemFromArchetype(selectedArchetypeItem, selectedAbilityBase);
        if (abilityStorageItem == null)
            return;
        GameManager.Instance.PlayerStats.AddAbilityToInventory(abilityStorageItem);

        SaveManager.CurrentSave.SavePlayerData();
        SaveManager.Save();

        ResetPanel();

        confirmButton.interactable = false;
    }
}