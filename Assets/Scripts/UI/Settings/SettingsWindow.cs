using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsWindow : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI ClearDataButtonText;
    private int deleteAllConfirmCount = 0;

    private void OnEnable()
    {
        ClearDataButtonText.text = "Reset All Data";
        deleteAllConfirmCount = 0;
    }

    public void OnClickClearData()
    {
        switch(deleteAllConfirmCount)
        {
            case 0:
                ClearDataButtonText.text = "Do you want to reset your save data?";
                deleteAllConfirmCount = 1;
                break;
            case 1:
                ClearDataButtonText.text = "This cannot be undone. Confirm once more.";
                deleteAllConfirmCount = 2;
                break;
            case 2:
                GameManager.Instance.InitializePlayerStats();
                break;
        }
    }
}