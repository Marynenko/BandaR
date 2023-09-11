using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public GridGenerator Generator { get; private set; }

    private void Awake()
    {
        Generator = GetComponentInChildren<GridGenerator>();
    }

    public void SetAvailableTiles()
    {
        foreach (var tile in Generator.Tiles)
            if (!tile.IsOccupied())
                tile.SetAvailable(true);
    }

    public void RemoveUnit(Unit unit)
    {
        var unitToRemove = unit as Unit;
        var currentTile = unitToRemove.OccupiedTile;

        if (currentTile != null)
        {
            //currentTile.ClearUnit();
            Generator.AllUnits.Remove(unitToRemove);
            Destroy(unitToRemove.gameObject);
        }
    }
}
