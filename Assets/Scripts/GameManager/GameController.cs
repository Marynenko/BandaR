using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour, IGameController
{
    [SerializeField] private Grid _grid;
    [SerializeField] private GridInteractor _interactor;
    [SerializeField] private GridGenerator _generator;

    private Unit lastSelectedUnit;
    private Cell lastSelectedCell;

    public void UnhighlightUnavailableCells()
    {
        // Идем по всем клеткам на игровом поле
        foreach (var cell in _grid.Cells)
        {
            //// Если клетка подсвечена и больше не доступна для хода, снимаем подсветку
            if (cell.CurrentState == State.Reachable && cell != _interactor.SelectedUnit.CurrentCell)
                UnselectCell(cell);
        }
    }

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
            return;

        if (cell == selectedUnit.CurrentCell)
            return;

        // Если есть последний выбранный юнит и ячейка, восстанавливаем их состояние
        if (lastSelectedUnit != null && lastSelectedCell != null)
        {
            SelectUnit(lastSelectedUnit);
            SelectCell(lastSelectedCell);
        }

        UnselectCell(selectedUnit.CurrentCell);

        if (!IsCellAvailableForMove(selectedUnit, cell, out List<Cell> Path))
            return;

        //var path = _interactor.FindPathToTarget(selectedUnit.CurrentCell, cell, out Path);

        if (Path.Count == 0)
            return;

        MoveUnit(selectedUnit, Path);

        SelectUnit(selectedUnit);

        //CheckAdjacentUnits(selectedUnit, _grid.AllUnits, selectedUnit.Team.EnemyTeam);
        CheckAdjacentUnits(selectedUnit, _grid.AllUnits);

        // Дополнение: проверяем, не находится ли выбранный юнит рядом с вражескими юнитами
        if (IsUnitAdjacentToEnemy(selectedUnit, _grid.AllUnits))
        {
            // Если юнит находится рядом с вражескими юнитами, выделяем его ячейку красным цветом
            SelectCell(selectedUnit.CurrentCell); // Red
        }

        // Сохраняем текущий юнит и ячейку как последние выбранные
        lastSelectedUnit = selectedUnit;
        lastSelectedCell = selectedUnit.CurrentCell;

        UnselectUnit(selectedUnit);
        UnselectCell(selectedUnit.CurrentCell);
    }



    private bool IsPlayerUnitSelected(Unit unit)
    => unit != null && unit.Type == UnitType.Player && unit.Status == UnitStatus.Selected;

    // Метод проверяет, доступна ли ячейка для перемещения выбранного юнита
    private bool IsCellAvailableForMove(Unit unit, Cell cell, out List<Cell> Path)
    {
        Path = new List<Cell>();
        return cell.IsWalkable() && unit.MovementPoints >= _interactor.PathConstructor.FindPathToTarget(unit.CurrentCell, cell, out Path, _grid).Count;
    }    
    

    private void UnselectUnit(Unit unit) => _interactor?.UnselectUnit(unit);
    private void UnselectCell(Cell cell) => cell.UnselectCell();
    private void MoveUnit(Unit unit, List<Cell> path) => _interactor.MoveUnitAlongPath(unit, path);
    private void SelectCell(Cell cell) => cell.SelectCell();
    private void SelectUnit(Unit unit) => _interactor?.SelectUnit(unit);

    #region Вторая ветка проверкми клеток
    private void CheckAdjacentUnits(Unit unit, List<IUnit> units) // Сюда в Neighbours
    {
        foreach (var neighbourCell in unit.CurrentCell.Neighbours)
        {
            var neighborUnit = units.OfType<Unit>().FirstOrDefault(u => u.CurrentCell == neighbourCell);
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
            if (unit2.CurrentCell == neighborCell)
                return true;
        return false;
    }

    private void AttackEnemies(Unit unit, List<Unit> enemies)
    {
        foreach (var enemy in enemies)
        {
            unit.Attack(enemy);

            if (enemy.Health <= 0)
                _grid.RemoveUnit(enemy);
        }
    }


    
    // Метод проверяет, находится ли юнит рядом с юнитами указанной команды
    private bool IsUnitAdjacentToEnemy(Unit unit, List<IUnit> units)
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