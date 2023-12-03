using System;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    private GameController _gameController;

    public Unit SelectedUnit { get; private set; }

    // public delegate void UnitSelectedEventHandler(Unit unit, Selector selector);

    private void OnEnable()
    {
        _gameController = GetComponentInParent<GameController>();
        _gameController.UnitSelected += SelectUnit;
    }

    private void OnDisable()
    {
        _gameController.UnitSelected -= SelectUnit;
    }

    public void SelectUnit(Unit unit)
    {
        var uiManager = UIManager.Instance;
        SelectedUnit = unit;
        
        SelectedUnit.OccupiedTile.State = SelectedUnit.Stats.Type switch
        {
            UnitType.Player => TileState.OccupiedByPlayer,
            UnitType.Ally => TileState.OccupiedByAlly,
            UnitType.Enemy => TileState.OccupiedByEnemy,
            _ => throw new ArgumentOutOfRangeException()
        };
        SelectedUnit.OccupiedTile.SelectTile();
        SelectedUnit.AvailableMoves =
            new HashSet<Tile>(uiManager.PathConstructor.FindAvailableMoves(unit.OccupiedTile, unit.Stats.MovementPoints));
        uiManager.GridUI.HighlightAvailableMoves(SelectedUnit.AvailableMoves, unit.OccupiedTile.State);
    }

    public void UnselectUnit(Unit unit)
    {
        SelectedUnit = unit;
        if (SelectedUnit.AvailableMoves != null)
            UIManager.Instance.GridUI.HighlightTiles(SelectedUnit.AvailableMoves, TileState.Standard);
        SelectedUnit.OccupiedTile.UnselectTile();
        if (CanMoveMore(SelectedUnit))
            SelectUnit(SelectedUnit);
        SelectedUnit = null;
    }

    public bool CanMoveMore(Unit unit)
    {
        return unit.Stats.MovementPoints > 1 && unit.Stats.EnergyForMove > 15f;
    } 
}