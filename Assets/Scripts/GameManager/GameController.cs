using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class GameController : MonoBehaviour
{
    [SerializeField] private GridGenerator _generator;

    private Unit lastSelectedUnit;
    private Tile lastSelectedTile;

    public Grid Grid;
    public GridSelector Selector;
    public GridInteractor Interactor;

    public void HandleUnitClick(Unit unit)
    {
        var selectedUnit = Interactor.SelectedUnit;

        if (selectedUnit == null)
            if (unit.Type == UnitType.Player && unit.Status == UnitStatus.Available)
                Selector.SelectUnit(unit);

        else if (selectedUnit.Equals(unit))
            return;
        else if (unit.Type == UnitType.Player)
            Selector.SelectUnit(unit);
        else if (unit.Type == UnitType.Enemy && selectedUnit.Type == UnitType.Player)
            HandleUnitAttack(selectedUnit, unit);
        else
            Interactor.HandleUnitDeselection(selectedUnit, unit, Selector);
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
        HandleLastSelectedUnit(selectedUnit);

        if (!IsTileInPath(selectedUnit, tile, out List<Tile> Path)) 
            return;

        HandleTileMovement(selectedUnit, Path);

        lastSelectedUnit = selectedUnit;
        lastSelectedTile = selectedUnit.OccupiedTile;
        selectedUnit.Status = UnitStatus.Moved;
        UnselectUnit(selectedUnit);
    }

    private bool IsClickValid(Unit selectedUnit, Tile tile) =>
        IsPlayerUnitAvailable(selectedUnit) &&
        tile != selectedUnit.OccupiedTile &&
        selectedUnit.Status != UnitStatus.Moved;

    private void HandleLastSelectedUnit(Unit selectedUnit)
    {
        if (lastSelectedUnit != null && lastSelectedTile != null)
        {
            SelectUnit(lastSelectedUnit);
            SelectTile(lastSelectedTile);
        }
    }

    private void HandleTileMovement(Unit selectedUnit, List<Tile> path)
    {
        UnselectTile(selectedUnit.OccupiedTile); // Убрать выделение из клетки на которой игрок.

        if (path.Count == 0)
            return;

        MoveUnit(selectedUnit, path);
        HandleAdjacentUnits(selectedUnit, Grid.AllUnits);

        if (IsUnitAdjacentToEnemy(selectedUnit, Grid.AllUnits))
        {
            SelectTile(selectedUnit.OccupiedTile);
        }
    }

    private void HandleAdjacentUnits(Unit selectedUnit, List<Unit> allUnits)
    {
        foreach (var neighbourTile in selectedUnit.OccupiedTile.Neighbours)
        {
            UpdateNeighborUnits(selectedUnit, neighbourTile, allUnits);
            CheckAdjacentEnemies(selectedUnit, allUnits);
        }
    }

    private void UpdateNeighborUnits(Unit unit, Tile neighbourTile, List<Unit> units)
    {
        var neighborUnit = units.FirstOrDefault(u => u.OccupiedTile == neighbourTile);

        if (neighborUnit != null && neighborUnit.Type == unit.Type)
        {
            neighborUnit.OnUnitMoved(unit);
        }
    }

    private void CheckAdjacentEnemies(Unit unit, List<Unit> units)
    {
        if (IsUnitAdjacentToEnemy(unit, units))
        {
            var adjacentEnemies = units.OfType<Unit>().Where(u => u.Type != unit.Type && IsUnitAdjacentTo(u, unit));
            AttackEnemies(unit, adjacentEnemies.ToList());
        }
    }

    // Метод проверяет, находится ли юнит рядом с юнитами указанной команды
    private bool IsUnitAdjacentToEnemy(Unit unit, List<Unit> units)
    {
        foreach (var neighborTile in unit.OccupiedTile.Neighbours)
        {
            var neighborUnit = units.OfType<Unit>().FirstOrDefault(u => u.OccupiedTile == neighborTile && u.Type == UnitType.Enemy);
            if (neighborUnit != null)
                return true;
        }

        return false;
    }

    private bool IsUnitAdjacentTo(Unit unit1, Unit unit2)
    {
        foreach (var neighborTile in unit1.OccupiedTile.Neighbours)
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
            if (unit.CanMoveToTile(tile, out float distSq))
                unit.MoveToTile(tile, distSq); //
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