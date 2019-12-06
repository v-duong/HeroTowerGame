using UnityEngine;
using TMPro;
public class HeroStatBox : MonoBehaviour
{
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI statText;

    public void SetStatBoxValues(string text)
    {
        statText.text = text;
    }
}