using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HelpButton : MonoBehaviour
{
    public List<string> helpStrings;

    public void OpenHelpWindow()
    {
        PopUpWindow popUpWindow = UIManager.Instance.PopUpWindow;
        popUpWindow.OpenHelpWindow(helpStrings);
    }
}