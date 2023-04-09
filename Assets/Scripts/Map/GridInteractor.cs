using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;
using UnityEngine.UIElements;

public class GridInteractor : MonoBehaviour
{
    public delegate void UnitSelectedEventHandler(Unit unit, UnitType unitType);
    public static event UnitSelectedEventHandler OnUnitSelected;

    private Grid _grid;
    private List<Cell> _availableMoves;

    public PathConstructor PathConstructor;
    public IReadOnlyList<Cell> AvailableMoves => _availableMoves.AsReadOnly().ToList();
    public Unit SelectedUnit { get; set; }


    public void OnEnable()
    {
        //GameController.OnUnitAction += HandleUnitAction;
        _grid = GetComponentInParent<Grid>();
        _availableMoves = new List<Cell>();
        OnUnitSelected += HandleUnitSelected;
    }
    private void OnDestroy()
    {
        OnUnitSelected -= HandleUnitSelected;
    }    

    public void SelectUnit(Unit unit)
    {
        if (SelectedUnit != null)
        {
            SelectedUnit.CurrentCell.UnselectCell();
            UnselectUnit(SelectedUnit);
        }

        OnUnitSelected?.Invoke(unit, unit.Type);
    }


    public void UnselectUnit(Unit unit)
    {
        //unit.Status = UnitStatus.Unselected;
        SelectedUnit = null;
        unit.CurrentCell.ClearUnit();
    }

    public void UpdateUnit(Unit unit)
    {
        // Обновить отображение юнита на игровом поле
        unit.UpdateVisuals();
    } 

    private void HandleUnitSelected(Unit unit, UnitType unitType)
    {
        if (unitType == UnitType.Player)
        {
            HandlePlayerSelected(unit);
        }
        else if (unitType == UnitType.Enemy)
        {
            HandleEnemySelected(unit);
        }
    }

    private void HandlePlayerSelected(Unit player)
    {
        player.CurrentCell.SetUnit(player);
        SelectedUnit = player;
        player.Status = UnitStatus.Selected;
        player.CurrentCell.SelectCell();
        SelectCellToMove(player.CurrentCell, UnitType.Player, true);
        player.CurrentCell.CellStatus = UnitOn.Yes; // тут или перед SelectCellToMove?
    }

    private void HandleEnemySelected(Unit enemy)
    {
        enemy.CurrentCell.SetUnit(enemy);
        SelectedUnit = enemy;
        enemy.Status = UnitStatus.Selected;
        enemy.CurrentCell.SelectCell();
        SelectCellToMove(enemy.CurrentCell, UnitType.Enemy, true);
        enemy.CurrentCell.CellStatus= UnitOn.Yes;
    }

    private void HandleUnitAction(UnitActionType actionType, Unit unit, Cell cell)
    {
        if (actionType == UnitActionType.Move)
        {
            unit.MoveToCell(cell);
        }
    }

    public void HandleUnitDeselection(Unit selectedUnit, Unit unit)
    {
        UnselectUnit(selectedUnit);
        unit.CurrentCell.UnselectCell();
        UnselectCells();
        SelectUnit(unit);
        HighlightAvailableMoves(AvailableMoves, unit.CurrentCell.ColorMovementCell);
    }

    public void HighlightAvailableMoves(IReadOnlyList<Cell> availableMoves, Color color)
    {
        UnselectCells();
        HighlightCell(availableMoves.First(), availableMoves.First().ColorUnitOnCell);
        availableMoves.Skip(1).ToList().ForEach(cell => HighlightCell(cell, color));
    }

    public void UnselectCells()
    {
        var Cells = _grid.Cells;
        foreach (var cell in Cells)
        {
            cell.ChangeColor(cell.ColorStandardCell);
        }
    }

    public void HighlightCell(Cell cell, Color color)
    {
        cell.ChangeColor(color);
    }


    public void SelectCellToMove(Cell cell, UnitType unitType, bool clearSelectedCells = false, Color? selectedUnitColor = null)
    {
        if (clearSelectedCells)
        {
            UnselectCells();
        }

        List<Cell> availableMovesCopy;

        if (unitType == UnitType.Player || unitType == UnitType.Enemy)
        {
            _availableMoves = GetAvailableMoves(cell, 1);
            availableMovesCopy = _availableMoves.GetRange(0, _availableMoves.Count);
        }
        else
        {
            return;
        }

        if (selectedUnitColor.HasValue)
        {
            availableMovesCopy.ElementAt(0).ChangeColor(selectedUnitColor.Value);
        }
        else
        {
            if (unitType == UnitType.Player)
            {
                availableMovesCopy.ElementAt(0).ChangeColor(cell.ColorUnitOnCell);
            }
            else if (unitType == UnitType.Enemy)
            {
                availableMovesCopy.ElementAt(0).ChangeColor(cell.ColorEnemyOnCell);
            }
        }

        availableMovesCopy.Remove(cell);
        foreach (var moveCell in availableMovesCopy)
        {
            moveCell.ChangeColor(moveCell.ColorMovementCell);
        }
    }

    public List<Cell> GetAvailableMoves(Cell cell, int maxMoves)
    {
        var visitedCells = new HashSet<Cell>();
        var AvailableMoves = new List<Cell>();

        var queue = new Queue<(Cell, int)>();
        queue.Enqueue((cell, maxMoves));

        while (queue.Count > 0)
        {
            var (currentCell, remainingMoves) = queue.Dequeue();

            visitedCells.Add(currentCell);
            AvailableMoves.Add(currentCell);

            if (remainingMoves > 0)
            {
                foreach (var neighbour in PathConstructor.GetNeighbourCells(currentCell, _grid))
                {
                    if (!visitedCells.Contains(neighbour) && neighbour.IsWalkable() && neighbour.CellStatus == UnitOn.No)
                    {
                        queue.Enqueue((neighbour, remainingMoves - 1));
                    }
                }
            }
        }

        cell.Neighbours = AvailableMoves;
        return AvailableMoves;
    }



    public void MoveUnitAlongPath(Unit unit, List<Cell> path)
    {
        // Двигаем юнита поочередно на каждую ячейку из списка
        foreach (var cell in path)
        {
            unit.MoveToCell(cell); // изменен вызов метода
        }
    }
}

public class Direction
{
    public int XOffset { get; private set; }
    public int YOffset { get; private set; }

    public Direction(int xOffset, int yOffset)
    {
        XOffset = xOffset;
        YOffset = yOffset;
    }
}




