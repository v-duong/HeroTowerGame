using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIKeyButton : MonoBehaviour
{
    private void OnEnable()
    {
        Text textObject = GetComponentInChildren<Text>();
        if (textObject != null)
            textObject.text = LocalizationManager.Instance.GetLocalizationText(textObject.text);
        TextMeshProUGUI textObject2 = GetComponentInChildren<TextMeshProUGUI>();
        if (textObject2 != null)
            textObject2.text = LocalizationManager.Instance.GetLocalizationText(textObject2.text);
    }
}