using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridInteractor : Grid
{
    public delegate void UnitSelectedEventHandler(Unit unit, UnitType unitType);
    public static event UnitSelectedEventHandler OnUnitSelected;
    public delegate void UnitActionEventHandler(UnitActionType actionType, Unit unit, Cell cell);
    public static event UnitActionEventHandler OnUnitAction;
    public delegate void EnemySelectedEventHandler(Unit enemy);
    public static event EnemySelectedEventHandler OnEnemySelected;
    public delegate void PlayerSelectedEventHandler(Unit player);
    public static event PlayerSelectedEventHandler OnPlayerSelected;

    [SerializeField] private List<Cell> _availableMoves;

    public List<Cell> Cells;
    public Unit SelectedUnit { get; set; }

    public void InitializeActions()
    {
        OnUnitSelected += HandleUnitSelected;
        OnUnitAction += HandleUnitAction;
        OnEnemySelected += HandleEnemySelected;
        OnPlayerSelected += HandlePlayerSelected;
    }

    List<Direction> directions = new List<Direction>()
    {
        new Direction(0, 1),   // Up
        new Direction(0, -1),  // Down
        new Direction(-1, 0),  // Left
        new Direction(1, 0)    // Right
    };


    public void SelectUnit(Unit unit)
    {
        //var selectedUnitExist = SelectedUnit != null;
        if (SelectedUnit != null)
            UnselectUnit(SelectedUnit);
        
        if (unit.Type == UnitType.Enemy)
        {
            OnEnemySelected?.Invoke(unit);
        }
        else if (unit.Type == UnitType.Player)
        {
            OnPlayerSelected?.Invoke(unit);
        }
    }

    public void UnselectUnit(Unit unit)
    {
        unit.Status = UnitStatus.Unselected;
        OnUnitSelected?.Invoke(unit, unit.Type);
        unit.CurrentCell.ChangeColor(unit.CurrentCell.CellStandardColor);
        unit.CurrentCell.UnitOn = StatusUnitOn.No;
    }

    public void SelectCell(Cell cell, UnitType unitType)
    {
        UnselectCells();
        if (unitType == UnitType.Player)
        {
            _availableMoves = GetAvailableMoves(cell, unitType, 2);
        }
        else if (unitType == UnitType.Enemy)
        {
            _availableMoves = GetAvailableMoves(cell, unitType, 2);
        }
        foreach (var moveCell in _availableMoves)
        {
            moveCell.ChangeColor(moveCell.CellMovementColor);
        }
    }

    public void MoveUnit(Unit unit, Cell targetCell)
    {
        if (unit.CurrentCell != null)
        {
            unit.CurrentCell.ChangeColor(unit.CurrentCell.CellStandardColor);
            unit.CurrentCell.UnitOn = StatusUnitOn.No;
        }

        targetCell.ChangeColor(Color.gray);
        targetCell.UnitOn = StatusUnitOn.Yes;
        unit.MoveToCell(targetCell);

        if (OnUnitAction != null)
        {
            OnUnitAction.Invoke(UnitActionType.Move, unit, targetCell);
        }
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
        if (SelectedUnit != null)
        {
            UnselectUnit(SelectedUnit);
        }
        SelectedUnit = player;
        player.Status = UnitStatus.Selected;
        player.CurrentCell.ChangeColor(player.CurrentCell.CellSelectedColor);
        player.CurrentCell.UnitOn = StatusUnitOn.Yes;
    }

    private void HandleEnemySelected(Unit enemy)
    {
        if (SelectedUnit != null)
        {
            UnselectUnit(SelectedUnit);
        }
        

        SelectedUnit = enemy;
        enemy.Status = UnitStatus.Selected;
        enemy.CurrentCell.ChangeColor(enemy.CurrentCell.CellEnemyOnColor);
    }

    private void HandleUnitAction(UnitActionType actionType, Unit unit, Cell cell)
    {
        if (actionType == UnitActionType.Move)
        {
            unit.Move(cell);
        }
    }

    public List<Cell> GetAvailableMoves(Cell cell, UnitType unitType, int maxMoves)
    {
        List<Cell> result = new List<Cell>();
        result.Add(cell);

        if (maxMoves <= 0)
        {
            return result;
        }
        foreach (var neighbour in GetNeighbourCells(cell))
        {
            if (neighbour.IsWalkable() && neighbour.UnitOn == StatusUnitOn.No)
            {
                result.AddRange(GetAvailableMoves(neighbour, unitType, maxMoves - 1));
            }
        }
        return result;
    }

    public List<Cell> GetNeighbourCells(Cell cell)
    {
        List<Cell> result = new List<Cell>();

        foreach (Direction dir in directions)
        {
            int xOffset = dir.XOffset;
            int yOffset = dir.YOffset;

            int x = cell.Row + xOffset;
            int y = cell.Column + yOffset;

            Cell neighbour = Cells.FirstOrDefault(c => c.Row == x && c.Column == y);

            if (neighbour != null)
            {
                result.Add(neighbour);
            }
        }

        return result;
    }



    public void UnselectCells()
    {
        foreach (var cell in Cells)
        {
            cell.ChangeColor(cell.CellStandardColor);
        }
    }

    private bool IsWithinBounds(int x, int y)
    {
        return x >= 0 && x < 3 && y >= 0 && y < 3;
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




