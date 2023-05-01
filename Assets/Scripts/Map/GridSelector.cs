using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridSelector : MonoBehaviour
{
    private Grid _grid;
    private GridInteractor _interactor;
    private List<Cell> _availableMoves;

    public delegate void UnitSelectedEventHandler(Unit unit, GridSelector selector);
    public static event UnitSelectedEventHandler OnUnitSelected;

    public Unit SelectedUnit { get; set; }    

    private void OnEnable()
    {
        _grid = GetComponentInParent<Grid>();
        _interactor = GetComponent<GridInteractor>();
        OnUnitSelected += _interactor.HandleUnitSelected;
    }

    private void OnDestroy()
    {
        OnUnitSelected -= _interactor.HandleUnitSelected;
    }

    public void SelectUnit(Unit unit)
    {
        if (SelectedUnit != null)
        {
            SelectedUnit.CurrentCell.UnselectCell();
            UnselectUnit(SelectedUnit);
        }

        OnUnitSelected?.Invoke(unit, this);
    }

    public void UnselectUnit(Unit unit)
    {
        //unit.Status = UnitStatus.Unselected;
        SelectedUnit = null;
        _interactor.SelectedUnit = null;
        //unit.CurrentCell.ClearUnit();
    }


    public void ChangeAvailableCellsColor()
    {
        var Cells = _grid.Cells;
        foreach (var cell in Cells)
        {
            cell.ChangeColor(cell.ColorStandardCell);
        }
    }

    public void SelectCellToMove(Cell cell, UnitType unitType, bool clearSelectedCells = false, Color? selectedUnitColor = null)
    {
        if (clearSelectedCells)
        {
            ChangeAvailableCellsColor();
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
                foreach (var neighbour in _grid.Interactor.PathConstructor.GetNeighbourCells(currentCell, _grid))
                {
                    if (!visitedCells.Contains(neighbour) && neighbour.IsAwailable() && neighbour.UnitOn == false)
                    {
                        queue.Enqueue((neighbour, remainingMoves - 1));
                    }
                }
            }
        }

        AvailableMoves.RemoveAt(0);
        return AvailableMoves;
    }

}
