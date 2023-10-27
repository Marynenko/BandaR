using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    #region Variables

    public MovementManager MovementManager;
    public InputPlayer Input;
    public Grid Grid;
    public Selector Selector;

    // Определение делегата
    public delegate void SelectionUnitHandler(Unit unit);

    // Определение событий
    public event SelectionUnitHandler UnitSelected;

    private List<Tile> _path;
    private bool _pathIsFound;

    #endregion

    public void HandleUnitClick(Unit unit)
    {
        if (unit.Stats.Type == UnitType.Player && unit.Status == UnitStatus.Available)
            UnitSelected?.Invoke(unit);

        UIManager.Instance.MenuAction.HideMenu();
    }

    public void HandleTileClick(Tile tile)
    {
        var selectedUnit = Selector.SelectedUnit;

        if (!_pathIsFound)
        {
            _path = FindPath(selectedUnit, tile);
            SetUnitTarget(selectedUnit, tile);
            _pathIsFound = true;
        }

        HandleTileMovement(selectedUnit);

        if (!selectedUnit.UnitIsMoving)
            _pathIsFound = false;
    }

    private List<Tile> FindPath(Unit unit, Tile tile) =>
        Selector.PathConstructor.FindPathToTarget(unit, tile, out _);

    private void SetUnitTarget(Unit selectedUnit, Tile tile)
    {
        var units = Grid.AllUnits;
        foreach (var unit in units)
            if (unit == selectedUnit)
                unit.Target = tile;
    }

    private void HandleTileMovement(Unit unit)
    {
        unit.OccupiedTile.UnselectTile();
        UIManager.Instance.GridUI.HighlightTiles(unit.AvailableMoves, TileState.Standard);

        // MovementManager.MoveUnitAlongPath(unit, _path, ref Input.IsUnitClickable);
        // MovementManager.SetUnitClickable(ref Input.IsUnitClickable);
        MovementManager.MoveUnitAlongPath(unit, _path, ref Input.IsUnitClickable);

        if (unit.UnitIsMoving) 
            return;
        
        if (_path.Count <= 1) 
            return;
        
        if (Selector.CanMoveMore(unit))
            Selector.SelectUnit(unit);
    }
}