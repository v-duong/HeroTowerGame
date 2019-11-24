using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XpStockStatLabel : MonoBehaviour
{
    public static readonly Color NEUTRAL_COLOR = new Color(0.73f, 0.73f, 0.73f);
    public static readonly Color RED_COLOR = new Color(0.78f, 0.5f, 0.5f);
    public static readonly Color GREEN_COLOR = new Color(0.5f, 0.78f, 0.5f);

    public Image backgroundImage;
    public TextMeshProUGUI currentStatText;
    public TextMeshProUGUI gainStatText;
    public TextMeshProUGUI finalStatText;

    public void ClearValues()
    {
        currentStatText.text = "";
        gainStatText.text = "";
        finalStatText.text = "";
    }

    public void SetValues(float current, float gain, float finalStat)
    {
        currentStatText.text = current.ToString("N0");

        if (gain > 0)
        {
            gainStatText.text = "+" + gain.ToString("N0");
        }
        else
        {
            gainStatText.text = gain.ToString("N0");
        }

        finalStatText.text = finalStat.ToString("N0");
    }

    public void SetNeutralColor()
    {
        backgroundImage.color = NEUTRAL_COLOR;
    }

    public void SetGreenColor()
    {
        backgroundImage.color = GREEN_COLOR;
    }

    public void SetRedColor()
    {
        backgroundImage.color = RED_COLOR;
    }
}
