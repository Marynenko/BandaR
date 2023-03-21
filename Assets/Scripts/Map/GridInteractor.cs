using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class GridInteractor : Grid
{
    private Unit _selectedUnit;
    private List<Cell> _reachableCells = new List<Cell>();
    public List<Cell> Neighbors { get { return _reachableCells; } }

    public void MarkCellAsReachable(Cell cell)
    {
        if (cell != null && cell.EUnitState != State.Impassable)
        {
            cell.ChangeColor(cell.CellHoveringColor);
            cell.EUnitState = State.Reachable;
            _reachableCells.Add(cell);
        }
    }

    public void UnmarkCellAsReachable(Cell cell)
    {
        if (cell != null && cell.EUnitState == State.Reachable)
        {
            cell.ChangeColor(cell.CellStandardColor);
            cell.EUnitState = State.Default;
            _reachableCells.Remove(cell);
        }
    }

    public List<Cell> GetReachableCells(Unit selectedUnit)
    {
        var reachableCells = new List<Cell>();

        if (selectedUnit != null)
        {
            var startCell = selectedUnit.CurrentCell;
            var movementRange = selectedUnit.PossibleMovements.Count;
            var queue = new Queue<Cell>();
            queue.Enqueue(startCell);

            while (queue.Count > 0)
            {
                var currentCell = queue.Dequeue();

                if (currentCell.EUnitState != State.Impassable && currentCell != startCell && currentCell != selectedUnit.CurrentCell)
                {
                    var distance = Mathf.Abs((currentCell.Position - startCell.Position).x) + Mathf.Abs((currentCell.Position - startCell.Position).y);

                    if (distance <= movementRange)
                    {
                        MarkCellAsReachable(currentCell);
                        reachableCells.Add(currentCell);

                        foreach (var neighbor in currentCell.GICell.Neighbors)
                        {
                            if (!reachableCells.Contains(neighbor) && !queue.Contains(neighbor) && neighbor.EUnitState != State.Impassable)
                            {
                                queue.Enqueue(neighbor);
                            }
                        }
                    }
                }
            }
        }

        return reachableCells;
    }

    public void UnselectCell(Cell cell)
    {
        if (cell != null)
        {
            cell.ChangeColor(cell.CellStandardColor);
            cell.UnitOn = UnitOnStatus.No;
            cell.EUnitState = State.Default;
        }
    }

    public void SelectUnit(Unit unit)
    {
        _selectedUnit = unit;
        _reachableCells = GetReachableCells(unit);
    }

    public void UnselectUnit(Unit unit)
    {
        _selectedUnit = null;
        foreach (var cell in _reachableCells)
        {
            UnmarkCellAsReachable(cell);
        }
        _reachableCells.Clear();
    }

    public void SelectCell(Cell cell, UnitType unitType)
    {
        if (_selectedUnit == null) return;

        if (unitType == UnitType.Player)
        {
            if (_reachableCells.Contains(cell))
            {
                _selectedUnit.CurrentCell.UnitOn = UnitOnStatus.No;
                _selectedUnit.Status = UnitStatus.Unselected;
                _selectedUnit.MoveTo(cell);
                _selectedUnit.Status = UnitStatus.Moved;
                cell.UnitOn = UnitOnStatus.Yes;
                UnselectUnit(_selectedUnit);
            }
        }
        else if (unitType == UnitType.Enemy)
        {
            UnselectUnit(_selectedUnit);
            SelectUnit(cell.UnitOnCell);
        }
    }

    public bool CanMoveToCell(Unit unit, Cell cell)
    {
        return _reachableCells.Contains(cell);
    }

    public void MoveUnitToCell(Unit unit, Cell cell)
    {
        if (_reachableCells.Contains(cell))
        {
            SelectCell(cell, UnitType.Player);

        }
        // Check if there is an enemy on the cell
        if (cell.UnitOn == UnitOnStatus.Yes && cell.UnitOnCell.Type == UnitType.Enemy)
        {
            // Attack the enemy
            _selectedUnit.Attack(cell.UnitOnCell);

            // Check if the enemy is still alive
            if (cell.UnitOnCell.CurrentHealth <= 0)
            {
                // Remove the enemy from the grid
                cell.UnitOnCell.RemoveFromGrid();
                cell.UnitOn = UnitOnStatus.No;

                // Update the score and display it
                GameManager.Instance.Score++;
                UIManager.Instance.UpdateScoreText(GameManager.Instance.Score);

                // Check if the game is over
                if (GameManager.Instance.Score >= GameManager.Instance.ScoreToWin)
                {
                    GameManager.Instance.EndGame();
                }
            }

            // Unselect the unit
            UnselectUnit(_selectedUnit);
        }
        else
        {
            // Move the unit to the cell
            _selectedUnit.MoveTo(cell);

            // Update the unit's status
            _selectedUnit.Status = UnitStatus.Moved;

            // Unselect the unit
            UnselectUnit(_selectedUnit);
        }
    }
    private void EndGame()
    {
        Debug.Log("Game Over!");
        // Code to end the game
    }

    // Method to restart the game
    private void RestartGame()
    {
        Debug.Log("Restarting Game...");
        // Code to restart the game
    }


}


