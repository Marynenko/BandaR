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
        SelectedUnit = unit;
        SelectedUnit.OccupiedTile.State = SelectedUnit.Type switch
        {
            UnitType.Player => TileState.OccupiedByPlayer,
            UnitType.Enemy => TileState.OccupiedByEnemy,
            _ => throw new ArgumentOutOfRangeException()
        };
        SelectedUnit.OccupiedTile.SelectTile();
        _availableMoves = PathConstructor.GetAvailableMoves(unit.OccupiedTile, unit.MovementRange);
        GridUI.HighlightAvailableMoves(_availableMoves, unit.OccupiedTile.State);
    }
    
    public void UnselectUnit(Unit unit)
    {
        GridUI.HighlightTiles(_availableMoves, TileState.Standard);
        SelectedUnit.OccupiedTile.UnselectTile();
        UnitTurnIsOver();
        SelectedUnit = null;
    }
    
    public bool UnitTurnIsOver()
    {
        // Проверяем, есть ли у персонажа еще очки передвижения
        if (SelectedUnit != null && SelectedUnit.MovementPoints > 1)
        {
            // Если есть, снова активируем персонажа
            SelectUnit(SelectedUnit);
        }

        return true;
    }

    public void UpdateUnit(Unit unit)
    {
        // Обновить отображение юнита на игровом поле
        unit.UpdateVisuals();
    }
}
