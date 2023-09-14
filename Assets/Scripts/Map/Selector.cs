using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    private TilesGrid _grid;
    private Interactor _interactor;
    private List<Tile> _availableMoves;

    // public delegate void UnitSelectedEventHandler(Unit unit, Selector selector);
    public delegate void UnitSelectedEventHandler(Unit unit);

    public static event UnitSelectedEventHandler OnUnitSelected;
    public static event UnitSelectedEventHandler OnUnitDeselected;


    public Unit SelectedUnit { get; set; }

    private void OnEnable()
    {
        _grid = GetComponentInParent<TilesGrid>();
        _interactor = GetComponent<Interactor>();
        OnUnitSelected += _interactor.HandleUnitSelected;
        OnUnitDeselected += _interactor.HandleUnitDeselection;
    }

    private void OnDestroy()
    {
        OnUnitSelected -= _interactor.HandleUnitSelected;
        OnUnitDeselected -= _interactor.HandleUnitDeselection;
    }

    public void SelectUnit(Unit unit)
    {
        if (SelectedUnit != null)
        {
            SelectedUnit.OccupiedTile.UnselectTile();
            UnselectUnit(SelectedUnit);
        }

        ChangeAvailableTilesColor();
        SelectTileToMoveFrom(SelectedUnit.OccupiedTile, SelectedUnit.Type);
        OnUnitSelected?.Invoke(unit);

    }
    
    public void UnselectUnit(Unit unit)
    {
        // Unselect the current unit and reset tile availability
        ChangeAvailableTilesColor();
        OnUnitDeselected?.Invoke(unit);
        
        //unit.OccupiedTile.ClearUnit();
    }

    public void SelectTileToMoveFrom(Tile tile, UnitType unitType)
    {
        _availableMoves = GetAvailableMoves(tile, SelectedUnit.MovementPoints);
        
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
        var tiles = _grid.Tiles;
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
                foreach (var neighbour in _grid.Interactor.PathConstructor.GetNearbyTiles(currentTile, _grid))
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
