using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroDetailMainPage : MonoBehaviour, IUpdatablePanel
{
    [SerializeField]
    private TextMeshProUGUI nameText;

    [SerializeField]
    private TextMeshProUGUI levelText;

    [SerializeField]
    private TextMeshProUGUI expText;

    [SerializeField]
    private TextMeshProUGUI apText;

    [SerializeField]
    private TextMeshProUGUI killText;

    [SerializeField]
    private Image primaryArchetypeHeader;

    [SerializeField]
    private Image secondaryArchetypeHeader;

    [SerializeField]
    private Image xpBarFill;

    [SerializeField]
    private HeroStatBox healthBox;

    [SerializeField]
    private HeroStatBox healthRegenBox;

    [SerializeField]
    private HeroStatBox soulBox;

    [SerializeField]
    private HeroStatBox soulRegenBox;

    [SerializeField]
    private HeroStatBox shieldRegenBox;

    [SerializeField]
    private List<HeroStatBox> attributeBoxes;

    [SerializeField]
    private List<HeroStatBox> defenseBoxes;

    [SerializeField]
    private List<HeroStatBox> resistanceBoxes;

    private HeroData hero;
    public Button lockButton;

    private void OnEnable()
    {
        UpdateWindow();
    }

    public void UpdateWindow()
    {
        hero = HeroDetailWindow.hero;
        nameText.text = hero.Name;

        if (hero.IsLocked)
        {
            lockButton.GetComponentInChildren<TextMeshProUGUI>().text = "Locked";
        }
        else
        {
            lockButton.GetComponentInChildren<TextMeshProUGUI>().text = "Unlocked";
        }

        primaryArchetypeHeader.GetComponentInChildren<TextMeshProUGUI>().text = hero.PrimaryArchetype.Base.LocalizedName;
        primaryArchetypeHeader.color = Helpers.GetArchetypeStatColor(hero.PrimaryArchetype.Base);
        if (hero.SecondaryArchetype != null)
        {
            secondaryArchetypeHeader.gameObject.SetActive(true);
            secondaryArchetypeHeader.GetComponentInChildren<TextMeshProUGUI>().text = hero.SecondaryArchetype.Base.LocalizedName;
            secondaryArchetypeHeader.color = Helpers.GetArchetypeStatColor(hero.SecondaryArchetype.Base);
        }
        else
        {
            secondaryArchetypeHeader.gameObject.SetActive(false);
        }
        levelText.text = "Level <b>" + hero.Level + "</b>";
        float requiredExp = Helpers.GetRequiredExperience(hero.Level + 1);
        float currentLevelExp = Helpers.GetRequiredExperience(hero.Level);
        expText.text = "Exp: " + hero.Experience.ToString("N0");
        if (hero.Level < 100)
        {
            expText.text += "/" + requiredExp.ToString("N0") + "\n";
            xpBarFill.fillAmount = (hero.Experience - currentLevelExp) / (requiredExp - currentLevelExp);
        }
        else
        {
            expText.text += " (MAX)";
            xpBarFill.fillAmount = 1f;
        }
        apText.text = "AP <b>" + hero.ArchetypePoints + "</b>";

        killText.text = "Kills <b>" + hero.killCount.ToString("N0") + "</b>";

        healthBox.statText.text = hero.MaximumHealth.ToString("N0");
        healthRegenBox.statText.text = hero.HealthRegenRate.ToString("N1") + "/s";

        soulBox.statText.text = hero.MaximumSoulPoints.ToString("N0");
        soulRegenBox.statText.text = hero.SoulRegenRate.ToString("N1") + "/s";

        attributeBoxes[0].statText.text = hero.Strength.ToString("N0");
        attributeBoxes[1].statText.text = hero.Intelligence.ToString("N0");
        attributeBoxes[2].statText.text = hero.Agility.ToString("N0");
        attributeBoxes[3].statText.text = hero.Will.ToString("N0");
        defenseBoxes[0].statText.text = hero.Armor.ToString("N0");
        defenseBoxes[1].statText.text = hero.MaximumManaShield.ToString("N0");
        defenseBoxes[2].statText.text = hero.DodgeRating.ToString("N0");
        defenseBoxes[3].statText.text = hero.ResolveRating.ToString("N0");

        foreach (ElementType element in Enum.GetValues(typeof(ElementType)))
        {
            float resistance = hero.GetResistance(element);
            float uncapResistance = hero.GetUncapResistance(element);

            resistanceBoxes[(int)element].statText.text = resistance.ToString() + "%";

            if (uncapResistance > resistance)
            {
                resistanceBoxes[(int)element].statText.text += " (" + uncapResistance + ")";
            }
        }
    }

    public void DebugLevelUp()
    {
        if (hero != null)
            hero.AddExperience(500020);
        UpdateWindow();
    }

    public void OpenNameEdit()
    {
        UIManager.Instance.PopUpWindow.OpenTextInput(hero.Name);
        UIManager.Instance.PopUpWindow.textInput.characterLimit = 20;
        UIManager.Instance.PopUpWindow.textInput.contentType = TMP_InputField.ContentType.Alphanumeric;
        UIManager.Instance.PopUpWindow.textInput.lineType = TMP_InputField.LineType.SingleLine;
        UIManager.Instance.PopUpWindow.SetButtonValues("Confirm", delegate
        {
            UIManager.Instance.CloseCurrentWindow();
            if (!string.IsNullOrWhiteSpace(UIManager.Instance.PopUpWindow.textInput.text))
                hero.Name = UIManager.Instance.PopUpWindow.textInput.text;
            UpdateWindow();
        }, null, null);
    }

    public void SetHeroLocked()
    {
        if (!hero.IsLocked)
        {
            hero.IsLocked = true;
            lockButton.GetComponentInChildren<TextMeshProUGUI>().text = "Locked";
        }
        else
        {
            hero.IsLocked = false;
            lockButton.GetComponentInChildren<TextMeshProUGUI>().text = "Unlocked";
        }
    }
}