using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroDissolvePanel : MonoBehaviour
{
    public Button confirmButton;
    public TextMeshProUGUI textBox;
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI slotsText;
    public TextMeshProUGUI helpText;
    public Button ArchetypeButton1;
    public Button ArchetypeButton2;
    private HeroData selectedHero;

    private ArchetypeBase archetypeToKeep = null;

    private void OnEnable()
    {
        selectedHero = null;
        archetypeToKeep = null;
        ArchetypeButton1.gameObject.SetActive(false);
        ArchetypeButton2.gameObject.SetActive(false);
        confirmButton.interactable = false;
        buttonText.text = "Select Hero";
        helpText.text = "";
        textBox.text = "";
        slotsText.text = "Hero Slots: " + GameManager.Instance.PlayerStats.HeroList.Count + "/" + PlayerStats.maxHeroes;
    }

    public void ArchetypeSelectOnClick()
    {
        UIManager.Instance.OpenHeroWindow(false);
        UIManager.Instance.HeroScrollContent.SetCallback(HeroSelect_Callback);
    }

    public void HeroSelect_Callback(HeroData hero)
    {
        if (hero.assignedTeam != -1)
        {
            PopUpWindow popUpWindow = UIManager.Instance.PopUpWindow;
            popUpWindow.OpenTextWindow("Hero is currently assigned to a team. Remove hero from team before recycling.", 380, 200);
            popUpWindow.textField.fontSize = 24;
            popUpWindow.textField.paragraphSpacing = 0;
            popUpWindow.textField.alignment = TextAlignmentOptions.Center;

            popUpWindow.SetButtonValues(null, null, "Confirm", delegate { UIManager.Instance.CloseCurrentWindow(); });
            return;
        }

        UIManager.Instance.CloseCurrentWindow();

        selectedHero = hero;

        if (selectedHero == null)
        {
            buttonText.text = "Select Hero";
            textBox.text = "";
            helpText.text = "";
            ArchetypeButton1.gameObject.SetActive(false);
            ArchetypeButton2.gameObject.SetActive(false);
            confirmButton.interactable = false;
            return;
        }

        buttonText.text = "Lv" + hero.Level + " " + hero.Name;
        helpText.text = "You may select one archetype to keep. Doing so will decrease the amount of XP stock gained.\n";
        textBox.text = "XP Stock Gained: " + (hero.Experience * 0.4f).ToString("N0");

        ArchetypeButton1.image.color = Color.white;
        ArchetypeButton1.GetComponentInChildren<TextMeshProUGUI>().text = hero.PrimaryArchetype.Base.LocalizedName;
        ArchetypeButton1.gameObject.SetActive(true);

        if (hero.SecondaryArchetype != null)
        {
            ArchetypeButton2.image.color = Color.white;
            ArchetypeButton2.GetComponentInChildren<TextMeshProUGUI>().text = hero.SecondaryArchetype.Base.LocalizedName;
            ArchetypeButton2.gameObject.SetActive(true);
        }
        else
        {
            ArchetypeButton2.gameObject.SetActive(false);
        }

        confirmButton.interactable = true;
    }

    public void SelectArchetype(int i)
    {
        ArchetypeBase archetype;
        Button button;
        Button button2;
        if (i == 0)
        {
            archetype = selectedHero.PrimaryArchetype.Base;
            button = ArchetypeButton1;
            button2 = ArchetypeButton2;
        }
        else
        {
            archetype = selectedHero.SecondaryArchetype.Base;
            button = ArchetypeButton2;
            button2 = ArchetypeButton1;
        }

        if (archetypeToKeep == archetype)
        {
            archetypeToKeep = null;
            button.image.color = Color.white;
            button2.image.color = Color.white;
            textBox.text = "XP Stock Gained: " + (selectedHero.Experience * 0.4f).ToString("N0");
        }
        else
        {
            archetypeToKeep = archetype;
            button.image.color = Helpers.SELECTION_COLOR;
            button2.image.color = Color.white;
            textBox.text = "Archetype " + archetypeToKeep.LocalizedName + " will be returned as an item.\nXP Stock Gained: " + (selectedHero.Experience * 0.2f).ToString("N0");
        }
    }

    public void OnClickConfirm()
    {
        if (selectedHero == null)
            return;

        for (int i = 0; i < 3; i++)
        {
            selectedHero.UnequipAbility(i);
        }
        foreach (EquipSlotType e in Enum.GetValues(typeof(EquipSlotType)))
        {
            selectedHero.UnequipFromSlot(e);
        }

        float xp;
        if (archetypeToKeep == null)
        {
            xp = selectedHero.Experience * 0.4f;
        }
        else
        {
            GameManager.Instance.PlayerStats.AddArchetypeToInventory(ArchetypeItem.CreateArchetypeItem(archetypeToKeep, selectedHero.Level));
            xp = selectedHero.Experience * 0.2f;
        }

        GameManager.Instance.PlayerStats.RemoveHeroFromList(selectedHero);

        selectedHero = null;
        archetypeToKeep = null;

        GameManager.Instance.PlayerStats.ModifyExpStock((int)xp);
        SaveManager.Save();
        ArchetypeButton1.gameObject.SetActive(false);
        ArchetypeButton2.gameObject.SetActive(false);
        buttonText.text = "Select Hero";
        helpText.text = "";
        textBox.text = "";
        slotsText.text = "Hero Slots: " + GameManager.Instance.PlayerStats.HeroList.Count + "/" + PlayerStats.maxHeroes;
    }
}