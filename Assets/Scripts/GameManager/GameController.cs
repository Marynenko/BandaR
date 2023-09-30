﻿using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class GameController : MonoBehaviour
{
    #region Variables

    private Unit _lastSelectedUnit;
    private Tile _lastSelectedTile;

    public Grid Grid;
    public Selector Selector;

    // Определение делегата
    public delegate void SelectionUnitHandler(Unit unit);

    // Определение событий
    public event SelectionUnitHandler UnitSelected;
    public event SelectionUnitHandler UnitUnselected;

    private List<Tile> Path;
    private bool PathIsFounded;

    #endregion

    public void HandleUnitClick(Unit unit)
    {
        if (unit.Type == UnitType.Player && unit.Status == UnitStatus.Available)
        {
            UnitSelected?.Invoke(unit);
        }

        UIManager.Instance.MenuAction.HideMenu();
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
            // else
            //     Selector.UpdateUnit(targetUnit);

            // Selector.UnselectUnit(selectedUnit);
            UnitUnselected?.Invoke(selectedUnit);

            if (selectedUnit.IsAlive())
            {
                // Update available moves after attack
                var availableMoves = new HashSet<Tile>
                (Selector.PathConstructor.GetAvailableMoves(selectedUnit.OccupiedTile,
                    selectedUnit.MovementPoints));
                GridUI.Instance.HighlightAvailableMoves(availableMoves, TileState.Movement); // TODO надо не надо?
            }
        }
    }

    public void HandleTileClick(Tile tile)
    {
        var selectedUnit = Selector.SelectedUnit;

        // if (!IsClickValid(selectedUnit, tile))
        //     return;
        // // Если есть последний выбранный юнит и ячейка, восстанавливаем их состояние
        // if (Selector.SelectedUnit != null)
        //     HandleLastSelectedUnit();

        // if (!IsTileInPath(selectedUnit, tile, out var path))
        //     return;

        if (PathIsFounded == false)
        {
            Path = FindPath(selectedUnit, tile);
            PathIsFounded = true;
        }

        HandleTileMovement(selectedUnit);

        if (!selectedUnit.UnitIsMoving)
        {
            _lastSelectedUnit = selectedUnit;
            _lastSelectedTile = selectedUnit.OccupiedTile;
            selectedUnit.Status = UnitStatus.Moved;
            PathIsFounded = false;
        }
        // UnitMenu.Instance.MenuAction._blockPanel.SetActive(false);
    }

    private void HandleTileMovement(Unit selectedUnit)
    {
        // UnitMenu.Instance.MenuAction._blockPanel.SetActive(true);
        selectedUnit.OccupiedTile.UnselectTile();
        GridUI.Instance.HighlightTiles(selectedUnit.AvailableMoves, TileState.Standard);

        MoveUnitAlongPath(selectedUnit);
        if (selectedUnit.UnitIsMoving)
            return;
        HandleAdjacentUnits(selectedUnit, Grid.AllUnits);

        if (IsUnitAdjacentToEnemy(selectedUnit, Grid.AllUnits))
        {
            selectedUnit.OccupiedTile.SelectTile();
        }

        if (!selectedUnit.UnitIsMoving)
            Selector.MoveMore();
    }

    public void MoveUnitAlongPath(Unit unit)
    {
        unit.UnitIsMoving = NeedToMoveMore(unit);
        Path.RemoveAll(tile => !unit.AvailableMoves.Contains(tile));
        Tile nextTile = null;
        // Двигаем юнита поочередно на каждую ячейку из списка
        foreach (var tile in Path)
        {
            nextTile = tile;
            break;
        }

        Vector2 unitV2 = new(unit.transform.position.x, unit.transform.position.z);
        Vector2 nextTileV2 = new(nextTile.transform.position.x, nextTile.transform.position.z);
        if (unitV2 == nextTileV2)
            Path.Remove(nextTile);

        if (unit.CanMoveToTile(nextTile, out var distanceSqrt) && nextTile != null)
        {
            unit.MoveToTile(nextTile, distanceSqrt);
        }

        unit.UnitIsMoving = NeedToMoveMore(unit);
    }

    private bool NeedToMoveMore(Unit unit)
    {
        if (Path.Count == 0)
            return false;
        return unit.MovementPoints > 1;
    }

    private bool IsClickValid(Unit selectedUnit, Tile tile)
        =>
            IsPlayerUnitAvailable(selectedUnit) &&
            tile != selectedUnit.OccupiedTile &&
            selectedUnit.Status != UnitStatus.Moved;

    private void HandleAdjacentUnits(Unit selectedUnit, IReadOnlyCollection<Unit> allUnits)
    {
        foreach (var neighborTile in selectedUnit.OccupiedTile.Neighbors)
        {
            UpdateNeighborUnits(selectedUnit, neighborTile, allUnits);
            CheckAdjacentEnemies(selectedUnit, allUnits);
        }
    }

    private void UpdateNeighborUnits(Unit unit, Tile neighborTile, IEnumerable<Unit> units)
    {
        var neighborUnit = units.FirstOrDefault(u => u.OccupiedTile == neighborTile);

        if (neighborUnit?.Type == unit.Type)
        {
            neighborUnit.OnUnitMoved(unit);
        }
    }


    private void CheckAdjacentEnemies(Unit unit, IReadOnlyCollection<Unit> units)
    {
        if (IsUnitAdjacentToEnemy(unit, units))
        {
            var adjacentEnemies = units
                .Where(u => u.Type != unit.Type && IsUnitAdjacentTo(u, unit));
            AttackEnemies(unit, adjacentEnemies.ToList());
        }
    }

    // Метод проверяет, находится ли юнит рядом с юнитами указанной команды
    private bool IsUnitAdjacentToEnemy(Unit unit, IReadOnlyCollection<Unit> units)
    {
        return unit.OccupiedTile.Neighbors.Select(neighborTile => units.FirstOrDefault(u =>
            u.OccupiedTile == neighborTile && u.Type == UnitType.Enemy)).Any(neighborUnit => neighborUnit != null);
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
    // private bool IsTileInPath(Unit unit, Tile tile, out List<Tile> path)
    // {
    //     path = Selector.PathConstructor.FindPathToTarget(unit.OccupiedTile, tile, out _);
    //     return tile.UnitOn == false && unit.MovementPoints >= path.Count;
    // }

    private List<Tile> FindPath(Unit unit, Tile tile)
    {
        return Selector.PathConstructor.FindPathToTarget(unit.OccupiedTile, tile, out _);
    }

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