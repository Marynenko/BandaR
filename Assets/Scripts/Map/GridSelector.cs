using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridSelector : MonoBehaviour
{
    private Grid _grid;
    private GridInteractor _interactor;
    private List<Tile> _availableMoves;

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
        // Unselect the current unit and reset cell availability

        if (unit != null)
        {
            //unit.Status = UnitStatus.Unselected;
            unit = null;
            SelectedUnit = unit;
            _interactor.SelectedUnit = unit;
            //unit.CurrentCell.ClearUnit();
        }
    }

    public void SelectCellToMoveFrom(Tile cell, UnitType unitType, bool clearSelectedCells = false, Color? selectedUnitColor = null)
    {
        if (clearSelectedCells)
        {
            ChangeAvailableCellsColor();
        }

        if (unitType == UnitType.Player || unitType == UnitType.Enemy)
            _availableMoves = GetAvailableMoves(cell, SelectedUnit.MovementPoints);
        else
            return;

        if (selectedUnitColor.HasValue)
            SelectedUnit.CurrentCell.ChangeColor(selectedUnitColor.Value);
        else
        {
            if (unitType == UnitType.Player)
                SelectedUnit.CurrentCell.ChangeColor(cell.ColorUnitOnCell);
            else if (unitType == UnitType.Enemy)
                SelectedUnit.CurrentCell.ChangeColor(cell.ColorEnemyOnCell);
        }

        List<Tile> availableMovesCopy = _availableMoves.GetRange(0, _availableMoves.Count);
        availableMovesCopy.Remove(cell);
        foreach (var moveCell in availableMovesCopy)
            moveCell.ChangeColor(moveCell.ColorMovementCell);
    }

    public void ChangeAvailableCellsColor()
    {
        var Cells = _grid.Cells;
        foreach (var cell in Cells)
            cell.ChangeColor(cell.ColorStandardCell);
    }

    public List<Tile> GetAvailableMoves(Tile cell, int maxMoves)
    {
        var visitedCells = new HashSet<Tile>();
        var AvailableMoves = new List<Tile>();

        var queue = new Queue<(Tile, int)>();
        queue.Enqueue((cell, maxMoves));

        while (queue.Count > 0)
        {
            var (currentCell, remainingMoves) = queue.Dequeue();

            visitedCells.Add(currentCell);
            AvailableMoves.Add(currentCell);

            if (remainingMoves > 1)
                foreach (var neighbour in _grid.Interactor.PathConstructor.GetNeighbourCells(currentCell, _grid))
                    if (!(visitedCells.Contains(neighbour) && neighbour.IsOccupied()))
                    {
                        // ���������, ������� �� ����� ������������, ����� ����� �� �������� ������
                        var cost = neighbour.MovementCost;
                        if (cost <= remainingMoves)
                            queue.Enqueue((neighbour, remainingMoves - cost));
                    }
        }

        return AvailableMoves;
    }


}
