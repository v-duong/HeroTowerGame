using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item
{
    public int Id { get; protected set; }
    public string Name { get; protected set; }
    public RarityType Rarity { get; protected set; }
    public int ItemLevel { get; protected set; }

    public void SetRarity(RarityType rarity) => Rarity = rarity;
}
