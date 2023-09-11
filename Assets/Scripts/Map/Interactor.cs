using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class Interactor : MonoBehaviour
{
    private Grid _grid;
    private List<Tile> _availableMoves;

    public PathConstructor PathConstructor;
    public Unit SelectedUnit { get; set; }
    public List<Tile> AvailableMoves => _availableMoves.AsReadOnly().ToList();


    public void OnEnable()
    {
        //GameController.OnUnitAction += HandleUnitAction;
        _grid = GetComponentInParent<Grid>();
        _availableMoves = new List<Tile>();
    }

    public void UpdateUnit(Unit unit)
    {
        // Обновить отображение юнита на игровом поле
        unit.UpdateVisuals();
    } 

    public void HandleUnitSelected(Unit unit, Selector selector)
    {
        if (unit.Type == UnitType.Player)
            HandlePlayerSelected(unit, selector);
        else if (unit.Type == UnitType.Enemy)
            HandleEnemySelected(unit, selector);
    }

    private void HandlePlayerSelected(Unit player, Selector selector)
    {
        UIManager.Instance.OpenMenuAction();

        SelectedUnit = player;
        selector.SelectedUnit = player;
        SelectedUnit.OccupiedTile.SelectTile(); // был player -> SelectedUnit;
        selector.SelectTileToMoveFrom(player.OccupiedTile, UnitType.Player);
        SelectedUnit.OccupiedTile.UnitOn = true; // тут или перед SelectTileToMoveFrom?
    }

    private void HandleEnemySelected(Unit enemy, Selector selector)
    {
        SelectedUnit = enemy;
        SelectedUnit.OccupiedTile.Available = true; //был enemy -> SelectedUnit;
        selector.SelectedUnit = enemy;
        SelectedUnit.Status = UnitStatus.Unavailable;
        SelectedUnit.OccupiedTile.SelectTile();
        selector.SelectTileToMoveFrom(SelectedUnit.OccupiedTile, UnitType.Enemy, true);
        SelectedUnit.OccupiedTile.UnitOn = true;
    }

    public void HandleUnitDeselection(Unit selectedUnit, Unit unit, Selector selector)
    {
        selector.UnselectUnit(selectedUnit);
        unit.OccupiedTile.DeselectTile();
        selector.ChangeAvailableTilesColor();
        selector.SelectUnit(unit);
        HighlightAvailableMoves(AvailableMoves, unit.OccupiedTile.ColorMovementTile, selector);
    }

    public void HighlightAvailableMoves(IReadOnlyList<Tile> availableMoves, Color color, Selector selector)
    {
        selector.ChangeAvailableTilesColor();
        HighlightTile(availableMoves.First(), availableMoves.First().ColorUnitOnTile);
        availableMoves.Skip(1).ToList().ForEach(tile => HighlightTile(tile, color));
    }

    public void UnhighlightAllTiles(Selector selector)
    {
        // Идем по всем клеткам на игровом поле
        foreach (var tile in _grid.Generator.Tiles)
        {
            // Если клетка подсвечена и больше не доступна для хода, снимаем подсветку
            if (tile.State == TileState.Standard && tile != SelectedUnit.OccupiedTile)
                tile.DeselectTile();
        }
    }

    public void UnhighlightAvailableMoves(Tile currentTile)
    {
        // Идем по всем клеткам на игровом поле
        foreach (var tile in currentTile.Neighbors)
            tile.UnhighlightTile();
    }

    public void HighlightTile(Tile tile, Color color)
    {
        tile.ChangeColor(color);
    }

    public List<Tile> GetAvailableTiles(Unit unit)
    {
        var availableTiles = new List<Tile>();

        foreach (var tile in _grid.Generator.Tiles)
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