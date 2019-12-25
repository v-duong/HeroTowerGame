using TMPro;
using UnityEngine;

public class InfoUI : MonoBehaviour
{
    public TextMeshProUGUI fragText;
    public TextMeshProUGUI xpText;

    private void Update()
    {
        fragText.text = GameManager.Instance.PlayerStats.ItemFragments.ToString("N0") + " <sprite=10>";
        xpText.text = GameManager.Instance.PlayerStats.ExpStock.ToString("N0") + " XP";
    }
}