using UnityEngine;

public class WorkshopParentWindow : MonoBehaviour
{
    [SerializeField]
    private ItemCraftingPanel itemCraftingPanel;

    [SerializeField]
    private GameObject heroCreationPanel;

    [SerializeField]
    private GameObject XpStockPanel;

    [SerializeField]
    private GameObject AbilityExtractPanel;

    [SerializeField]
    private GameObject categoryPanel;

    [HideInInspector]
    public GameObject currentWorkshopPanel = null;

    public void OpenItemCraftingPanel(Item item)
    {
        OpenPanel(itemCraftingPanel.gameObject);
        itemCraftingPanel.SetItem(item);
    }

    private void SetPanelActive(GameObject panel)
    {
        if (currentWorkshopPanel != null)
            currentWorkshopPanel.SetActive(false);
        panel.SetActive(true);
        currentWorkshopPanel = panel;
    }

    public void SetCategoryPanelActive()
    {
        UIManager.Instance.OpenWindow(categoryPanel, false);
    }

    public void OpenPanel(GameObject panel)
    {
        UIManager.Instance.OpenWindow(this.gameObject, true);
        SetPanelActive(panel);
    }
}