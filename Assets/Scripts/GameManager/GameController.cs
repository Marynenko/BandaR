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
    private Tile _lastSelectedTile; // TODO Переименовать в StartTile и использовать

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
        if (unit.Stats.Type == UnitType.Player && unit.Status == UnitStatus.Available)
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
        if (targetUnit.Stats.Type == UnitType.Enemy && selectedUnit.CanAttack(targetUnit))
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
                    selectedUnit.Stats.MovementPoints));
                GridUI.Instance.HighlightAvailableMoves(availableMoves, TileState.Movement);
            }
        }
    }

    public void HandleTileClick(Tile tile)
    {
        var selectedUnit = Selector.SelectedUnit;

        if (PathIsFounded == false)
        {
            Path = FindPath(selectedUnit, tile);
            SetUnitTarget(selectedUnit, tile);
            PathIsFounded = true;
        }

        // if (Path.Count != 0)
        HandleTileMovement(selectedUnit);

        if (!selectedUnit.UnitIsMoving)
        {
            // _lastSelectedUnit = selectedUnit;
            // _lastSelectedTile = selectedUnit.OccupiedTile;
            PathIsFounded = false;
        }
    }

    private void SetUnitTarget(Unit selectedUnit, Tile tile)
    {
        var units = Grid.AllUnits;
        foreach (var unit in units)
        {
            if (unit == selectedUnit)
                unit.Target = tile;
        }
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

        if (selectedUnit.UnitIsMoving) return;
        if (Path.Count <= 1) return;
        if (selectedUnit.CanMoveMore())
            Selector.SelectUnit(selectedUnit);
    }

    private void MoveUnitAlongPath(Unit unit)
    {
        Path.RemoveAll(tile => !unit.AvailableMoves.Contains(tile));
        var nextTile = Path.FirstOrDefault();
        Vector2 unitV2 = new(unit.transform.position.x, unit.transform.position.z);
        Vector2 nextTileV2 = new(nextTile.transform.position.x, nextTile.transform.position.z);
        if (unitV2 == nextTileV2)
            Path.Remove(nextTile);

        if (unit.CanMoveToTile(nextTile, out var distanceSqrt) && nextTile != null)
        {
            unit.MoveToTile(nextTile, distanceSqrt);
        }

        unit.UnitIsMoving = NeedToMoveMore(unit, nextTile);
    }

    private bool NeedToMoveMore(Unit unit, Tile tile)
    {
        if (Path.Count == 0)
        {
            unit.Target = tile;
            return false;
        }
        if (unit.Stats.MovementPoints <= 1)
        {
            unit.Target = tile;
            return false;
        }

        return unit.OccupiedTile != Path.ElementAt(Path.Count - 1);
    }

    private void HandleAdjacentUnits(Unit selectedUnit, IReadOnlyCollection<Unit> allUnits)
    {
        foreach (var neighborTile in selectedUnit.OccupiedTile.Neighbors)
        {
            UpdateNeighborUnits(selectedUnit, neighborTile, allUnits);
        }
    }

    private void UpdateNeighborUnits(Unit unit, Tile neighborTile, IEnumerable<Unit> units)
    {
        var neighborUnit = units.FirstOrDefault(u => u.OccupiedTile == neighborTile);

        if (neighborUnit?.Stats.Type == unit.Stats.Type)
        {
            neighborUnit.OnUnitMoved(unit);
        }
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
        unit != null && unit.Stats.Type == UnitType.Player && unit.Status == UnitStatus.Available;
    
    private List<Tile> FindPath(Unit unit, Tile tile)
    {
        return Selector.PathConstructor.FindPathToTarget(unit, tile, out _);
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