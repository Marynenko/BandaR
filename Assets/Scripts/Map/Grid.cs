using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    private const float POSITION_Y = .8f;

    protected Cell[,] GridCells = null;

    public GridInteractor Interactor;
    public GridGenerator Generator;
    public List<Unit> AllUnits;


    private void Awake()
    {
        Interactor = FindObjectOfType<GridInteractor>();
        Generator = FindObjectOfType<GridGenerator>();
        var Width = Generator.GridSize.x;
        var Height = Generator.GridSize.y;

        GridCells = new Cell[Width, Height];

        InitializaionGrid();
    }

    private void InitializaionGrid()
    {
        foreach (var cell in Interactor.Cells)
        {
            int x = Convert.ToInt32(cell.Coordinates.x);
            int y = Convert.ToInt32(cell.Coordinates.y);
            cell.Initialize(x, y, Interactor, true, StatusUnitOn.No);
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


