using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpWindow : MonoBehaviour
{
    public GameObject textScrollView;
    public GameObject verticalScrollView;
    public GameObject gridScrollView;
    public GameObject textInputParent;
    public GameObject popUpWindowMain;
    public TextMeshProUGUI textField;
    public Button confirmButton;
    public TextMeshProUGUI confirmButtonText;
    public GameObject verticalGroupParent;
    public GameObject gridGroupParent;
    public TMP_InputField textInput;

    private bool isVertical = false;
    private bool isGrid = false;

    public List<GameObject> temporaryElements = new List<GameObject>();

    private void OnDisable()
    {
        temporaryElements.ForEach(x=>Destroy(x.gameObject));
        temporaryElements.Clear();
    }

    public void SetButtonValues(string buttonString, UnityEngine.Events.UnityAction action)
    {
        confirmButtonText.text = buttonString;
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(action);
    }

    public void OpenTextWindow(int width = 400, int height = 700)
    {
        ((RectTransform)popUpWindowMain.transform).sizeDelta = new Vector2(width, height) ;
        UIManager.Instance.OpenWindow(this.gameObject, false);
        textScrollView.SetActive(true);
        verticalScrollView.SetActive(false);
        gridScrollView.SetActive(false);
        textInput.gameObject.SetActive(false);
        isVertical = false;
        isGrid = false;
        ((RectTransform)textField.transform).anchoredPosition = Vector3.zero;
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

    public void AddObjectToViewport(GameObject gameObject)
    {
        temporaryElements.Add(gameObject);
        gameObject.transform.SetParent(verticalGroupParent.transform, false);
    }
}