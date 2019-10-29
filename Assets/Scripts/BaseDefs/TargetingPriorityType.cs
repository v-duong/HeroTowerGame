﻿using System;

public enum PrimaryTargetingType
{
    CLOSEST,
    FURTHEST,
    FIRST,
    LAST,
    LEAST_HEALTH,
    MOST_HEALTH,
    LOWEST_HEALTH_PERCENT,
    HIGHEST_HEALTH_PERCENT,
    RANDOM,
}

[Flags]
public enum SecondaryTargetingFlags
{
    NONE = 0,
    PRIORITIZE_ATTACKERS = 0b1,
    PRIORITIZE_RARITY = 0b10,
}