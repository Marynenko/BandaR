using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class GameController : MonoBehaviour, IGameController
{
    private GridInteractor _gridInteractor;

    public static event Action<UnitActionType, Unit, Cell> OnUnitAction;

    private void Start()
    {
        var grid = FindObjectOfType<Grid>();
        _gridInteractor = grid.Interactor;
    }

    public void HandleUnitClick(Unit unit)
    {
        var selectedUnit = _gridInteractor.SelectedUnit;

        if (selectedUnit == null)
            if (unit.Type == UnitType.Player)
                _gridInteractor.SelectUnit(unit);

        else if (selectedUnit.Equals(unit))
            return;
        else if (unit.Type == UnitType.Player)
            _gridInteractor.SelectUnit(unit);
        else if (unit.Type == UnitType.Enemy && selectedUnit.Type == UnitType.Player)
            HandleUnitAttack(selectedUnit, unit);
        else
            _gridInteractor.HandleUnitDeselection(selectedUnit, unit);
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

        _gridInteractor.UnselectUnit(selectedUnit);
        var availableMoves = _gridInteractor.AvailableMoves;

        if (!availableMoves.Contains(cell))
        {
            return;
        }

        var path = _gridInteractor.FindPathToTarget(selectedUnit.CurrentCell, cell);
        if (path.Count == 0)
        {
            return;
        }

        _gridInteractor.MoveUnitAlongPath(selectedUnit, path);

        selectedUnit.CurrentCell.UnitOn = StatusUnitOn.No;
        Color unitColor = selectedUnit.Type == UnitType.Player ? selectedUnit.CurrentCell.ColorUnitOnCell : selectedUnit.CurrentCell.ColorEnemyOnCell;
        cell.ChangeColor(unitColor);
        cell.UnitOn = StatusUnitOn.Yes;

        _gridInteractor.SelectCell(cell, selectedUnit.Type);
        _gridInteractor.SelectUnit(selectedUnit);

        // проверяем соседство юнитов после каждого перемещения
        //if (AreUnitsAdjacent(selectedUnit, _gridInteractor.AllUnits))
        //{
        //    // начинаем бой или выполняем нужные действия
        //}
    }




    public bool AreUnitsAdjacent(Unit unit1, Unit unit2)
    {
        var distance = Vector3.Distance(unit1.transform.position, unit2.transform.position);
        return distance <= 1f; // или другое значение, в зависимости от размеров клетки и модели юнитов
    }
}