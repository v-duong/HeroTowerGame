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
    private TextMeshProUGUI currentStatText;

    [SerializeField]
    private TextMeshProUGUI newStatText;

    [SerializeField]
    private TextMeshProUGUI levelText;

    [SerializeField]
    private TextMeshProUGUI costText;

    [SerializeField]
    private Slider levelSlider;

    private int selectedLevel;

    private void OnEnable()
    {
        selectedHero = null;
        heroSlot.GetComponentInChildren<TextMeshProUGUI>().text = "SELECT HERO";
        currentStatText.text = "";
        newStatText.text = "";
        levelSlider.minValue = 2;
        selectedLevel = 2;
        levelSlider.interactable = false;
        confirmButton.interactable = false;
        OnSliderChange();
        costText.text = "0" + " / " + GameManager.Instance.PlayerStats.ExpStock.ToString("N0");
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
        levelSlider.value = selectedHero.Level + 1;
        selectedLevel = selectedHero.Level + 1;
        levelSlider.minValue = selectedHero.Level + 1;
        UpdatePanels();
    }

    public void UpdatePanels()
    {
        currentStatText.text = "";
        newStatText.text = "";
        costText.text = "";
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

        currentStatText.text += (int)selectedHero.BaseHealth + "\n";
        currentStatText.text += (int)selectedHero.BaseSoulPoints + "\n";
        currentStatText.text += (int)selectedHero.BaseStrength + "\n";
        currentStatText.text += (int)selectedHero.BaseIntelligence + "\n";
        currentStatText.text += (int)selectedHero.BaseAgility + "\n";
        currentStatText.text += (int)selectedHero.BaseWill + "\n";

        int levelDifference = selectedLevel - selectedHero.Level;

        float newHealth = selectedHero.BaseHealth + selectedHero.PrimaryArchetype.HealthGrowth * levelDifference;
        float newSoulpoint = selectedHero.BaseSoulPoints + selectedHero.PrimaryArchetype.SoulPointGrowth * levelDifference;
        float newStrength = selectedHero.BaseStrength + selectedHero.PrimaryArchetype.StrengthGrowth * levelDifference;
        float newIntelligence = selectedHero.BaseIntelligence + selectedHero.PrimaryArchetype.IntelligenceGrowth * levelDifference;
        float newAgility = selectedHero.BaseAgility + selectedHero.PrimaryArchetype.AgilityGrowth * levelDifference;
        float newWill = selectedHero.BaseWill + selectedHero.PrimaryArchetype.WillGrowth * levelDifference;

        if (selectedHero.SecondaryArchetype != null)
        {
            newHealth += selectedHero.SecondaryArchetype.HealthGrowth / 2 * levelDifference;
            newSoulpoint += selectedHero.SecondaryArchetype.SoulPointGrowth / 2 * levelDifference;
            newStrength += selectedHero.SecondaryArchetype.StrengthGrowth / 2 * levelDifference;
            newIntelligence += selectedHero.SecondaryArchetype.IntelligenceGrowth / 2 * levelDifference;
            newAgility += selectedHero.SecondaryArchetype.AgilityGrowth / 2 * levelDifference;
            newWill += selectedHero.SecondaryArchetype.WillGrowth / 2 * levelDifference;
        }

        newStatText.text += (int)newHealth + "\n";
        newStatText.text += (int)newSoulpoint + "\n";
        newStatText.text += (int)newStrength + "\n";
        newStatText.text += (int)newIntelligence + "\n";
        newStatText.text += (int)newAgility + "\n";
        newStatText.text += (int)newWill + "\n";

        int requiredExperience = Helpers.GetRequiredExperience(selectedLevel) - selectedHero.Experience;

        costText.text = requiredExperience.ToString("N0") + " / " + GameManager.Instance.PlayerStats.ExpStock.ToString("N0");

        heroSlot.GetComponentInChildren<TextMeshProUGUI>().text = selectedHero.Name + "\nLv " + selectedHero.Level + "\n\n" + selectedHero.Experience;

        if (requiredExperience > GameManager.Instance.PlayerStats.ExpStock)
            confirmButton.interactable = false;
        else
            confirmButton.interactable = true;
    }

    public void OnSliderChange()
    {
        levelText.text = "Lv " + levelSlider.value;
        selectedLevel = (int)levelSlider.value;
        UpdatePanels();
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
        levelSlider.value = selectedHero.Level + 1;
        selectedLevel = selectedHero.Level + 1;
        levelSlider.minValue = selectedHero.Level + 1;
        UpdatePanels();
    }
}