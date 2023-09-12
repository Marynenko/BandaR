using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Selector : MonoBehaviour
{
    private Grid _grid;
    private Interactor _interactor;
    private List<Tile> _availableMoves;

    public delegate void UnitSelectedEventHandler(Unit unit, Selector selector);
    public static event UnitSelectedEventHandler OnUnitSelected;

    public Unit SelectedUnit { get; set; }    

    private void OnEnable()
    {
        _grid = GetComponentInParent<Grid>();
        _interactor = GetComponent<Interactor>();
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

        if (unit == null) return;
        //unit.Status = UnitStatus.Unselected;
        SelectedUnit = null;
        _interactor.SelectedUnit = null;
        //unit.OccupiedTile.ClearUnit();
    }

    public void SelectTileToMoveFrom(Tile tile, UnitType unitType, bool clearSelectedTiles = false)
    {
        if (clearSelectedTiles)
        {
            ChangeAvailableTilesColor();
        }

        _availableMoves = GetAvailableMoves(tile, SelectedUnit.MovementPoints);

        //if (unitType == UnitType.Player || unitType == UnitType.Enemy)
        //    _availableMoves = GetAvailableMoves(tile, SelectedUnit.MovementPoints); // ИСПРАВИТЬ
        //else
        //    return;

        //if (selectedUnitColor.HasValue)
        //    SelectedUnit.OccupiedTile.ChangeColor(selectedUnitColor.Value);
        switch (unitType)
        {
            case UnitType.Player:
                SelectedUnit.OccupiedTile.ChangeColor(TileState.OccupiedByPlayer);
                break;
            case UnitType.Enemy:
                SelectedUnit.OccupiedTile.ChangeColor(TileState.OccupiedByEnemy);
                break;
        }

        var availableMovesCopy = _availableMoves.GetRange(0, _availableMoves.Count);
        availableMovesCopy.Remove(tile);
        foreach (var tileToMove in availableMovesCopy)
            tileToMove.ChangeColor(TileState.Movement);
    }

    public void ChangeAvailableTilesColor()
    {
        var tiles = _grid.Generator.Tiles;
        foreach (var tile in tiles)
            tile.ChangeColor(TileState.Standard);
    }

    public List<Tile> GetAvailableMoves(Tile tile, int maxMoves)
    {
        var visitedTiles = new HashSet<Tile>();
        var availableMoves = new List<Tile>();

        var queue = new Queue<(Tile, int)>();
        queue.Enqueue((tile, maxMoves));

        while (queue.Count > 0)
        {
            var (currentTile, remainingMoves) = queue.Dequeue();

            visitedTiles.Add(currentTile);
            availableMoves.Add(currentTile);

            if (remainingMoves > 1)
                foreach (var neighbour in _grid.Generator.Interactor.PathConstructor.GetNearbyTiles(currentTile, _grid))
                    if (!(visitedTiles.Contains(neighbour) && neighbour.IsOccupied()))
                    {
                        // Проверяем, хватает ли очков передвижения, чтобы дойти до соседней клетки
                        var cost = neighbour.MovementCost;
                        if (cost <= remainingMoves)
                            queue.Enqueue((neighbour, remainingMoves - cost));
                    }
        }

        return availableMoves;
    }
}
