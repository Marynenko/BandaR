﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameController
{
    void HandleUnitClick(Unit unit);
    void HandleTileClick(Tile tile);
}

