using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RayHandler : MonoBehaviour
{
    [SerializeField] private GridInteractor _gridInteractor;

    public static event Action<UnitActionType, Unit, Cell> OnUnitAction;

    private void Start()
    {
        _gridInteractor.InitializeActions();
    }

    public void HandleUnitClick(Unit unit)
    {
        var selectedUnit = _gridInteractor.SelectedUnit;

        if (selectedUnit == null)
        {
            if (unit.Type == UnitType.Player)
            {
                _gridInteractor.SelectUnit(unit);
                HighlightAvailableMoves(_gridInteractor.AvailableMoves, unit.CurrentCell.ColorMovementCell);
            }
        }
        else if (selectedUnit.Equals(unit))
        {
            return;
        }
        else if (unit.Type == UnitType.Player)
        {
            _gridInteractor.SelectUnit(unit);
            HighlightAvailableMoves(_gridInteractor.AvailableMoves, unit.CurrentCell.ColorMovementCell);
        }
        else if (unit.Type == UnitType.Enemy && selectedUnit.Type == UnitType.Player)
        {
            var currentCell = unit.CurrentCell;
            if (selectedUnit.CanAttack(currentCell))
            {
                selectedUnit.Attack(unit);
                _gridInteractor.UnselectCells();
            }
        }
        else
        {
            _gridInteractor.UnselectUnit(selectedUnit);
            _gridInteractor.SelectUnit(unit);
            HighlightAvailableMoves(_gridInteractor.AvailableMoves, unit.CurrentCell.ColorMovementCell);
        }
    }

    private void HighlightAvailableMoves(IReadOnlyList<Cell> availableMoves, Color color)
    {
        _gridInteractor.UnselectCells();
        _gridInteractor.HighlightCell(availableMoves.First(), availableMoves.First().ColorUnitOnCell);
        availableMoves.Skip(1).ToList().ForEach(cell => _gridInteractor.HighlightCell(cell, color));
    }



    public void HandleCellClick(Cell cell)
    {
        var selectedUnit = _gridInteractor.SelectedUnit;

        if (selectedUnit == null || selectedUnit.Type != UnitType.Player || selectedUnit.Status != UnitStatus.Selected)
        {
            return;
        }

        if (cell == selectedUnit.CurrentCell)
        {
            return;
        }

        //var availableMoves = _gridInteractor.GetAvailableMoves(selectedUnit.CurrentCell, selectedUnit.Type, 1);
        _gridInteractor.UnselectUnit(selectedUnit);
        var availableMoves = _gridInteractor.GetAvailableMoves(cell, selectedUnit.Type, 1);

        if (availableMoves.Contains(cell))
        {
            _gridInteractor.MoveUnit(selectedUnit, cell);

            // Снимаем выделение с текущей ячейки
            selectedUnit.CurrentCell.ChangeColor(selectedUnit.CurrentCell.ColorStandardCell);
            selectedUnit.CurrentCell.UnitOn = StatusUnitOn.No;
            selectedUnit.CurrentCell.SetIsWalkable(true);

            // Выделяем ячейку, на которую переместился юнит
            cell.ChangeColor(selectedUnit.CurrentCell.ColorUnitOnCell);
            cell.UnitOn = StatusUnitOn.Yes;

            _gridInteractor.UnselectUnit(selectedUnit);

            OnUnitAction?.Invoke(UnitActionType.Move, selectedUnit, cell);
        }

    }
}

