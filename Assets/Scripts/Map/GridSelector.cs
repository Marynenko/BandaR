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
            SelectedUnit.OccupiedTile.UnselectTile();
            UnselectUnit(SelectedUnit);
        }

        OnUnitSelected?.Invoke(unit, this);
    }

    public void UnselectUnit(Unit unit)
    {
        // Unselect the current unit and reset tile availability

        if (unit != null)
        {
            //unit.Status = UnitStatus.Unselected;
            unit = null;
            SelectedUnit = unit;
            _interactor.SelectedUnit = unit;
            //unit.OccupiedTile.ClearUnit();
        }
    }

    public void SelectTileToMoveFrom(Tile tile, UnitType unitType, bool clearSelectedTiles = false, Color? selectedUnitColor = null)
    {
        if (clearSelectedTiles)
        {
            ChangeAvailableTilesColor();
        }

        if (unitType == UnitType.Player || unitType == UnitType.Enemy)
            _availableMoves = GetAvailableMoves(tile, SelectedUnit.MovementPoints);
        else
            return;

        if (selectedUnitColor.HasValue)
            SelectedUnit.OccupiedTile.ChangeColor(selectedUnitColor.Value);
        else
        {
            if (unitType == UnitType.Player)
                SelectedUnit.OccupiedTile.ChangeColor(tile.ColorUnitOnTile);
            else if (unitType == UnitType.Enemy)
                SelectedUnit.OccupiedTile.ChangeColor(tile.ColorEnemyOnTile);
        }

        List<Tile> availableMovesCopy = _availableMoves.GetRange(0, _availableMoves.Count);
        availableMovesCopy.Remove(tile);
        foreach (var moveTile in availableMovesCopy)
            moveTile.ChangeColor(moveTile.ColorMovementTile);
    }

    public void ChangeAvailableTilesColor()
    {
        var Tiles = _grid.Tiles;
        foreach (var tile in Tiles)
            tile.ChangeColor(tile.ColorStandardTile);
    }

    public List<Tile> GetAvailableMoves(Tile tile, int maxMoves)
    {
        var visitedTiles = new HashSet<Tile>();
        var AvailableMoves = new List<Tile>();

        var queue = new Queue<(Tile, int)>();
        queue.Enqueue((tile, maxMoves));

        while (queue.Count > 0)
        {
            var (currentTile, remainingMoves) = queue.Dequeue();

            visitedTiles.Add(currentTile);
            AvailableMoves.Add(currentTile);

            if (remainingMoves > 1)
                foreach (var neighbour in _grid.Interactor.PathConstructor.GetNeighbourTiles(currentTile, _grid))
                    if (!(visitedTiles.Contains(neighbour) && neighbour.IsOccupied()))
                    {
                        // Проверяем, хватает ли очков передвижения, чтобы дойти до соседней клетки
                        var cost = neighbour.MovementCost;
                        if (cost <= remainingMoves)
                            queue.Enqueue((neighbour, remainingMoves - cost));
                    }
        }

        return AvailableMoves;
    }


}
