﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class SaveManager : MonoBehaviour {
    public static SaveManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }
}
