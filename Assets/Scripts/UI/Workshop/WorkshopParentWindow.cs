using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkshopParentWindow : MonoBehaviour
{
    public static WorkshopParentWindow Instance { get; private set; }
    [SerializeField]
    private GameObject itemCraftingPanel;
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

    private void Awake()
    {
        Instance = this;
        currentWorkshopPanel = itemCraftingPanel;
    }


    private void SetPanelActive(GameObject panel)
    {
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
