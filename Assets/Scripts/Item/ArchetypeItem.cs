using System.Collections.Generic;

public class ArchetypeItem : Item
{
    public ArchetypeBase Base { get; private set; }

    protected ArchetypeItem(ArchetypeBase b)
    {
        Base = b;
        Name = LocalizationManager.Instance.GetLocalizationText_Archetype(b.idName);
    }

    public static ArchetypeItem CreateRandomArchetypeItem(int ilvl)
    {
        return new ArchetypeItem(ResourceManager.Instance.GetRandomArchetypeBase(ilvl));
    }

    public static ArchetypeItem CreateArchetypeItem(ArchetypeBase archetype, int ilvl)
    {
        return new ArchetypeItem(archetype);
    }

    public override ItemType GetItemType()
    {
        return ItemType.ARCHETYPE;
    }

}