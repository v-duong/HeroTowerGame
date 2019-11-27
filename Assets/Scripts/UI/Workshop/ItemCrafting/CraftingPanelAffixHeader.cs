using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingPanelAffixHeader : MonoBehaviour
{
    public Image mainBackground;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI countText;

    public void SetHeaderValues(int count, int maxCount, string header = null, bool showCount = false, Color? color = null)
    {
        if (!showCount)
        countText.text = "( " + count + " / " + maxCount + " )";
        else
            countText.text = "";

        if (header != null)
            headerText.text = header;
    }
}