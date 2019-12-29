using TMPro;
using UnityEngine;

public class InfoUI : MonoBehaviour
{
    public TextMeshProUGUI fragText;
    public TextMeshProUGUI xpText;
    public TextMeshProUGUI invText;

    private void Update()
    {
        fragText.text = GameManager.Instance.PlayerStats.ItemFragments.ToString("N0") + " <sprite=10>";
        xpText.text = GameManager.Instance.PlayerStats.ExpStock.ToString("N0") + " XP";
        invText.text = "Items: " + GameManager.Instance.PlayerStats.EquipmentInventory.Count + "/" + PlayerStats.maxEquipInventory;
    }
}