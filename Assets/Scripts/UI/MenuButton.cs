using UnityEngine;

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

    public void OnClickSettings()
    {
        UIManager.Instance.PopUpWindow.OpenSettingsWindow();
    }

    public void AddItem()
    {
        Equipment equipment = Equipment.CreateRandomEquipment(100);
        GameManager.Instance.PlayerStats.AddEquipmentToInventory(equipment);
        GameManager.Instance.PlayerStats.AddArchetypeToInventory(ArchetypeItem.CreateRandomArchetypeItem(100));
        GameManager.Instance.PlayerStats.AddEquipmentToInventory(Equipment.CreateRandomUnique(100));
    }

    public void AddAccessories()
    {
        Equipment equipment = Equipment.CreateRandomEquipment(100, GroupType.RING);
        Equipment equipment2 = Equipment.CreateRandomEquipment(100, GroupType.NECKLACE);
        Equipment equipment3 = Equipment.CreateRandomEquipment(100, GroupType.BELT);
        GameManager.Instance.PlayerStats.AddEquipmentToInventory(equipment);
        GameManager.Instance.PlayerStats.AddEquipmentToInventory(equipment2);
        GameManager.Instance.PlayerStats.AddEquipmentToInventory(equipment3);
    }

    public void AddHero()
    {
        ArchetypeItem a1 = ArchetypeItem.CreateRandomArchetypeItem(100);
        ArchetypeItem a2 = ArchetypeItem.CreateRandomArchetypeItem(100);
        while (a1.Base == a2.Base)
        {
            a2 = ArchetypeItem.CreateRandomArchetypeItem(100);
        }
        HeroData hero = HeroData.CreateNewHero("TEST" + Random.Range(4, 500), a1.Base, a2.Base);
        GameManager.Instance.PlayerStats.AddHeroToList(hero);
    }

    public void CloseWindow()
    {
        UIManager.Instance.CloseCurrentWindow();
    }

    public void LoadData()
    {
        SaveManager.Load();
    }

    public void SaveData()
    {
        SaveManager.SaveAll();
    }
}