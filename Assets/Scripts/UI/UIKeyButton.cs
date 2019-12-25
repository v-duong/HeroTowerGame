using TMPro;
using UnityEngine;

public class UIKeyButton : MonoBehaviour
{
    public string originalString;
    public string localizedString;
    public bool initialized = false;

    private void Awake()
    {
        if (!initialized)
            Initialize();
    }

    public void Initialize()
    {
        TextMeshProUGUI textComponent = GetComponentInChildren<TextMeshProUGUI>();
        if (textComponent != null)
        {
            originalString = textComponent.text;
            localizedString = LocalizationManager.Instance.GetLocalizationText(originalString);
            textComponent.text = localizedString;
        }
        initialized = true;
    }
}