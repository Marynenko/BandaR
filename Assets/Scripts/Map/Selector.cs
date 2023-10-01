using System;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    private Grid _grid;
    private GameController _gameController;

    public PathConstructor PathConstructor;
    public Unit SelectedUnit { get; private set; }

    // public delegate void UnitSelectedEventHandler(Unit unit, Selector selector);

    private void OnEnable()
    {
        _grid = GetComponentInParent<Grid>();
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
        // SelectedUnit.AvailableMoves = PathConstructor.GetAvailableMoves(unit.OccupiedTile, unit.MovementRange);
        SelectedUnit.AvailableMoves =
            new HashSet<Tile>(PathConstructor.GetAvailableMoves(unit.OccupiedTile, unit.MovementPoints));
        GridUI.Instance.HighlightAvailableMoves(SelectedUnit.AvailableMoves, unit.OccupiedTile.State);
    }

    public void UnselectUnit(Unit unit)
    {
        SelectedUnit = unit;
        if (SelectedUnit.AvailableMoves != null)
            GridUI.Instance.HighlightTiles(SelectedUnit.AvailableMoves, TileState.Standard);
        SelectedUnit.OccupiedTile.UnselectTile();
        if (SelectedUnit.CanMoveMore())
            SelectUnit(SelectedUnit);
        SelectedUnit = null;
    }
}