using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour, IGameController
{
    [SerializeField] private GridGenerator _generator;

    private Unit lastSelectedUnit;
    private Tile lastSelectedCell;

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

            if (targetUnit.Health <= 0)
                Grid.RemoveUnit(targetUnit);
            else
                Interactor.UpdateUnit(targetUnit);

            selectedUnit.CurrentCell.UnselectCell();
            Selector.UnselectUnit(selectedUnit);


            if (selectedUnit.IsAlive())
            {
                // Update available moves after attack
                var availableMoves = Selector.GetAvailableMoves(selectedUnit.CurrentCell, selectedUnit.MovementPoints);
                Interactor.HighlightAvailableMoves(availableMoves, selectedUnit.CurrentCell.ColorMovementCell, Selector);
            }
        }
    }

    public void HandleCellClick(Tile cell)
    {
        var selectedUnit = Selector.SelectedUnit;

        if (!IsPlayerUnitAvailable(selectedUnit))
            return;

        if (cell == selectedUnit.CurrentCell)
            return;

        if (selectedUnit.Status == UnitStatus.Moved) // Тут посмотреть
            return;

        // Если есть последний выбранный юнит и ячейка, восстанавливаем их состояние
        if (lastSelectedUnit != null && lastSelectedCell != null)
        {
            SelectUnit(lastSelectedUnit);
            SelectCell(lastSelectedCell);
        }

        if (!IsCellInPath(selectedUnit, cell, out List<Tile> Path)) return;

        UnselectCell(selectedUnit.CurrentCell); // Убрать выделение из клетки на которой игрок.


        //var path = Interactor.FindPathToTarget(selectedUnit.CurrentCell, cell, out Path);

        if (Path.Count == 0)
            return;

        MoveUnit(selectedUnit, Path);

        //CheckAdjacentUnits(selectedUnit, Grid.AllUnits, selectedUnit.Team.EnemyTeam);
        CheckAdjacentUnits(selectedUnit, Grid.AllUnits);

        // Дополнение: проверяем, не находится ли выбранный юнит рядом с вражескими юнитами
        if (IsUnitAdjacentToEnemy(selectedUnit, Grid.AllUnits))
        {
            // Если юнит находится рядом с вражескими юнитами, выделяем его ячейку красным цветом
            SelectCell(selectedUnit.CurrentCell); // Red
        }

        lastSelectedUnit = selectedUnit;
        lastSelectedCell = selectedUnit.CurrentCell;

        // Сохраняем текущий юнит и ячейку как последние выбранные
        selectedUnit.Status = UnitStatus.Moved;

        UnselectUnit(selectedUnit); // Можно убрать наверное
    }   

    private bool IsPlayerUnitAvailable(Unit unit)
    => unit != null && unit.Type == UnitType.Player && unit.Status == UnitStatus.Available;

    // Метод проверяет, доступна ли ячейка для перемещения выбранного юнита
    private bool IsCellInPath(Unit unit, Tile cell, out List<Tile> path)
    {
        path = Interactor.PathConstructor.FindPathToTarget(unit.CurrentCell, cell, out _, Grid);        
        return cell.UnitOn == false && unit.MovementPoints >= path.Count;
    }

    private void UnselectUnit(Unit unit) => Selector?.UnselectUnit(unit);
    private void UnselectCell(Tile cell) => cell.UnselectCell();
    private void MoveUnit(Unit unit, List<Tile> path) => MoveUnitAlongPath(unit, path);
    public void MoveUnitAlongPath(Unit unit, List<Tile> path)
    {
        // Двигаем юнита поочередно на каждую ячейку из списка
        foreach (var cell in path)
            if (unit.CanMoveToCell(cell))
                unit.MoveToCell(cell); //
    }
    private void SelectCell(Tile cell) => cell.SelectCell();
    private void SelectUnit(Unit unit) => Selector?.SelectUnit(unit);

    #region Ветка проверок клеток НЕ РАБОТАЕТ
    private void CheckAdjacentUnits(Unit unit, List<Unit> units) // Сюда в Neighbours
    {
        foreach (var neighbourCell in unit.CurrentCell.Neighbours)
        {
            var neighborUnit = units.FirstOrDefault(u => u.CurrentCell == neighbourCell);
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
        foreach (var neighborCell in unit1.CurrentCell.Neighbours)
        {
            if (neighborCell == unit2.CurrentCell) return true;
            if (neighborCell.CurrentState == State.OccupiedByPlayer) return true;
        }

        return false;
    }

    private void AttackEnemies(Unit unit, List<Unit> enemies)
    {
        foreach (var enemy in enemies)
        {
            unit.Attack(enemy);

            if (enemy.Health <= 0)
                Grid.RemoveUnit(enemy);
        }
    }

    // Метод проверяет, находится ли юнит рядом с юнитами указанной команды
    private bool IsUnitAdjacentToEnemy(Unit unit, List<Unit> units)
    {
        foreach (var neighborCell in unit.CurrentCell.Neighbours)
        {
            var neighborUnit = units.OfType<Unit>().FirstOrDefault(u => u.CurrentCell == neighborCell && u.Type == UnitType.Enemy);
            if (neighborUnit != null)
                return true;
        }

        return false;
    }
    #endregion
}