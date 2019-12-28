using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryFilterWindow : MonoBehaviour
{
    public GameObject primaryCategories;
    public GameObject armorAttributeCategories;
    public GameObject armorSlotCategories;
    public GameObject weaponCategoriesHand;
    public GameObject weaponCategoriesType;
    public GameObject accessoryCategories;
    public Button showAllButton;

    public List<InventoryFilterButton> selectedButtons = new List<InventoryFilterButton>();
    private Dictionary<CategoryType, List<InventoryFilterButton>> buttonList = new Dictionary<CategoryType, List<InventoryFilterButton>>();

    private void Start()
    {
        foreach (CategoryType category in Enum.GetValues(typeof(CategoryType)))
        {
            buttonList.Add(category, new List<InventoryFilterButton>());
        }
        foreach (InventoryFilterButton button in primaryCategories.GetComponentsInChildren<InventoryFilterButton>())
        {
            button.category = (int)CategoryType.PRIMARY;
            buttonList[CategoryType.PRIMARY].Add(button);
        }
        foreach (InventoryFilterButton button in armorAttributeCategories.GetComponentsInChildren<InventoryFilterButton>())
        {
            button.category = (int)CategoryType.ARMOR_ATTR;
            buttonList[CategoryType.ARMOR_ATTR].Add(button);
        }
        foreach (InventoryFilterButton button in armorSlotCategories.GetComponentsInChildren<InventoryFilterButton>())
        {
            button.category = (int)CategoryType.ARMOR_SLOT;
            buttonList[CategoryType.ARMOR_SLOT].Add(button);
        }
        foreach (InventoryFilterButton button in weaponCategoriesHand.GetComponentsInChildren<InventoryFilterButton>())
        {
            button.category = (int)CategoryType.WEAPON_HAND;
            buttonList[CategoryType.WEAPON_HAND].Add(button);
        }
        foreach (InventoryFilterButton button in weaponCategoriesType.GetComponentsInChildren<InventoryFilterButton>())
        {
            button.category = (int)CategoryType.WEAPON_TYPE;
            buttonList[CategoryType.WEAPON_TYPE].Add(button);
        }
        foreach (InventoryFilterButton button in accessoryCategories.GetComponentsInChildren<InventoryFilterButton>())
        {
            button.category = (int)CategoryType.ACCESSORY_SLOT;
            buttonList[CategoryType.ACCESSORY_SLOT].Add(button);
        }
    }

    private void OnEnable()
    {
        CheckSubcategories();
    }

    public void AddFilterButton(InventoryFilterButton button)
    {
        if (button.category == (int)CategoryType.PRIMARY)
        {
            ClearSelectedButtons();
        }

        showAllButton.GetComponent<Button>().image.color = Color.white;

        if (button.groupType == GroupType.NO_GROUP)
        {
            ClearSelectedButtons();
            button.GetComponent<Button>().image.color = Helpers.SELECTION_COLOR;
        }
        else if (!selectedButtons.Contains(button))
        {
            foreach (InventoryFilterButton otherButton in buttonList[(CategoryType)button.category])
            {
                otherButton.GetComponent<Button>().image.color = Color.white;
                selectedButtons.Remove(otherButton);
            }

            selectedButtons.Add(button);
            button.GetComponent<Button>().image.color = Helpers.SELECTION_COLOR;
        }
        else
        {
            selectedButtons.Remove(button);
            button.GetComponent<Button>().image.color = Color.white;
        }

        CheckSubcategories();
    }

    public void ClearSelectedButtons()
    {
        foreach (InventoryFilterButton button in selectedButtons)
        {
            button.GetComponent<Button>().image.color = Color.white;
        }
        selectedButtons.Clear();
    }

    public void CheckSubcategories()
    {
        bool armorSelected = selectedButtons.Find(x => x.groupType == GroupType.ALL_ARMOR);
        bool weaponSelected = selectedButtons.Find(x => x.groupType == GroupType.WEAPON);
        bool accSelected = selectedButtons.Find(x => x.groupType == GroupType.ALL_ACCESSORY);

        armorAttributeCategories.SetActive(armorSelected);
        armorSlotCategories.SetActive(armorSelected);

        weaponCategoriesHand.SetActive(weaponSelected);
        weaponCategoriesType.SetActive(weaponSelected);

        accessoryCategories.SetActive(accSelected);
    }

    public void FilterInventoryOnClick()
    {
        HashSet<GroupType> groupTypes = new HashSet<GroupType>();

        foreach (InventoryFilterButton button in selectedButtons)
        {
            groupTypes.Add(button.groupType);
        }

        UIManager.Instance.InvScrollContent.FilterShownSlotsByType(groupTypes);
    }

    private enum CategoryType
    {
        PRIMARY,
        ARMOR_ATTR,
        ARMOR_SLOT,
        WEAPON_HAND,
        WEAPON_RANGE,
        WEAPON_TYPE,
        ACCESSORY_SLOT
    }
}