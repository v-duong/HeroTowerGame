using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroDetailWindow : MonoBehaviour
{
    [SerializeField]
    private HeroDetailMainPage detailMainPage;

    [SerializeField]
    private HeroDetailEquipmentPage equipmentPage;

    [SerializeField]
    private ArchetypeUITreeWindow treeWindow;

    [SerializeField]
    private List<Button> categoryButtons;

    public static HeroData hero;

    public IUpdatablePanel currentPanel;

    public void UpdateCurrentPanel()
    {
        currentPanel.UpdateWindow();
    }

    public void SetCategorySelected(int index)
    {
        for (int j = 0; j < categoryButtons.Count; j++)
        {
            if (j == index)
            {
                categoryButtons[j].image.color = Helpers.SELECTION_COLOR;
            }
            else
            {
                categoryButtons[j].image.color = Color.white;
            }
        }

        switch (index)
        {
            case 0:
                currentPanel = detailMainPage;
                detailMainPage.gameObject.SetActive(true);
                equipmentPage.gameObject.SetActive(false);
                break;

            case 1:
                currentPanel = equipmentPage;
                detailMainPage.gameObject.SetActive(false);
                equipmentPage.gameObject.SetActive(true);
                break;

            default:
                break;
        }

        UpdateCurrentPanel();
    }

    public void SetArchetypeCategoryNames(string primary, string secondary)
    {
        categoryButtons[2].GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizationText_ArchetypeName(primary) + " Tree";
        if (!string.IsNullOrWhiteSpace(secondary))
        {
            categoryButtons[3].gameObject.SetActive(true);
            categoryButtons[3].GetComponentInChildren<TextMeshProUGUI>().text = LocalizationManager.Instance.GetLocalizationText_ArchetypeName(secondary) + " Tree";
        }
        else
        {
            categoryButtons[3].gameObject.SetActive(false);
        }
    }

    public void ItemEquip(Item item, EquipSlotType equipSlot)
    {
        if (item == null)
            hero.UnequipFromSlot(equipSlot);
        else
            hero.EquipToSlot(item as Equipment, equipSlot);
        equipmentPage.UpdateWindow();
    }

    public void ClickPrimaryTree()
    {
        treeWindow.OpenArchetypeTree(hero, treeWindow.hero != hero, 0);
    }

    public void ClickSecondaryTree()
    {
        treeWindow.OpenArchetypeTree(hero, treeWindow.hero != hero, 1);
    }
}