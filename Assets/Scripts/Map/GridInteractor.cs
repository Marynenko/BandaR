using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class GridInteractor : Grid
{
    [SerializeField] private List<Cell> _availableMoves;
    [SerializeField] private readonly int _playerMaxMoves;
    [SerializeField] private readonly int _enemyMaxMoves;

    public Unit SelectedUnit { get; private set; }

    public void SelectUnit(Unit unit)
    {
        if (SelectedUnit != null)
        {
            UnselectUnit(SelectedUnit);
        }

        SelectedUnit = unit;
    }

    public void SelectCell(Cell cell, UnitType unitType)
    {
        _availableMoves = GetAvailableMoves(cell, unitType);
        foreach (var move in _availableMoves)
        {
            move.ChangeColor(move.CellMovementColor);
        }
    }



    public List<Cell> GetAvailableMoves(Cell cell, UnitType unitType)
    {
        List<Cell> availableMoves = new List<Cell>();

        if (unitType == UnitType.Player)
        {
            // проверяем все клетки вокруг текущей клетки и добавляем их в список возможных ходов, если они свободны
            for (int x = cell.GridX - 1; x <= cell.GridX + 1; x++)
            {
                for (int y = cell.GridY - 1; y <= cell.GridY + 1; y++)
                {
                    if (x >= 0 && x < _grid.Width && y >= 0 && y < _grid.Height)
                    {
                        Cell adjacentCell = _grid.Cells[x, y];
                        if (adjacentCell != cell && adjacentCell.IsEmpty && !adjacentCell.HasObstacle)
                        {
                            availableMoves.Add(adjacentCell);
                        }
                    }
                }
            }
        }

        return availableMoves;
    }


    public void UnselectUnit(Unit unit)
    {
        SelectedUnit = null;
        foreach (var move in _availableMoves)
        {
            move.ChangeColor(move.CellStandardColor);
        }
    }

    public bool CanMoveToCell(Unit unit, Cell cell)
    {
        return _availableMoves.Contains(cell);
    }

    public void MoveUnitToCell(Unit unit, Cell cell)
    {
        if (CanMoveToCell(unit, cell))
        {
            unit.MoveTo(cell);
            foreach (var move in _availableMoves)
            {
                move.ChangeColor(move.CellStandardColor);
            }
        }
    }





}


