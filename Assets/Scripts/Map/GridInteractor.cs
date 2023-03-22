using System.Collections.Generic;
using UnityEngine;

public class GridInteractor : Grid
{

    [SerializeField] private List<Cell> _availableMoves;
    [SerializeField] private readonly int _playerMaxMoves;
    [SerializeField] private readonly int _enemyMaxMoves;

    public List<Cell> Cells;
    public Unit SelectedUnit { get; set; }
    private void Start()
    {
         //+= OnPlayerSelected;
    }


    public void SelectUnit(Unit unit)
    {
        UnselectUnit(SelectedUnit);
        SelectedUnit = unit;
    }

    public void UnselectUnit(Unit unit)
    {
        if (unit == SelectedUnit)
        {
            SelectedUnit = null;
        }
    }

    public void SelectCell(Cell cell, UnitType unitType)
    {
        UnselectCells();
        if (unitType == UnitType.Player)
        {
            _availableMoves = GetAvailableMoves(cell, unitType, _playerMaxMoves);
        }
        else if (unitType == UnitType.Enemy)
        {
            _availableMoves = GetAvailableMoves(cell, unitType, _enemyMaxMoves);
        }
        foreach (var moveCell in _availableMoves)
        {
            moveCell.ChangeColor(moveCell.CellMovementColor);
        }
    }

    public List<Cell> GetAvailableMoves(Cell cell, UnitType unitType, int maxMoves)
    {
        List<Cell> result = new List<Cell>();
        result.Add(cell);
        if (maxMoves <= 0)
        {
            return result;
        }
        foreach (var neighbour in cell.Neighbours)
        {
            if (neighbour.IsWalkable() && neighbour.UnitOn == UnitOnStatus.No)
            {
                result.AddRange(GetAvailableMoves(neighbour, unitType, maxMoves - 1));
            }
        }
        return result;
    }

    //public List<Cell> GetAvailableMoves(Cell cell, UnitType unitType)
    //{
    //    if (unitType == UnitType.Player)
    //    {
    //        return GetAvailableMoves(cell, unitType, _playerMaxMoves);
    //    }
    //    else if (unitType == UnitType.Enemy)
    //    {
    //        return GetAvailableMoves(cell, unitType, _enemyMaxMoves);
    //    }
    //    return new List<Cell>();
    //}

    public void UnselectCells()
    {
        foreach (var cell in Cells)
        {
            cell.ChangeColor(cell.CellStandardColor);
        }
    }

    public void MoveUnitToCell(Unit unit, Cell cell)
    {
        //unit.MoveToCell(cell);
        UnselectUnit(unit);
        UnselectCells();
    }
}
