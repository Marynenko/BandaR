using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour, IGameController
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
            HandleUnitAttack(selectedUnit, unit);
        }
        else
        {
            HandleUnitDeselection(selectedUnit, unit);
        }
    }

    public void HighlightAvailableMoves(IReadOnlyList<Cell> availableMoves, Color color)
    {
        _gridInteractor.UnselectCells();
        _gridInteractor.HighlightCell(availableMoves.First(), availableMoves.First().ColorUnitOnCell);
        availableMoves.Skip(1).ToList().ForEach(cell => _gridInteractor.HighlightCell(cell, color));
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
            HandleUnitAttack(selectedUnit, unit);
        }
        else
        {
            HandleUnitDeselection(selectedUnit, unit);
        }
    }

    public bool AreUnitsAdjacent(Unit unit1, Unit unit2)
    {
        var distance = Vector3.Distance(unit1.transform.position, unit2.transform.position);
        return distance <= 1f; // или другое значение, в зависимости от размеров клетки и модели юнитов
    }

    private void HandleUnitDeselection(Unit selectedUnit, Unit unit)
    {
        _gridInteractor.UnselectUnit(selectedUnit);
        _gridInteractor.UnselectCells();
        _gridInteractor.SelectUnit(unit);
        HighlightAvailableMoves(_gridInteractor.AvailableMoves, unit.CurrentCell.ColorMovementCell);
    }

    private void HandleUnitAttack(Unit selectedUnit, Unit targetUnit)
    {
        if (selectedUnit.Status != UnitStatus.Selected)
        {
            return;
        }

        if (targetUnit.Type == UnitType.Enemy && selectedUnit.CanAttack(targetUnit))
        {
            selectedUnit.Attack(targetUnit);

            if (targetUnit.Health <= 0)
            {
                _gridInteractor.RemoveUnit(targetUnit);
            }
            else
            {
                _gridInteractor.UpdateUnit(targetUnit);
            }

            _gridInteractor.UnselectUnit(selectedUnit);

            // Update available moves after attack
            var availableMoves = _gridInteractor.GetAvailableMoves(selectedUnit.CurrentCell, selectedUnit.Type, 1);
            _gridInteractor.HighlightAvailableMoves(availableMoves, selectedUnit.CurrentCell.ColorMovementCell);
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

        var availableMoves = _gridInteractor.GetAvailableMoves(cell, selectedUnit.Type, 1);

        if (!availableMoves.Contains(cell))
        {
            return;
        }

        var path = _gridInteractor.FindPathToTarget(selectedUnit.CurrentCell, cell);
        if (path.Count == 0)
        {
            return;
        }

        _gridInteractor.UnselectUnit(selectedUnit);
        _gridInteractor.MoveUnitAlongPath(selectedUnit, path);

        selectedUnit.CurrentCell.ChangeColor(selectedUnit.CurrentCell.ColorStandardCell);
        selectedUnit.CurrentCell.UnitOn = null;

        cell.UnitOn = selectedUnit;
        cell.ChangeColor(cell.ColorStandardCell);

        _gridInteractor.SelectUnit(selectedUnit);
    }


    private bool CanMoveToCell(Cell cell, Unit unit)
    {
        // Проверяем, есть ли юнит в списке доступных для перемещения ячеек
        if (!_gridInteractor.AvailableMoves.Contains(cell))
        {
            return false;
        }

        // Проверяем, есть ли на ячейке другой юнит
        if (cell.UnitOn != null)
        {
            return false;
        }

        // Проверяем, достаточно ли очков хода у юнита для перемещения на эту ячейку
        if (unit.MovementPoints < cell.MovementCost)
        {
            return false;
        }

        return true;
    }





}

