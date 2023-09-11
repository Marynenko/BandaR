﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GridGenerator _generator;

    private Unit _lastSelectedUnit;
    private Tile _lastSelectedTile;

    public Grid Grid;
    public Selector Selector;
    public Interactor Interactor;

    public void HandleUnitClick(Unit unit)
    {
        var selectedUnit = Interactor.SelectedUnit;

        if (selectedUnit != null) return;
        if (unit.Type == UnitType.Player && unit.Status == UnitStatus.Available)
            Selector.SelectUnit(unit);

        else if (selectedUnit.Equals(unit))
            return;
        else switch (unit.Type)
        {
            case UnitType.Player:
                Selector.SelectUnit(unit);
                break;
            case UnitType.Enemy when selectedUnit.Type == UnitType.Player:
                HandleUnitAttack(selectedUnit, unit);
                break;
            default:
                Interactor.HandleUnitDeselection(selectedUnit, unit, Selector);
                break;
        }
    }

    private void HandleUnitAttack(Unit selectedUnit, Unit targetUnit)
    {
        if (selectedUnit.Status != UnitStatus.Moved) // Изменил из Selected на Moved
            return;
        if (selectedUnit.Status == UnitStatus.Unavailable)
            return;
        if (targetUnit.Type == UnitType.Enemy && selectedUnit.CanAttack(targetUnit))
        {
            selectedUnit.Attack(targetUnit);

            if (targetUnit.Stats.Health <= 0)
                Grid.RemoveUnit(targetUnit);
            else
                Interactor.UpdateUnit(targetUnit);

            selectedUnit.OccupiedTile.DeselectTile();
            Selector.UnselectUnit(selectedUnit);


            if (selectedUnit.IsAlive())
            {
                // Update available moves after attack
                var availableMoves = Selector.GetAvailableMoves(selectedUnit.OccupiedTile, selectedUnit.MovementPoints);
                Interactor.HighlightAvailableMoves(availableMoves, selectedUnit.OccupiedTile.ColorMovementTile, Selector);
            }
        }
    }

    public void HandleTileClick(Tile tile)
    {
        var selectedUnit = Selector.SelectedUnit;

        if (!IsClickValid(selectedUnit, tile))
            return;

        // Если есть последний выбранный юнит и ячейка, восстанавливаем их состояние
        HandleLastSelectedUnit();

        if (!IsTileInPath(selectedUnit, tile, out var path)) 
            return;

        HandleTileMovement(selectedUnit, path);

        _lastSelectedUnit = selectedUnit;
        _lastSelectedTile = selectedUnit.OccupiedTile;
        selectedUnit.Status = UnitStatus.Moved;
        UnselectUnit(selectedUnit);
    }

    private bool IsClickValid<T, TU>(T selectedUnit, TU tile)
        where T : Unit
        where TU : Tile 
        =>
        IsPlayerUnitAvailable(selectedUnit) &&
        tile != selectedUnit.OccupiedTile &&
        selectedUnit.Status != UnitStatus.Moved;

    private void HandleLastSelectedUnit()
    {
        if (_lastSelectedUnit != null && _lastSelectedTile != null)
        {
            SelectUnit(_lastSelectedUnit);
            SelectTile(_lastSelectedTile);
        }
    }

    private void HandleTileMovement(Unit selectedUnit, List<Tile> path)
    {
        UnselectTile(selectedUnit.OccupiedTile); // Убрать выделение из клетки на которой игрок.

        if (path.Count == 0)
            return;

        MoveUnit(selectedUnit, path);
        HandleAdjacentUnits(selectedUnit, Grid.Generator.AllUnits);

        if (IsUnitAdjacentToEnemy(selectedUnit, Grid.Generator.AllUnits))
        {
            SelectTile(selectedUnit.OccupiedTile);
        }
    }

    private void HandleAdjacentUnits(Unit selectedUnit, List<Unit> allUnits)
    {
        foreach (var neighborTile in selectedUnit.OccupiedTile.Neighbors)
        {
            UpdateNeighborUnits(selectedUnit , neighborTile, allUnits);
            CheckAdjacentEnemies(selectedUnit, allUnits);
        }
    }

    public void UpdateNeighborUnits(Unit unit, Tile neighborTile, List<Unit> units)
    {
        var neighborUnit = units.FirstOrDefault(u => u.OccupiedTile == neighborTile);

        if (neighborUnit != null && neighborUnit.Type == unit.Type)
        {
            neighborUnit.OnUnitMoved(unit);
        }
    }


    private void CheckAdjacentEnemies(Unit unit, List<Unit> units)
    {
        if (IsUnitAdjacentToEnemy(unit, units))
        {
            var adjacentEnemies = units.OfType<Unit>()
                .Where(u => u.Type != unit.Type && IsUnitAdjacentTo(u, unit));
            AttackEnemies(unit, adjacentEnemies.ToList());
        }
    }

    // Метод проверяет, находится ли юнит рядом с юнитами указанной команды
    private bool IsUnitAdjacentToEnemy(Unit unit, List<Unit> units)
    {
        foreach (var neighborTile in unit.OccupiedTile.Neighbors)
        {
            var neighborUnit = units.OfType<Unit>()
                .FirstOrDefault(u => u.OccupiedTile == neighborTile && u.Type == UnitType.Enemy);
            if (neighborUnit != null)
                return true;
        }

        return false;
    }

    private bool IsUnitAdjacentTo(Unit unit1, Unit unit2)
    {
        foreach (var neighborTile in unit1.OccupiedTile.Neighbors)
        {
            if (neighborTile == unit2.OccupiedTile) return true;
            if (neighborTile.State == TileState.OccupiedByPlayer) return true;
        }

        return false;
    }

    private bool IsPlayerUnitAvailable(Unit unit) =>
        unit != null && unit.Type == UnitType.Player && unit.Status == UnitStatus.Available;

    // Метод проверяет, доступна ли ячейка для перемещения выбранного юнита
    private bool IsTileInPath(Unit unit, Tile tile, out List<Tile> path)
    {
        path = Interactor.PathConstructor.FindPathToTarget(unit.OccupiedTile, tile, out _, Grid);        
        return tile.UnitOn == false && unit.MovementPoints >= path.Count;
    }

    private void UnselectUnit(Unit unit) => Selector?.UnselectUnit(unit);
    private void UnselectTile(Tile tile) => tile.DeselectTile();
    private void MoveUnit(Unit unit, List<Tile> path) => MoveUnitAlongPath(unit, path);
    public void MoveUnitAlongPath(Unit unit, List<Tile> path)
    {
        // Двигаем юнита поочередно на каждую ячейку из списка
        foreach (var tile in path)
            if (unit.CanMoveToTile(tile, out float distanceSqrt))
                unit.MoveToTile(tile, distanceSqrt); //
    }
    private void SelectTile(Tile tile) => tile.SelectTile();
    private void SelectUnit(Unit unit) => Selector?.SelectUnit(unit);

    #region Ветка проверок клеток НЕ РАБОТАЕТ
    private void AttackEnemies(Unit unit, List<Unit> enemies)
    {
        foreach (var enemy in enemies)
        {
            unit.Attack(enemy);

            if (enemy.Stats.Health <= 0)
                Grid.RemoveUnit(enemy);
        }
    }
    #endregion
}