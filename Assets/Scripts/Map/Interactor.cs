using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    private TilesGrid _grid;
    private List<Tile> _availableMoves;

    public PathConstructor PathConstructor;
    public Unit SelectedUnit { get; private set; }
    public List<Tile> AvailableMoves => _availableMoves.AsReadOnly().ToList();


    public void OnEnable()
    {
        //GameController.OnUnitAction += HandleUnitAction;
        _grid = GetComponentInParent<TilesGrid>();
        _availableMoves = new List<Tile>();
    }

    public void UpdateUnit(Unit unit)
    {
        // Обновить отображение юнита на игровом поле
        unit.UpdateVisuals();
    }

    public void HandleUnitSelected(Unit unit)
    {
        SelectedUnit = unit;
        SelectedUnit.OccupiedTile.Available = true; //был enemy -> SelectedUnit;
        SelectedUnit.Status = UnitStatus.Unavailable;
        SelectedUnit.OccupiedTile.SelectTile();
        SelectedUnit.OccupiedTile.UnitOn = true;
    }

    public void HandleUnitDeselection(Unit selectedUnit)
    {
        SelectedUnit.OccupiedTile.UnselectTile();
        SelectedUnit = null;
        HighlightAvailableMoves(AvailableMoves, TileState.Movement);
    }

    public void HighlightAvailableMoves(IReadOnlyList<Tile> availableMoves, TileState state)
    {
        HighlightTile(availableMoves.First(), TileState.OccupiedByPlayer);
        availableMoves.Skip(1).ToList().ForEach(tile => HighlightTile(tile, state));
    }

    public void UnhighlightAllTiles()
    {
        // Идем по всем клеткам на игровом поле
        foreach (var tile in _grid.Tiles)
        {
            // Если клетка подсвечена и больше не доступна для хода, снимаем подсветку
            if (tile.State == TileState.Standard && tile != SelectedUnit.OccupiedTile)
                tile.UnselectTile();
        }
    }

    public void UnhighlightAvailableMoves(Tile currentTile)
    {
        // Идем по всем клеткам на игровом поле
        foreach (var tile in currentTile.Neighbors)
            tile.UnhighlightTile();
    }

    public void HighlightTile(Tile tile, TileState state)
    {
        tile.ChangeColor(state);
    }

    public List<Tile> GetAvailableTiles(Unit unit)
    {
        var availableTiles = new List<Tile>();

        foreach (var tile in _grid.Tiles)
            if (tile.IsAvailableForUnit(unit))
                availableTiles.Add(tile);
        return availableTiles;
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