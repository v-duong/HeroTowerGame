﻿using System;
using System.Collections.Generic;

public class ArchetypeItem : Item
{
    public ArchetypeBase Base { get; private set; }

    protected ArchetypeItem(ArchetypeBase b)
    {
        Id = Guid.NewGuid();
        Base = b;
        Name = LocalizationManager.Instance.GetLocalizationText_Archetype(b.idName);
    }

    public ArchetypeItem(Guid id, string baseName)
    {
        Id = id;
        Base = ResourceManager.Instance.GetArchetypeBase(baseName);
        Name = LocalizationManager.Instance.GetLocalizationText_Archetype(Base.idName);
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

    public static int GetFragmentWorth(int stars)
    {
        switch(stars)
        {
            case 0:
                return 0;
            case 1:
                return 1;
            case 2:
                return 3;
            case 3:
                return 5;
            case 4:
                return 8;
            case 5:
                return 13;
            default:
                return 0;
        }
    }
}