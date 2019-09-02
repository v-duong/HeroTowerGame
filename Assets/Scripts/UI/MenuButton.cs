using UnityEngine;
using UnityEngine.UI;

public class MenuButton : UIKeyButton
{

    public void OnClickInvToggle()
    {
        UIManager.Instance.OpenInventoryWindow();
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
    }

    public void AddHero()
    {
        HeroData hero = HeroData.CreateNewHero("TEST" + Random.Range(4, 100), ArchetypeItem.CreateRandomArchetypeItem(100), ArchetypeItem.CreateRandomArchetypeItem(100));
        GameManager.Instance.PlayerStats.AddHeroToList(hero);
    }

    public void CloseWindow()
    {
        UIManager.Instance.CloseCurrentWindow();
    }

    public void UnloadMainMenu()
    {
        //GameManager.Instance.MoveToBattle("stage1-1NORMAL");
    }

    public void LoadMainMenu()
    {
        GameManager.Instance.MoveToMainMenu();
    }
}