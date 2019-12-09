using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XpStockPanel : MonoBehaviour
{
    [SerializeField]
    private HeroData selectedHero;

    [SerializeField]
    private Button heroSlot;

    [SerializeField]
    private Button confirmButton;

    [SerializeField]
    private Button plusOneButton;
    [SerializeField]
    private Button minusOneButton;

    [SerializeField]
    private Slider levelSlider;

    [SerializeField]
    private XpStockStatLabel levelStatLine;

    [SerializeField]
    private XpStockStatLabel healthStatLine;

    [SerializeField]
    private XpStockStatLabel strStatLine;

    [SerializeField]
    private XpStockStatLabel intStatLine;

    [SerializeField]
    private XpStockStatLabel agiStatLine;

    [SerializeField]
    private XpStockStatLabel willStatLine;

    [SerializeField]
    private XpStockStatLabel stockLine;

    private int selectedLevel;

    private void OnEnable()
    {
        selectedHero = null;
        heroSlot.GetComponentInChildren<TextMeshProUGUI>().text = "SELECT HERO";
        levelStatLine.ClearValues();
        healthStatLine.ClearValues();
        strStatLine.ClearValues();
        intStatLine.ClearValues();
        agiStatLine.ClearValues();
        willStatLine.ClearValues();
        levelSlider.minValue = 2;
        selectedLevel = 2;
        levelSlider.interactable = false;
        confirmButton.interactable = false;
        OnSliderChange();
        float xpStock = GameManager.Instance.PlayerStats.ExpStock;
        stockLine.SetValues(xpStock, 0, xpStock);
        stockLine.SetNeutralColor();
    }

    public void OnHeroSlotClick()
    {
        UIManager.Instance.OpenHeroWindow(false, x => x.Level < 100);
        UIManager.Instance.HeroScrollContent.SetCallback(OnHeroSelect);
    }

    public void OnHeroSelect(HeroData hero)
    {
        UIManager.Instance.CloseCurrentWindow();
        selectedHero = hero;
        UpdateSliderValues();
        UpdatePanels();
    }

    public void UpdateSliderValues()
    {
        int level = Mathf.Min(selectedHero.Level + 1, 100);
        levelSlider.value = level;
        selectedLevel = level;
        levelSlider.minValue = level;
    }

    public void UpdatePanels()
    {
        if (selectedHero == null)
            return;

        if (selectedHero.Level == 100)
        {
            levelSlider.interactable = false;
            confirmButton.interactable = false;
        }
        else
        {
            levelSlider.interactable = true;
            confirmButton.interactable = true;
        }

        int levelDifference = selectedLevel - selectedHero.Level;
        float newHealth = selectedHero.BaseHealth + selectedHero.PrimaryArchetype.HealthGrowth * levelDifference;
        float newSoulpoint = selectedHero.BaseSoulPoints + selectedHero.PrimaryArchetype.SoulPointGrowth * levelDifference;
        float newStrength = selectedHero.BaseStrength + selectedHero.PrimaryArchetype.StrengthGrowth * levelDifference;
        float newIntelligence = selectedHero.BaseIntelligence + selectedHero.PrimaryArchetype.IntelligenceGrowth * levelDifference;
        float newAgility = selectedHero.BaseAgility + selectedHero.PrimaryArchetype.AgilityGrowth * levelDifference;
        float newWill = selectedHero.BaseWill + selectedHero.PrimaryArchetype.WillGrowth * levelDifference;

        if (selectedHero.SecondaryArchetype != null)
        {
            newHealth += selectedHero.SecondaryArchetype.HealthGrowth / 4 * levelDifference;
            newSoulpoint += selectedHero.SecondaryArchetype.SoulPointGrowth / 4 * levelDifference;
            newStrength += selectedHero.SecondaryArchetype.StrengthGrowth / 2 * levelDifference;
            newIntelligence += selectedHero.SecondaryArchetype.IntelligenceGrowth / 2 * levelDifference;
            newAgility += selectedHero.SecondaryArchetype.AgilityGrowth / 2 * levelDifference;
            newWill += selectedHero.SecondaryArchetype.WillGrowth / 2 * levelDifference;
        }

        levelStatLine.SetValues(selectedHero.Level, levelDifference, selectedLevel);
        healthStatLine.SetValues(selectedHero.BaseHealth, newHealth - selectedHero.BaseHealth, newHealth);
        strStatLine.SetValues(selectedHero.BaseStrength, newStrength - selectedHero.BaseStrength, newStrength);
        intStatLine.SetValues(selectedHero.BaseIntelligence, newIntelligence - selectedHero.BaseIntelligence, newIntelligence);
        agiStatLine.SetValues(selectedHero.BaseAgility, newAgility - selectedHero.BaseAgility, newAgility);
        willStatLine.SetValues(selectedHero.BaseWill, newWill - selectedHero.BaseWill, newWill);

        heroSlot.GetComponentInChildren<TextMeshProUGUI>().text = selectedHero.Name + "\nCurrent Exp: " + selectedHero.Experience;

        int xpStock = GameManager.Instance.PlayerStats.ExpStock;

        for (int i = 0; i < 100 - selectedHero.Level; i++)
        {
            int reqExpForLevel = Helpers.GetRequiredExperience(selectedHero.Level + i);
            if (reqExpForLevel - selectedHero.Experience > xpStock)
            {
                levelSlider.maxValue = selectedHero.Level + i;
                break;
            }
        }

        int requiredExperience = Helpers.GetRequiredExperience(selectedLevel) - selectedHero.Experience;
        stockLine.SetValues(xpStock, -requiredExperience, xpStock - requiredExperience);

        if (requiredExperience > GameManager.Instance.PlayerStats.ExpStock)
        {
            confirmButton.interactable = false;
            stockLine.SetRedColor();
        }
        else if (selectedHero.Level == 100)
        {
            confirmButton.interactable = false;
            stockLine.SetNeutralColor();
        }
        else
        {
            confirmButton.interactable = true;
            stockLine.SetGreenColor();
        }
    }

    public void OnSliderChange()
    {
        selectedLevel = (int)levelSlider.value;
        UpdatePanels();
    }

    public void PlusOneButtonClick()
    {
        levelSlider.value += 1;
    }

    public void MinusOneButtonClick()
    {
        levelSlider.value -= 1;
    }

    public void OnConfirmClick()
    {
        if (selectedHero == null)
            return;
        int requiredExperience = Helpers.GetRequiredExperience(selectedLevel) - selectedHero.Experience;
        if (requiredExperience > GameManager.Instance.PlayerStats.ExpStock)
            return;
        GameManager.Instance.PlayerStats.ModifyExpStock(-requiredExperience);
        selectedHero.AddExperience(requiredExperience);

        SaveManager.CurrentSave.SaveHeroData(selectedHero);
        SaveManager.CurrentSave.SavePlayerData();
        SaveManager.Save();

        UpdateSliderValues();
        UpdatePanels();
    }
}