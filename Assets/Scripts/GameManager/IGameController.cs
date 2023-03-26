using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameController
{
    void HandleUnitClick(Unit unit);
    void HandleCellClick(Cell cell);
    void HighlightAvailableMoves(IReadOnlyList<Cell> availableMoves, Color color);
}

