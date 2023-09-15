using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class Selector: MonoBehaviour
{
    private List<Tile> _availableMoves;
    private Grid _grid;
    private GameController _gameController;
    
    public PathConstructor PathConstructor;
    public List<Tile> AvailableMoves => _availableMoves.AsReadOnly().ToList();
    public Unit SelectedUnit { get; private set; }

    // public delegate void UnitSelectedEventHandler(Unit unit, Selector selector);

    private void OnEnable()
    {
        _grid = GetComponentInParent<Grid>();
        _availableMoves = new List<Tile>();
        _gameController = GetComponentInParent<GameController>();
        _gameController.UnitSelected += SelectUnit;
        _gameController.UnitUnselected += UnselectUnit;
    }

    private void OnDisable()
    {
        _gameController.UnitSelected -= SelectUnit;
        _gameController.UnitUnselected -= UnselectUnit;
    }

    public void SelectUnit(Unit unit)
    {
        // GridUI.HighlightAllTilesToStandard(); // TODO посмотреть стоит ли чистить цвета постоянно??
        GridUI.HighlightTile(unit.OccupiedTile, unit.OccupiedTile.State);
        // HighlightAvailableMoves(unit.OccupiedTile);
        _availableMoves = PathConstructor.GetAvailableMoves(unit.OccupiedTile, unit.MovementRange);
        GridUI.HighlightAvailableMoves(_availableMoves, TileState.Movement);
        
        SelectedUnit = unit;
        SelectedUnit.OccupiedTile.Available = true; //был enemy -> SelectedUnit;
        SelectedUnit.Status = UnitStatus.Unavailable;
        SelectedUnit.OccupiedTile.SelectTile();
        SelectedUnit.OccupiedTile.UnitOn = true;
    }
    
    public void UnselectUnit(Unit unit)
    {
        // Unselect the current unit and reset tile availability
        // HighlightAllTilesToStandard(); // TODO посмотреть стоит ли чистить цвета постоянно??
        GridUI.HighlightTile(unit.OccupiedTile, TileState.Standard);
        SelectedUnit.OccupiedTile.UnselectTile();
        SelectedUnit = null;
        
        //unit.OccupiedTile.ClearUnit();
    }

    public List<Tile> GetAvailableTiles(Unit unit)
    {
        var availableTiles = new List<Tile>();

        foreach (var tile in _grid.Tiles)
            if (tile.IsAvailableForUnit(unit))
                availableTiles.Add(tile);
        return availableTiles;
    }

    public void UpdateUnit(Unit unit)
    {
        // Обновить отображение юнита на игровом поле
        unit.UpdateVisuals();
    }
}
