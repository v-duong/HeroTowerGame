using UnityEngine;

public class MenuButton : MonoBehaviour
{
    public void OnClickInvToggle()
    {
        UIManager.Instance.OpenInventoryWindow();
    }

    public void OnClickHeroToggle()
    {
        UIManager ui = UIManager.Instance;
        ui.CloseAllWindows();
        ui.OpenWindow(UIManager.Instance.HeroWindowRect.gameObject);
    }

    public void AddItem()
    {
        Equipment equipment = Equipment.CreateRandomEquipment(100);
        GameManager.Instance.PlayerStats.AddEquipmentToInventory(equipment);
    }

    public void AddHero()
    {
        HeroData hero = HeroData.CreateNewHero("TEST " + Random.Range(1, 321), ArchetypeItem.CreateRandomArchetypeItem(100), ArchetypeItem.CreateRandomArchetypeItem(100));
        GameManager.Instance.PlayerStats.AddHeroToList(hero);
    }

    public void CloseWindow()
    {
        UIManager.Instance.CloseCurrentWindow();
    }
}