using System;
using System.Collections;
using System.Collections.Generic;
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
                var availableMoves = _gridInteractor.AvailableMoves;

                foreach (var cell in availableMoves)
                {
                    if (!cell.Equals(unit.CurrentCell))
                    {
                        _gridInteractor.HighlightCell(cell, cell.ColorMovementCell);
                    }
                }
            }
        }
        else if (selectedUnit.Equals(unit))
        {
            return;
        }
        else if (unit.Type == UnitType.Player)
        {
            _gridInteractor.SelectUnit(unit);
            var currentCell = unit.CurrentCell;
            var availableMoves = _gridInteractor.AvailableMoves;

            foreach (var cell in availableMoves)
            {
                if (!cell.Equals(currentCell))
                {
                    _gridInteractor.HighlightCell(cell, cell.ColorMovementCell);
                }
            }
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
            var currentCell = unit.CurrentCell;
            var availableMoves = _gridInteractor.AvailableMoves;

            foreach (var cell in availableMoves)
            {
                if (!cell.Equals(currentCell))
                {
                    _gridInteractor.HighlightCell(cell, cell.ColorMovementCell);
                }
            }
        }
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

        var availableMoves = _gridInteractor.GetAvailableMoves(selectedUnit.CurrentCell, selectedUnit.Type, 1);

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

