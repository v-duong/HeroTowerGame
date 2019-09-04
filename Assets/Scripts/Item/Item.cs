using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item
{
    public Guid Id { get; protected set; }
    public string Name { get; set; }
    public RarityType Rarity { get; protected set; }
    public int ItemLevel { get; protected set; }

    public void SetId(Guid id) => Id = id;
    public void SetRarity(RarityType rarity) => Rarity = rarity;
    public abstract ItemType GetItemType();
}
