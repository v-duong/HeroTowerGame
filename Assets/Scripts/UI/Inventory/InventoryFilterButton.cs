using TMPro;
using UnityEngine;

public class InventoryFilterButton : MonoBehaviour
{
    public int category;
    public GroupType groupType;

    private void Start()
    {
        if (groupType != GroupType.NO_GROUP)
        {
            GetComponentInChildren<TextMeshProUGUI>(true).text = LocalizationManager.Instance.GetLocalizationText(groupType);
        }
        else
        {
            GetComponentInChildren<TextMeshProUGUI>(true).text = "Show All";
        }
    }

    public void FilterOnClick()
    {
        UIManager.Instance.InvScrollContent.filterWindow.AddFilterButton(this);
    }
}