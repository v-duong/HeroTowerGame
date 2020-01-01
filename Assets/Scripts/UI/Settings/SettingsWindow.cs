using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsWindow : MonoBehaviour
{
    [SerializeField]
    private Button damageOn;
    [SerializeField]
    private Button damageOff;
    [SerializeField]
    private TextMeshProUGUI ClearDataButtonText;
    private int deleteAllConfirmCount = 0;

    private void OnEnable()
    {
        UpdateButtons();
        ClearDataButtonText.text = "Reset All Data";
        deleteAllConfirmCount = 0;
    }

    public void SetDamageNumbers(bool value)
    {
        GameManager.Instance.PlayerStats.showDamageNumbers = value;
        UpdateButtons();
    }

    public void UpdateButtons()
    {
        if (GameManager.Instance.PlayerStats.showDamageNumbers)
        {
            SetSelectedColor(damageOn);
            SetUnselectedColor(damageOff);
        }
        else
        {
            SetSelectedColor(damageOff);
            SetUnselectedColor(damageOn);
        }
    }

    public void SetSelectedColor(Button button)
    {
        button.image.color = Helpers.SELECTION_COLOR;
    }

    public void SetUnselectedColor(Button button)
    {
        button.image.color = new Color(0.94f, 0.94f, 0.94f);
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