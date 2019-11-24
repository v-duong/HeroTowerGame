using TMPro;
using UnityEngine;

public class HeroCreationStatLabel : MonoBehaviour
{

    public TextMeshProUGUI firstStatText;
    public TextMeshProUGUI finalStatText;

    public void ClearValues()
    {
        firstStatText.text = "";
        finalStatText.text = "";
    }

    public void SetValues(float current, float finalStat)
    {
        firstStatText.text = current.ToString("N2");

        finalStatText.text = finalStat.ToString("N0");
    }

}