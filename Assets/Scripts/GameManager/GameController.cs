﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameController : MonoBehaviour, IGameController
{
    [SerializeField] private Grid _grid;
    [SerializeField] private GridInteractor _interactor;
    [SerializeField] private GridGenerator _generator;

    public void HandleUnitClick(Unit unit)
    {
        var selectedUnit = _interactor.SelectedUnit;

        if (selectedUnit == null)
            if (unit.Type == UnitType.Player)
                _interactor.SelectUnit(unit);

            else if (selectedUnit.Equals(unit))
                return;
            else if (unit.Type == UnitType.Player)
                _interactor.SelectUnit(unit);
            else if (unit.Type == UnitType.Enemy && selectedUnit.Type == UnitType.Player)
                HandleUnitAttack(selectedUnit, unit);
            else
                _interactor.HandleUnitDeselection(selectedUnit, unit);
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
                _grid.RemoveUnit(targetUnit);
            }
            else
            {
                _interactor.UpdateUnit(targetUnit);
            }

            selectedUnit.CurrentCell.UnselectCell();
            _interactor.UnselectUnit(selectedUnit);


            // Update available moves after attack
            var availableMoves = _interactor.GetAvailableMoves(selectedUnit.CurrentCell, 1);
            _interactor.HighlightAvailableMoves(availableMoves, selectedUnit.CurrentCell.ColorMovementCell);
        }
    }

    public void HandleCellClick(Cell cell)
    {
        var selectedUnit = _interactor.SelectedUnit;

        if (!IsPlayerUnitSelected(selectedUnit))
        {
            return;
        }

        if (cell == selectedUnit.CurrentCell)
        {
            return;
        }

        _interactor.UnselectUnit(selectedUnit);
        selectedUnit.CurrentCell.UnselectCell();

        if (!IsCellAvailableForMove(selectedUnit, cell))
        {
            return;
        }

        var path = _interactor.FindPathToTarget(selectedUnit.CurrentCell, cell);
        if (path.Count == 0)
        {
            return;
        }

        MoveUnit(selectedUnit, path);
        SelectCell(selectedUnit.CurrentCell);

        _interactor.SelectUnit(selectedUnit);

        //CheckAdjacentUnits(selectedUnit, _grid.AllUnits, selectedUnit.Team.EnemyTeam);
        CheckAdjacentUnits(selectedUnit, _grid.AllUnits);

        // Дополнение: проверяем, не находится ли выбранный юнит рядом с вражескими юнитами
        if (IsUnitAdjacentToEnemy(selectedUnit, _grid.AllUnits))
        {
            // Если юнит находится рядом с вражескими юнитами, выделяем его ячейку красным цветом
            selectedUnit.CurrentCell.SelectCell(); // Red
        }
    }

    // Метод проверяет, находится ли юнит рядом с юнитами указанной команды
    private bool IsUnitAdjacentToEnemy(Unit unit, List<IUnit> units)
    {
        foreach (var neighborCell in unit.CurrentCell.Neighbors)
        {
            var neighborUnit = units.OfType<Unit>().FirstOrDefault(u => u.CurrentCell == neighborCell && u.Type == UnitType.Enemy);
            if (neighborUnit != null)
            {
                return true;
            }
        }

        return false;
    }


    // Метод проверяет, доступна ли ячейка для перемещения выбранного юнита
    private bool IsCellAvailableForMove(Unit unit, Cell cell)
    {
        return cell.IsWalkable() && unit.MovementPoints >= _interactor.FindPathToTarget(unit.CurrentCell, cell).Count;
    }


    private bool IsPlayerUnitSelected(Unit unit)
    {
        return unit != null && unit.Type == UnitType.Player && unit.Status == UnitStatus.Selected;
    }


    private void MoveUnit(Unit unit, List<Cell> path)
    {
        _interactor.MoveUnitAlongPath(unit, path);
    }

    private void SelectCell(Cell cell)
    {
        cell.SelectCell();
    }

    private void CheckAdjacentUnits(Unit unit, List<IUnit> units)
    {
        foreach (var neighborCell in unit.CurrentCell.Neighbors)
        {
            var neighborUnit = units.OfType<Unit>().FirstOrDefault(u => u.CurrentCell == neighborCell);
            if (neighborUnit != null && neighborUnit.Type == unit.Type)
            {
                neighborUnit.OnUnitMoved(unit);
            }

            // Дополнение: проверяем, не находится ли выбранный юнит рядом с вражескими юнитами
            if (IsUnitAdjacentToEnemy(unit, units))
            {
                // Находим всех вражеских юнитов, которые находятся рядом с выбранным юнитом
                var adjacentEnemies = units.OfType<Unit>().Where(u => u.Type != unit.Type && IsUnitAdjacentTo(u, unit));

                // Атакуем каждого из вражеских юнитов, которые находятся рядом с выбранным юнитом
                AttackEnemies(unit, adjacentEnemies as List<Unit>);
            }
        }
    }

    private bool IsUnitAdjacentTo(Unit unit1, Unit unit2)
    {
        foreach (var neighborCell in unit1.CurrentCell.Neighbors)
        {
            if (unit2.CurrentCell == neighborCell)
            {
                return true;
            }
        }
        return false;
    }

    private void AttackEnemies(Unit unit, List<Unit> enemies)
    {
        foreach (var enemy in enemies)
        {
            unit.Attack(enemy);

            if (enemy.Health <= 0)
            {
                _grid.RemoveUnit(enemy);
            }
        }
    }

    private List<Unit> GetAdjacentEnemies(Unit unit, List<Unit> units)
    {
        var adjacentEnemies = new List<Unit>();

        foreach (var neighborCell in unit.CurrentCell.Neighbors)
        {
            var neighborUnit = units.FirstOrDefault(u => u.CurrentCell == neighborCell && u.Type == UnitType.Enemy);
            if (neighborUnit != null)
            {
                adjacentEnemies.Add(neighborUnit);
            }
        }

        return adjacentEnemies;
    }

    private bool IsEnemy(Unit unit, Unit selectedUnit)
    {
        return unit != selectedUnit && unit.ID != selectedUnit.ID;
    }

    private void AttackUnit(Unit attacker, Unit defender)
    {
        // Рассчитываем урон, наносимый атакующим юнитом за одну атаку
        //var damage = CalculateDamage(attacker, defender);
        //// Наносим урон защищающемуся юниту
        //defender.ReceiveDamage(damage);

        //// Если защищающийся юнит погиб, удаляем его из списка всех юнитов на поле
        //if (defender.IsDead)
        //{
        //    _grid.AllUnits.Remove(defender);
        //}
    }

    private void CalculateDamage(Unit attacker, Unit defender) // return int change in the future
    {
        // Рассчитываем урон, наносимый атакующим юнитом за одну атаку, используя формулу, зависящую от параметров юнитов
        //return (int)Mathf.Clamp(attacker.Attack - defender.Defense, 1, Mathf.Infinity);
    }

    private bool IsEnemyOnCell(Cell cell, Unit playerUnit)
    {
        return false;
        //    var enemyUnits = _grid.AllUnits.Except(new List<Unit> { playerUnit }).Where(u => IsEnemy(playerUnit));
        //    return enemyUnits.Any(u => u.CurrentCell == cell);
        //}
    }
}