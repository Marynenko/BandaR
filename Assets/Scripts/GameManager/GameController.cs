using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class GameController : MonoBehaviour
{
    private Unit _lastSelectedUnit; // T - Unit
    private Tile _lastSelectedTile; // TU - Tile

    public Grid Grid;
    public Selector Selector;
    
    // Определение делегата
    public delegate void SelectionUnitHandler(Unit unit);
    
    // Определение событий
    public event SelectionUnitHandler UnitSelected;
    public event SelectionUnitHandler UnitUnselected;


    public void HandleUnitClick(Unit unit)    
    {
        var selectedUnit = Selector.SelectedUnit;

        // if (selectedUnit != null) return;
        if (unit.Type == UnitType.Player && unit.Status == UnitStatus.Available)
        {
            UnitSelected?.Invoke(unit);
        }
        // else if (selectedUnit?.Equals(unit) == true)
        //     return;

        // else switch (unit.Type)
        // {
        //     case UnitType.Player:
        //         Selector.SelectUnit(unit);
        //         break;
        //     case UnitType.Enemy when selectedUnit.Type == UnitType.Player:
        //         HandleUnitAttack(selectedUnit, unit);
        //         break;
        //     default:
        //         Selector.HandleUnitDeselection(selectedUnit);
        //         break;
        // }
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
                var availableMoves = Selector.PathConstructor.GetAvailableMoves(selectedUnit.OccupiedTile, selectedUnit.MovementPoints);
                GridUI.HighlightAvailableMoves(availableMoves, TileState.Movement); // TODO надо не надо?
            }
        }
    }

    public void HandleTileClick(Tile tile)
    {
        var selectedUnit = Selector.SelectedUnit;

         if (!IsClickValid(selectedUnit, tile))
            return;

        // // Если есть последний выбранный юнит и ячейка, восстанавливаем их состояние
        // if (Selector.SelectedUnit != null)
        //     HandleLastSelectedUnit();

        if (!IsTileInPath(selectedUnit, tile, out var path))
            return;

        HandleTileMovement(selectedUnit, path);

        _lastSelectedUnit = selectedUnit;
        _lastSelectedTile = selectedUnit.OccupiedTile;
        selectedUnit.Status = UnitStatus.Moved;
    }

    private bool IsClickValid(Unit selectedUnit, Tile tile)
        =>
        IsPlayerUnitAvailable(selectedUnit) &&
        tile != selectedUnit.OccupiedTile &&
        selectedUnit.Status != UnitStatus.Moved;

    // private void HandleLastSelectedUnit()
    // {
    //     if (_lastSelectedUnit ==  _lastSelectedTile) return;
    //     UnitSelected?.Invoke(_lastSelectedUnit);
    //     // TileSelected?.Ivoke(); TODO сделать в будущем TileSelected и TileUnselected
    //     // Selector?.SelectUnit(_lastSelectedUnit);
    //     _lastSelectedTile.SelectTile();
    // }

    private void HandleTileMovement(Unit selectedUnit, List<Tile> path)
    {
        selectedUnit.OccupiedTile.UnselectTile();
        
        if (path.Count == 0)
            return;

        MoveUnitAlongPath(selectedUnit, path);
        HandleAdjacentUnits(selectedUnit, Grid.AllUnits);

        if (IsUnitAdjacentToEnemy(selectedUnit, Grid.AllUnits))
        {
            selectedUnit.OccupiedTile.SelectTile();
        }
        
        Selector.UnitTurnIsOver();
    }

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
            u.OccupiedTile == neighborTile && u.Type == UnitType.Enemy)).
            Any(neighborUnit => neighborUnit != null);
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
        path = Selector.PathConstructor.FindPathToTarget(unit.OccupiedTile, tile, out _);
        return tile.UnitOn == false && unit.MovementPoints >= path.Count;
    }
    
    public void MoveUnitAlongPath(Unit unit, List<Tile> path)
    {
        // Двигаем юнита поочередно на каждую ячейку из списка
        foreach (var tile in path)
            if (unit.CanMoveToTile(tile, out float distanceSqrt))
                unit.MoveToTile(tile, distanceSqrt);
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