using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class GridInteractor : Grid
{
    public List<Cell> Cells = new List<Cell>();

    public Unit SelectedUnit { get; internal set; }
    private Unit _selectedUnit;
    public Unit SelectedUnit => _selectedUnit;
    private Cell _selectedCell;

    public Cell SelectedCell => _selectedCell;

    // Выбираем юнит и сохраняем ссылку на него
    public void SelectUnit(Unit unit)
    {
        UnselectUnit();

        _selectedUnit = unit;
        _selectedUnit.CurrentCell.ChangeColor(_selectedUnit.CurrentCell.CellSelectedColor);
    }

    // Снимаем выделение с юнита и обнуляем ссылку
    public void UnselectUnit()
    {
        if (_selectedUnit != null)
        {
            _selectedUnit.CurrentCell.ChangeColor(_selectedUnit.CurrentCell.CellStandardColor);
            _selectedUnit.CurrentCell.UnitOn = UnitOnStatus.No;
            _selectedUnit.Status = UnitStatus.Unselected;

            _selectedUnit = null;
        }
    }

    // Выбираем клетку и сохраняем ссылку на нее
    public void SelectCell(Cell cell, UnitType unitType)
    {
        UnselectCell();

        _selectedCell = cell;
        _selectedCell.ChangeColor(_selectedCell.CellSelectedColor);

        if (unitType == UnitType.Player)
        {
            _selectedCell.MarkCellAsReachable();
        }
    }

    // Снимаем выделение с клетки и обнуляем ссылку
    public void UnselectCell()
    {
        if (_selectedCell != null)
        {
            _selectedCell.ChangeColor(_selectedCell.CellStandardColor);
            _selectedCell.UnitOn = UnitOnStatus.No;
            _selectedCell.UnmarkCellAsReachable();

            _selectedCell = null;
        }
    }


   public bool CanMoveToCell(Unit selectedUnit, Cell cell)
{
    // Получаем все клетки, на которые можно переместиться для данного юнита
    var reachableCells = GetReachableCells(selectedUnit);

    // Проверяем, есть ли переданная клетка в списке достижимых
    return reachableCells.Contains(cell);
}

public void MoveUnitToCell(Unit selectedUnit, Cell cell)
{
    // Получаем все клетки, на которые можно переместиться для данного юнита
    var reachableCells = GetReachableCells(selectedUnit);

    // Если переданная клетка является достижимой, перемещаем юнит
    if (reachableCells.Contains(cell))
    {
        // Снимаем выбор с предыдущей клетки
        UnselectCell(selectedUnit.CurrentCell);

        // Обновляем текущую клетку юнита
        selectedUnit.CurrentCell = cell;

        // Помечаем клетку, на которую перемещаемся, как выбранную и находится на ней юнит
        SelectCell(cell, selectedUnit.Type);
        cell.UnitOn = UnitOnStatus.Yes;
        cell.ChangeColor(cell.CellSelectedColor);

        // Обновляем статус юнита
        selectedUnit.Status = UnitStatus.Moved;
    }
}

    internal void UnselectUnit(Unit unit)
    {
        throw new NotImplementedException();
    }
}


