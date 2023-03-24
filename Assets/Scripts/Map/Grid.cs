using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{   
    [SerializeField] protected GridInteractor GGridInteractor;
    protected GridGenerator _gridGenerator;
    protected Cell[,] GridCells = null;

    public List<Unit> Units;


    protected void Initialization()
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
        // Получить все Unit в игре и добавить их в список Units
        //Units = FindObjectsOfType<Unit>().ToList();
        foreach (Unit unit in Units)
        {
            unit.InitializeUnit();
        }
    }

}
