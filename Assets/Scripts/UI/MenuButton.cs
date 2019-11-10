using UnityEngine;
using UnityEngine.UI;

public class MenuButton : UIKeyButton
{

    public void OnClickInvToggle()
    {
        UIManager.Instance.OpenInventoryWindow(true, true);
    }

    public void OnClickTeam()
    {
        UIManager.Instance.OpenTeamWindow();
    }

    public void OnClickHeroToggle()
    {
        UIManager.Instance.OpenHeroWindow();
    }

    public void OnClickWorkshop()
    {
        UIManager.Instance.OpenWorkshopWindow();
    }

    public void OnClickStageSelect()
    {
        UIManager.Instance.OpenStageSelectWindow();
    }

    public void AddItem()
    {
        Equipment equipment = Equipment.CreateRandomEquipment(100);
        GameManager.Instance.PlayerStats.AddEquipmentToInventory(equipment);
        GameManager.Instance.PlayerStats.AddArchetypeToInventory(ArchetypeItem.CreateRandomArchetypeItem(100));
        GameManager.Instance.PlayerStats.AddEquipmentToInventory(Equipment.CreateRandomUnique(100));
    }

    public void AddHero()
    {
        ArchetypeItem a1 = ArchetypeItem.CreateRandomArchetypeItem(100);
        ArchetypeItem a2 = ArchetypeItem.CreateRandomArchetypeItem(100);
        while (a1.Base == a2.Base)
        {
            a2 = ArchetypeItem.CreateRandomArchetypeItem(100);
        }
        HeroData hero = HeroData.CreateNewHero("TEST" + Random.Range(4, 500), a1, a2);
        GameManager.Instance.PlayerStats.AddHeroToList(hero);
    }

    public void CloseWindow()
    {
        UIManager.Instance.CloseCurrentWindow();
    }

    public void LoadData()
    {
        SaveManager.Instance.Load();
    }

    public void SaveData()
    {
        SaveManager.Instance.Save();
    }

}