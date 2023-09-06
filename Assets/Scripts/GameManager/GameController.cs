using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour, IGameController
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

            selectedUnit.OccupiedTile.UnselectTile();
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

        if (!IsPlayerUnitAvailable(selectedUnit))
            return;

        if (tile == selectedUnit.OccupiedTile)
            return;

        if (selectedUnit.Status == UnitStatus.Moved) // Тут посмотреть
            return;

        // Если есть последний выбранный юнит и ячейка, восстанавливаем их состояние
        if (lastSelectedUnit != null && lastSelectedTile != null)
        {
            SelectUnit(lastSelectedUnit);
            SelectTile(lastSelectedTile);
        }

        if (!IsTileInPath(selectedUnit, tile, out List<Tile> Path)) return;

        UnselectTile(selectedUnit.OccupiedTile); // Убрать выделение из клетки на которой игрок.


        //var path = Interactor.FindPathToTarget(selectedUnit.OccupiedTile, tile, out Path);

        if (Path.Count == 0)
            return;

        MoveUnit(selectedUnit, Path);

        //CheckAdjacentUnits(selectedUnit, Grid.AllUnits, selectedUnit.Team.EnemyTeam);
        CheckAdjacentUnits(selectedUnit, Grid.AllUnits);

        // Дополнение: проверяем, не находится ли выбранный юнит рядом с вражескими юнитами
        if (IsUnitAdjacentToEnemy(selectedUnit, Grid.AllUnits))
        {
            // Если юнит находится рядом с вражескими юнитами, выделяем его ячейку красным цветом
            SelectTile(selectedUnit.OccupiedTile); // Red
        }

        lastSelectedUnit = selectedUnit;
        lastSelectedTile = selectedUnit.OccupiedTile;

        // Сохраняем текущий юнит и ячейку как последние выбранные
        selectedUnit.Status = UnitStatus.Moved;

        UnselectUnit(selectedUnit); // Можно убрать наверное
    }   

    private bool IsPlayerUnitAvailable(Unit unit)
    => unit != null && unit.Type == UnitType.Player && unit.Status == UnitStatus.Available;

    // Метод проверяет, доступна ли ячейка для перемещения выбранного юнита
    private bool IsTileInPath(Unit unit, Tile tile, out List<Tile> path)
    {
        path = Interactor.PathConstructor.FindPathToTarget(unit.OccupiedTile, tile, out _, Grid);        
        return tile.UnitOn == false && unit.MovementPoints >= path.Count;
    }

    private void UnselectUnit(Unit unit) => Selector?.UnselectUnit(unit);
    private void UnselectTile(Tile tile) => tile.UnselectTile();
    private void MoveUnit(Unit unit, List<Tile> path) => MoveUnitAlongPath(unit, path);
    public void MoveUnitAlongPath(Unit unit, List<Tile> path)
    {
        // Двигаем юнита поочередно на каждую ячейку из списка
        foreach (var tile in path)
            if (unit.CanMoveToTile(tile))
                unit.MoveToTile(tile); //
    }
    private void SelectTile(Tile tile) => tile.SelectTile();
    private void SelectUnit(Unit unit) => Selector?.SelectUnit(unit);

    #region Ветка проверок клеток НЕ РАБОТАЕТ
    private void CheckAdjacentUnits(Unit unit, List<Unit> units) // Сюда в Neighbours
    {
        foreach (var neighbourTile in unit.OccupiedTile.Neighbours)
        {
            var neighborUnit = units.FirstOrDefault(u => u.OccupiedTile == neighbourTile);
            if (neighborUnit != null && neighborUnit.Type == unit.Type)
                neighborUnit.OnUnitMoved(unit);

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
        foreach (var neighborTile in unit1.OccupiedTile.Neighbours)
        {
            if (neighborTile == unit2.OccupiedTile) return true;
            if (neighborTile.CurrentState == State.OccupiedByPlayer) return true;
        }

        return false;
    }

    private void AttackEnemies(Unit unit, List<Unit> enemies)
    {
        foreach (var enemy in enemies)
        {
            unit.Attack(enemy);

            if (enemy.Stats.Health <= 0)
                Grid.RemoveUnit(enemy);
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
    #endregion
}