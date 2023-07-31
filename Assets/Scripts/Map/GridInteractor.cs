using System.Collections.Generic;
using System.Linq;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static UnityEngine.UI.CanvasScaler;

public class GridInteractor : MonoBehaviour
{
    private Grid _grid;
    private List<Cell> _availableMoves;

    public PathConstructor PathConstructor;
    public Unit SelectedUnit { get; set; }
    public List<Cell> AvailableMoves => _availableMoves.AsReadOnly().ToList();


    public void OnEnable()
    {
        //GameController.OnUnitAction += HandleUnitAction;
        _grid = GetComponentInParent<Grid>();
        _availableMoves = new List<Cell>();
    }  

    public void UpdateUnit(Unit unit)
    {
        // Обновить отображение юнита на игровом поле
        unit.UpdateVisuals();
    } 

    public void HandleUnitSelected(Unit unit, GridSelector selector)
    {
        if (unit.Type == UnitType.Player)
            HandlePlayerSelected(unit, selector);
        else if (unit.Type == UnitType.Enemy)
            HandleEnemySelected(unit, selector);
    }

    private void HandlePlayerSelected(Unit player, GridSelector selector)
    {
        UIManager.Instance.OpenMenuAction();

        SelectedUnit = player;
        selector.SelectedUnit = player;
        player.CurrentCell.SelectCell();
        selector.SelectCellToMoveFrom(player.CurrentCell, UnitType.Player /*true*/);
        player.CurrentCell.UnitOn = true; // тут или перед SelectCellToMoveFrom?
    }

    private void HandleEnemySelected(Unit enemy, GridSelector selector)
    {
        enemy.CurrentCell.Available = true;
        //enemy.CurrentCell.SetUnit(enemy);
        SelectedUnit = enemy;
        selector.SelectedUnit = enemy;
        enemy.Status = UnitStatus.Unavailable;
        enemy.CurrentCell.SelectCell();
        selector.SelectCellToMoveFrom(enemy.CurrentCell, UnitType.Enemy, true);
        enemy.CurrentCell.UnitOn = true;
    }

    public void HandleUnitDeselection(Unit selectedUnit, Unit unit, GridSelector selector)
    {
        selector.UnselectUnit(selectedUnit);
        unit.CurrentCell.UnselectCell();
        selector.ChangeAvailableCellsColor();
        selector.SelectUnit(unit);
        HighlightAvailableMoves(AvailableMoves, unit.CurrentCell.ColorMovementCell, selector);
    }

    public void HighlightAvailableMoves(IReadOnlyList<Cell> availableMoves, Color color, GridSelector selector)
    {
        selector.ChangeAvailableCellsColor();
        HighlightCell(availableMoves.First(), availableMoves.First().ColorUnitOnCell);
        availableMoves.Skip(1).ToList().ForEach(cell => HighlightCell(cell, color));
    }

    public void UnhighlightAllCells(GridSelector selector)
    {
        // Идем по всем клеткам на игровом поле
        foreach (var cell in _grid.Cells)
        {
            // Если клетка подсвечена и больше не доступна для хода, снимаем подсветку
            if (cell.CurrentState == State.Reachable && cell != SelectedUnit.CurrentCell)
                cell.UnselectCell();
        }
    }

    public void UnhighlightAvailableMoves(Cell currentCell)
    {
        // Идем по всем клеткам на игровом поле
        foreach (var cell in currentCell.Neighbours)
            cell.UnhighlightCell();
    }

    public void HighlightCell(Cell cell, Color color)
    {
        cell.ChangeColor(color);
    }

    public List<Cell> GetAvailableCells(Unit unit)
    {
        var availableCells = new List<Cell>();

        foreach (var cell in _grid.Cells)
            if (cell.IsAvailableForUnit(unit))
                availableCells.Add(cell);
        return availableCells;
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




