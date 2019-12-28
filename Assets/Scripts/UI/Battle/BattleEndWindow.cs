using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleEndWindow : MonoBehaviour
{
    public Button endBattleButton;
    public Button nextLoopButton;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI expText;
    public TextMeshProUGUI fragText;
    public TextMeshProUGUI bodyText;
    public bool isVictory;
    public float totalExp;
    public float addedExp;
    public float totalFrag;
    public float addedFrag;
    private float expChangePerSecond;
    private float fragChangePerSecond;
    private float timePassed;
    private bool expStringInitialized = false;
    private bool fragStringInitialized = false;

    private void OnEnable()
    {
        timePassed = 0;
        expStringInitialized = false;
        fragStringInitialized = false;
        ScrollRect scrollRect = GetComponentInChildren<ScrollRect>();
        scrollRect.content.anchoredPosition = Vector2.zero;
    }

    public void ShowVictoryWindow()
    {
        this.gameObject.SetActive(true);
        headerText.text = "Winner";
        bodyText.text = "";

        nextLoopButton.gameObject.SetActive(GameManager.Instance.PlayerStats.EquipmentInventory.Count < PlayerStats.maxEquipInventory);
    }

    public void ShowLoseWindow()
    {
        this.gameObject.SetActive(true);
        headerText.text = "Lose";
        bodyText.text = "";
        nextLoopButton.gameObject.SetActive(false);
    }

    public void Update()
    {
        timePassed += Time.unscaledDeltaTime;
        if (timePassed > 1f)
        {
            if (addedExp != 0)
            {
                addedExp = Mathf.MoveTowards(addedExp, 0, expChangePerSecond);
                UpdateExpString();
            }
            if (addedFrag != 0)
            {
                addedFrag = Mathf.MoveTowards(addedFrag, 0, fragChangePerSecond);
                UpdateFragString();
            }
        }
        else
        {
            if (!expStringInitialized)
            UpdateExpString();
            if (!fragStringInitialized)
            UpdateFragString();
        }
    }

    public void UpdateExpString()
    {
        string addedExpString;
        string addedExpStockString;

        if (addedExp > 0)
        {
            addedExpString = "<color=#00b33c> +" + addedExp.ToString("N0") + "</color>";
            addedExpStockString = "<color=#00b33c> +" + (addedExp * PlayerStats.EXP_STOCK_RATE).ToString("N0") + "</color>";
        }
        else if (addedExp < 0)
        {
            addedExpString = "<color=#990000> " + addedExp.ToString("N0") + "</color>";
            addedExpStockString = "<color=#990000> " + (addedExp * PlayerStats.EXP_STOCK_RATE).ToString("N0") + "</color>";
        }
        else
        {
            addedExpString = "";
            addedExpStockString = "";
        }

        expText.text = "Experience: " + (totalExp - addedExp).ToString("N0") + addedExpString + "\n";
        expText.text += "Stocked Experience: " + ((totalExp - addedExp) * PlayerStats.EXP_STOCK_RATE).ToString("N0") + addedExpStockString;
        expStringInitialized = true;
    }

    public void UpdateFragString()
    {
        string addedFragString;

        if (addedFrag > 0)
        {
            addedFragString = "<color=#00b33c> +" + addedFrag.ToString("N0") + "</color>";
        }
        else if (addedFrag < 0)
        {
            addedFragString = "<color=#990000> " + addedFrag.ToString("N0") + "</color>";
        }
        else
        {
            addedFragString = "";
        }

        fragText.text = "Item Fragments: " + (totalFrag - addedFrag).ToString("N0") + addedFragString + "\n";
        fragStringInitialized = true;
    }

    public void SetExpGainValues(int old, int added)
    {
        addedExp = added;
        totalExp = old;
        expChangePerSecond = System.Math.Abs(added * Time.unscaledDeltaTime);
    }

    public void SetFragGainValues(int old, int added)
    {
        addedFrag = added;
        totalFrag = old;
        fragChangePerSecond = System.Math.Abs(added * Time.unscaledDeltaTime);
    }

    public void AddToBodyText(string s)
    {
        bodyText.text += s;
    }

    public void LoadMainMenu()
    {
        StageManager.Instance.BattleManager.AllocateRewards();
        GameManager.Instance.MoveToMainMenu();
    }

    public void StartNextLoop()
    {
        StageManager.Instance.BattleManager.StartSurvivalBattle();
        this.gameObject.SetActive(false);
    }
}