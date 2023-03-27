using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    private const float POSITION_Y = .8f;

    protected GridInteractor GGridInteractor;
    protected GridGenerator _gridGenerator;
    protected Cell[,] GridCells = null;

    public List<Unit> AllUnits;


    private void Awake()
    {
        _gridGenerator = FindObjectOfType<GridGenerator>();
        var Width = _gridGenerator.GridSize.x;
        var Height = _gridGenerator.GridSize.y;

        GridCells = new Cell[Width, Height];

        foreach (var cell in GGridInteractor.Cells)
        {
            int x = Convert.ToInt32(cell.Coordinates.x);
            int y = Convert.ToInt32(cell.Coordinates.y);
            cell.Initialize(x, y, GGridInteractor, true, StatusUnitOn.No);
            GridCells[x, y] = cell;
        }
        // Получить все Unit в игре и добавить их в список AllUnits
        //AllUnits = FindObjectsOfType<Unit>().ToList();
        foreach (Unit unit in AllUnits)
        {
            unit.InitializeUnit();
        }
    }

    public void AddUnit(Unit unit)
    {
        AllUnits.Add(unit);
        unit.transform.position = new Vector3(transform.position.x, POSITION_Y, transform.position.z);
    }

    public void RemoveUnit(Unit unit)
    {
        if (AllUnits.Contains(unit))
        {
            AllUnits.Remove(unit);
        }
    }
}


