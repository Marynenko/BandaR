using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameController : MonoBehaviour, IGameController
{
    [SerializeField] private Grid _grid;
    [SerializeField] private GridInteractor _interactor;
    [SerializeField] private GridGenerator _generator;

    private Unit lastSelectedUnit;
    private Cell lastSelectedCell;

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
    }



    private bool IsPlayerUnitSelected(Unit unit)
    => unit != null && unit.Type == UnitType.Player && unit.Status == UnitStatus.Selected;

    // Метод проверяет, доступна ли ячейка для перемещения выбранного юнита
    private bool IsCellAvailableForMove(Unit unit, Cell cell, out List<Cell> Path)
    {
        Path = new List<Cell>();
        return cell.IsWalkable() && unit.MovementPoints >= _interactor.FindPathToTarget(unit.CurrentCell, cell, out Path).Count;
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

    #region Первая ветка Проверки клетки
    private bool IsAdjacentCellAvailableForMove(Unit unit, Cell cell)
    {
        if (!cell.IsWalkable()) // если клетка непроходима
            return false;

        // проверяем, что клетка находится рядом с текущей позицией юнита
        var adjacentCells = GetAdjacentCells(unit.CurrentCell);
        if (!adjacentCells.Contains(cell))
            return false;

        // проверяем, что юнит имеет достаточное количество очков передвижения
        return unit.MovementPoints >= 1; // в данном случае передвижение на одну клетку
    }

    public List<Cell> GetAdjacentCells(Cell cell)
    {
        var adjacentCells = new List<Cell>();

        // Проверяем соседние клетки по горизонтали и вертикали
        for (int row = cell.Row - 1; row <= cell.Row + 1; row++)
            for (int column = cell.Column - 1; column <= cell.Column + 1; column++)
            {
                // Пропускаем текущую клетку
                if (row == cell.Row && column == cell.Column)
                    continue;

                // Получаем клетку по ее координатам
                var adjacentCell = _grid.GetCell(row, column);

                // Проверяем, что клетка существует и находится рядом с текущей клеткой
                if (adjacentCell != null && IsCellAdjacent(cell, adjacentCell))
                    adjacentCells.Add(adjacentCell);
            }

        return adjacentCells;
    }

    public bool IsCellAdjacent(Cell cell1, Cell cell2)
    => Math.Abs(cell1.Row - cell2.Row) <= 1 && Math.Abs(cell1.Column - cell2.Column) <= 1;

    #endregion

    #region Info об проверках клеток
    /*
     * Оба варианта имеют свои преимущества и недостатки, и выбор зависит от контекста.

Методы CheckAdjacentUnits, IsUnitAdjacentTo, AttackEnemies и IsUnitAdjacentToEnemy являются частью более 
    крупной функции и отвечают за определение и атаку близлежащих вражеских юнитов. 
    Если вы хотите сохранить эту логику в одном месте, то лучше использовать эти методы. 
    К тому же, методы IsUnitAdjacentTo и IsUnitAdjacentToEnemy могут быть использованы в других местах вашего приложения, 
    где требуется определить, находятся ли два юнита рядом.

С другой стороны, если вам нужно просто проверить, находится ли определенная клетка рядом с текущей позицией юнита и 
    может ли юнит передвигаться на эту клетку, то вам нужен метод IsAdjacentCellAvailableForMove и его аналог GetAdjacentCells. 
    Эти методы лучше использовать в сценариях, где вы хотите проверить возможность движения или атаки, 
    без выполнения всех остальных действий, выполняемых в методах
    CheckAdjacentUnits, IsUnitAdjacentTo, AttackEnemies и IsUnitAdjacentToEnemy.

Какой метод использовать в конкретной ситуации, зависит от ваших потребностей и требований к производительности. 
    Если вам нужно проверить соседние клетки очень часто, например, в цикле анимации, 
    то использование метода IsAdjacentCellAvailableForMove и GetAdjacentCells может быть более эффективным, 
    так как эти методы выполняют меньше действий, чем CheckAdjacentUnits, IsUnitAdjacentTo, AttackEnemies и IsUnitAdjacentToEnemy. 
    
    Если же вы хотите сохранить логику определения и атаки вражеских юнитов в одном месте, 
    то лучше использовать методы CheckAdjacentUnits, IsUnitAdjacentTo, AttackEnemies и IsUnitAdjacentToEnemy.
     * */

    #endregion

    #region temporarly Trash
    private List<Unit> GetAdjacentEnemies(Unit unit, List<Unit> units)
    {
        var adjacentEnemies = new List<Unit>();

        foreach (var neighborCell in unit.CurrentCell.Neighbours)
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
    #endregion
}