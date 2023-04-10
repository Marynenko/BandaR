using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridInteractor : MonoBehaviour
{
    public delegate void UnitSelectedEventHandler(Unit unit, UnitType unitType);
    public static event UnitSelectedEventHandler OnUnitSelected;

    private Grid _grid;
    private List<Cell> _availableMoves;

    public PathConstructor PathConstructor;
    public GridSelector GridSelector;
    public IReadOnlyList<Cell> AvailableMoves => _availableMoves.AsReadOnly().ToList();
    public Unit SelectedUnit { get; set; }


    public void OnEnable()
    {
        //GameController.OnUnitAction += HandleUnitAction;
        _grid = GetComponentInParent<Grid>();
        _availableMoves = new List<Cell>();
        OnUnitSelected += HandleUnitSelected;
    }
    private void OnDestroy()
    {
        OnUnitSelected -= HandleUnitSelected;
    }    

    public void SelectUnit(Unit unit)
    {
        if (SelectedUnit != null)
        {
            SelectedUnit.CurrentCell.UnselectCell();
            UnselectUnit(SelectedUnit);
        }

        OnUnitSelected?.Invoke(unit, unit.Type);
    }


    public void UnselectUnit(Unit unit)
    {
        //unit.Status = UnitStatus.Unselected;
        SelectedUnit = null;
        unit.CurrentCell.ClearUnit();
    }

    public void UpdateUnit(Unit unit)
    {
        // Обновить отображение юнита на игровом поле
        unit.UpdateVisuals();
    } 

    private void HandleUnitSelected(Unit unit, UnitType unitType)
    {
        if (unitType == UnitType.Player)
        {
            HandlePlayerSelected(unit);
        }
        else if (unitType == UnitType.Enemy)
        {
            HandleEnemySelected(unit);
        }
    }

    private void HandlePlayerSelected(Unit player)
    {
        player.CurrentCell.SetUnit(player);
        SelectedUnit = player;
        player.Status = UnitStatus.Selected;
        player.CurrentCell.SelectCell();
        GridSelector.SelectCellToMove(player.CurrentCell, UnitType.Player, true);
        player.CurrentCell.UnitOn = true; // тут или перед SelectCellToMove?
    }

    private void HandleEnemySelected(Unit enemy)
    {
        enemy.CurrentCell.SetUnit(enemy);
        SelectedUnit = enemy;
        enemy.Status = UnitStatus.Selected;
        enemy.CurrentCell.SelectCell();
        GridSelector.SelectCellToMove(enemy.CurrentCell, UnitType.Enemy, true);
        enemy.CurrentCell.UnitOn = true;
    }

    private void HandleUnitAction(UnitActionType actionType, Unit unit, Cell cell)
    {
        if (actionType == UnitActionType.Move)
        {
            unit.MoveToCell(cell);
        }
    }

    public void HandleUnitDeselection(Unit selectedUnit, Unit unit)
    {
        UnselectUnit(selectedUnit);
        unit.CurrentCell.UnselectCell();
        GridSelector.UnselectCells();
        SelectUnit(unit);
        HighlightAvailableMoves(AvailableMoves, unit.CurrentCell.ColorMovementCell);
    }

    public void HighlightAvailableMoves(IReadOnlyList<Cell> availableMoves, Color color)
    {
        GridSelector.UnselectCells();
        HighlightCell(availableMoves.First(), availableMoves.First().ColorUnitOnCell);
        availableMoves.Skip(1).ToList().ForEach(cell => HighlightCell(cell, color));
    }



    public void HighlightCell(Cell cell, Color color)
    {
        cell.ChangeColor(color);
    }   

    public void MoveUnitAlongPath(Unit unit, List<Cell> path)
    {
        // Двигаем юнита поочередно на каждую ячейку из списка
        foreach (var cell in path)
        {
            unit.MoveToCell(cell); // изменен вызов метода
        }
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




