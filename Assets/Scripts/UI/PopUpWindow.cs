﻿using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpWindow : MonoBehaviour
{
    public GameObject textScrollView;
    public GameObject verticalScrollView;
    public GameObject gridScrollView;
    public GameObject popUpWindowMain;
    public TextMeshProUGUI textField;
    public Button confirmButton;
    public Button cancelButton;
    public TextMeshProUGUI confirmButtonText;
    public TextMeshProUGUI cancelButtonText;
    public GameObject verticalGroupParent;
    public GameObject gridGroupParent;
    public TMP_InputField textInput;

    private bool isVertical = false;
    private bool isGrid = false;
    private bool isHelpWindowOpen = false;

    public List<GameObject> temporaryElements = new List<GameObject>();
    private Stack<List<string>> helpWindowStack = new Stack<List<string>>();
    private List<string> currentHelpStrings;

    private void OnDisable()
    {
        temporaryElements.ForEach(x => Destroy(x.gameObject));
        temporaryElements.Clear();
    }

    public void SetButtonValues(string confirmButtonString, UnityEngine.Events.UnityAction confirmAction, string cancelButtonString, UnityEngine.Events.UnityAction cancelAction)
    {
        if (string.IsNullOrEmpty(confirmButtonString))
        {
            confirmButton.gameObject.SetActive(false);
        }
        else
        {
            confirmButton.gameObject.SetActive(true);
            confirmButtonText.text = confirmButtonString;
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(confirmAction);
        }

        if (string.IsNullOrEmpty(cancelButtonString))
            cancelButton.gameObject.SetActive(false);
        else
        {
            cancelButton.gameObject.SetActive(true);
            cancelButtonText.text = cancelButtonString;
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(cancelAction);
        }
    }

    public void OpenTextWindow(string text, int width = 400, int height = 700)
    {
        ((RectTransform)popUpWindowMain.transform).sizeDelta = new Vector2(width, height);
        UIManager.Instance.OpenWindow(this.gameObject, false);
        textScrollView.SetActive(true);
        verticalScrollView.SetActive(false);
        gridScrollView.SetActive(false);
        textInput.gameObject.SetActive(false);
        isVertical = false;
        isGrid = false;
        ((RectTransform)textField.transform).anchoredPosition = Vector3.zero;
        textField.text = text;
    }

    public void OpenVerticalWindow(int width = 400, int height = 700)
    {
        ((RectTransform)popUpWindowMain.transform).sizeDelta = new Vector2(width, height);
        UIManager.Instance.OpenWindow(this.gameObject, false);
        textScrollView.SetActive(false);
        verticalScrollView.SetActive(true);
        gridScrollView.SetActive(false);
        textInput.gameObject.SetActive(false);
        isVertical = true;
        isGrid = false;
    }

    public void OpenGridWindow(int width = 400, int height = 700)
    {
        ((RectTransform)popUpWindowMain.transform).sizeDelta = new Vector2(width, height);
        UIManager.Instance.OpenWindow(this.gameObject, false);
        textScrollView.SetActive(false);
        verticalScrollView.SetActive(false);
        gridScrollView.SetActive(true);
        textInput.gameObject.SetActive(false);
        isVertical = false;
        isGrid = true;
    }

    public void OpenTextInput(string defaultString, int width = 400, int height = 150)
    {
        ((RectTransform)popUpWindowMain.transform).sizeDelta = new Vector2(width, height);
        UIManager.Instance.OpenWindow(this.gameObject, false);
        textScrollView.SetActive(false);
        verticalScrollView.SetActive(false);
        gridScrollView.SetActive(false);
        textInput.gameObject.SetActive(true);
        isVertical = false;
        isGrid = false;

        textInput.text = defaultString;
    }

    public void OpenHelpWindow(List<string> helpStrings)
    {
        if (!isHelpWindowOpen || !this.gameObject.activeSelf)
        {
            helpWindowStack.Clear();
            OpenTextWindow("");
            SetButtonValues("Back", HelpWindowBack, "Close All", delegate { UIManager.Instance.CloseCurrentWindow(); isHelpWindowOpen = false; });
            textField.fontSize = 20;
            textField.paragraphSpacing = 50;
            textField.lineSpacing = 0;
            textField.alignment = TextAlignmentOptions.Left;
            isHelpWindowOpen = true;
            currentHelpStrings = null;
        }        

        if (currentHelpStrings != null)
            helpWindowStack.Push(currentHelpStrings);
        SetHelpTextField(helpStrings);
    }

    private void SetHelpTextField(List<string> helpStrings)
    {
        ((RectTransform)textField.transform).anchoredPosition = Vector3.zero;
        textField.text = "";

        foreach (string s in helpStrings)
        {
            textField.text += LocalizationManager.Instance.GetLocalizationText_HelpString(s);
            textField.text += '\n';
        }
        currentHelpStrings = helpStrings;
    }

    private void HelpWindowBack()
    {
        if (helpWindowStack.Count == 0)
        {
            isHelpWindowOpen = false;
            UIManager.Instance.CloseCurrentWindow();
        }
        else
        {
            SetHelpTextField(helpWindowStack.Pop());
        }
    }

    public void AddObjectToViewport(GameObject gameObject)
    {
        temporaryElements.Add(gameObject);
        gameObject.transform.SetParent(verticalGroupParent.transform, false);
    }
}