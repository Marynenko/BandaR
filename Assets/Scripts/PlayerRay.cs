﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerRay : MonoBehaviour
{
    [SerializeField] private GridInteractor _gridInteractor;

    public event Action<UnitActionType, Unit, Cell> OnUnitAction;

    private void Start()
    {
        _gridInteractor.InitializeActions();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                if (hit.collider.TryGetComponent(out Unit unit))
                {
                    HandleUnitClick(unit);
                }
                else if (hit.collider.TryGetComponent(out Cell cell))
                {
                    HandleCellClick(cell);
                }
            }
        }
    }

    private void HandleUnitClick(Unit unit)
    {
        var selectedUnit = _gridInteractor.SelectedUnit;

        if (selectedUnit == null)
        {
            if (unit.Type == UnitType.Player)
            {
                _gridInteractor.SelectUnit(unit);
                var currentCell = unit.CurrentCell;
                _gridInteractor.SelectCell(currentCell, unit.Type);
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
            _gridInteractor.SelectCell(currentCell, unit.Type);
        }
        else if (unit.Type == UnitType.Enemy)
        {
            if (_gridInteractor.AreUnitsAdjacent(selectedUnit, unit))
            {
                _gridInteractor.SelectUnit(unit);
                var currentCell = unit.CurrentCell;
                currentCell.ChangeColor(currentCell.CellEnemyOnColor);
                _gridInteractor.SelectCell(currentCell, unit.Type);
                currentCell.UnitOn = StatusUnitOn.Yes;
                unit.Status = UnitStatus.Selected;
            }
            else
            {
                var path = _gridInteractor.FindPathToTarget(unit.CurrentCell, selectedUnit.CurrentCell);
                if (path != null)
                {
                    _gridInteractor.MoveUnitAlongPath(unit, path);
                    unit.Status = UnitStatus.Selected;
                }
            }
        }
    }

    private void HandleCellClick(Cell cell)
    {
        var selectedUnit = _gridInteractor.SelectedUnit;
        if (selectedUnit == null)
        {
            Debug.LogWarning("No unit is currently selected.");
            return;
        }

        if (cell == selectedUnit.CurrentCell)
        {
            return;
        }

        var availableMoves = _gridInteractor.GetAvailableMoves(cell, selectedUnit.Type, 1);
        availableMoves.Remove(selectedUnit.CurrentCell);

        if (availableMoves.Contains(cell))
        {
            _gridInteractor.MoveUnit(selectedUnit, cell);
            selectedUnit.Status = UnitStatus.Unselected;
            selectedUnit.CurrentCell.UnitOn = StatusUnitOn.No;
            _gridInteractor.UnselectUnit(selectedUnit);
            OnUnitAction?.Invoke(UnitActionType.Move, selectedUnit, cell);
        }
        else
        {
            _gridInteractor.UnselectUnit(selectedUnit);
            selectedUnit.CurrentCell.ChangeColor(selectedUnit.CurrentCell.CellStandardColor);
            selectedUnit.CurrentCell.UnitOn = StatusUnitOn.No;
        }
    }

}

public enum UnitActionType
{
    Move
}
