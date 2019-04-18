﻿using System.Collections.Generic;

public class ArchetypeItem : Item
{
    public ArchetypeBase Base { get { return ResourceManager.Instance.GetArchetypeBase(BaseId); } }
    private string BaseId { get; set; }


    protected ArchetypeItem(ArchetypeBase b)
    {
        BaseId = b.idName;
    }

    public static ArchetypeItem CreateRandomArchetypeItem(int ilvl)
    {
        return new ArchetypeItem(ResourceManager.Instance.GetRandomArchetypeBase(ilvl));
    }

}