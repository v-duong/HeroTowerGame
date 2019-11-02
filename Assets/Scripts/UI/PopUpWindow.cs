using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpWindow : MonoBehaviour
{
    public GameObject textScrollView;
    public GameObject verticalScrollView;
    public GameObject gridScrollView;
    public TextMeshProUGUI textField;
    public Button confirmButton;
    public TextMeshProUGUI confirmButtonText;
    public GameObject verticalGroupParent;
    public GameObject gridGroupParent;

    private bool isVertical = false;
    private bool isGrid = false;

    public List<GameObject> temporaryElements = new List<GameObject>();

    private void OnDisable()
    {
        temporaryElements.ForEach(x=>Destroy(x.gameObject));
        temporaryElements.Clear();
    }

    public void OpenTextWindow()
    {
        UIManager.Instance.OpenWindow(this.gameObject, false);
        textScrollView.SetActive(true);
        verticalScrollView.SetActive(false);
        gridScrollView.SetActive(false);
        isVertical = false;
        isGrid = false;
    }

    public void OpenVerticalWindow()
    {
        UIManager.Instance.OpenWindow(this.gameObject, false);
        textScrollView.SetActive(false);
        verticalScrollView.SetActive(true);
        gridScrollView.SetActive(false);
        isVertical = true;
        isGrid = false;
    }

    public void OpenGridWindow()
    {
        UIManager.Instance.OpenWindow(this.gameObject, false);
        textScrollView.SetActive(false);
        verticalScrollView.SetActive(false);
        gridScrollView.SetActive(true);
        isVertical = false;
        isGrid = true;
    }

    public void AddObjectToViewport(GameObject gameObject)
    {
        temporaryElements.Add(gameObject);
        gameObject.transform.SetParent(verticalGroupParent.transform, false);
    }
}