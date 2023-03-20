using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class GridInteractor : Grid
{
    public List<Cell> Cells = new List<Cell>();

    private Unit _unitSelected;

    public void ChangeUnitStats(Unit unit, UnitStatus status)
    {
        if (status == UnitStatus.Unselected)
        {
            SelectUnit(unit);
            SelectCell(unit.CurrentCell, unit.Status);
            Debug.Log("Unit Selected!");
        }
        else
        {
            UnselectUnit(unit);
            UnselectCell(unit.CurrentCell);
            Debug.Log("Unit Unselected!");
        }
    }

    private void SelectUnit(Unit unit)
    {
        //unit.Type = UnitType.Player;
        unit.Status = UnitStatus.Selected;

    }

    public void UnselectUnit(Unit unit)
    {
        unit.Status = UnitStatus.Unselected;

    }

    private bool CheckOtherUnit(Unit unitToCheck)
    {
        foreach (var unit in Units) 
        {
            if (unitToCheck.Equals(unit))
                break;
            else if (unitToCheck.Status == UnitStatus.Unselected && unit.Status == UnitStatus.Unselected) // Unselect
                return true;
            else if (unitToCheck.Status == UnitStatus.Unselected && unit.Status == UnitStatus.Selected) // Select
                return false;
            else if (unitToCheck.Status == UnitStatus.Selected && unit.Status == UnitStatus.Unselected) // Select
                return false;
            else if (unitToCheck.Status == UnitStatus.Selected && unit.Status == UnitStatus.Selected) // Select
                return false;
            else if (unitToCheck.Status == UnitStatus.Selected && unit.Status == unitToCheck.Status) // Select
                return false;
        }

        return true;

    }

    public void SelectCell(Cell cell, UnitStatus status)
    {
        if (status == UnitStatus.Selected)
        {
            cell.EUnitOn = UnitOn.Yes;
            cell.EUnitState = State.Movement;
            cell.ChangeColor(cell.CellUnitOnColor);
        }
        else
        {
            cell.EUnitOn = UnitOn.No;
            cell.EUnitState = State.Selected;
            cell.ChangeColor(cell.CellStandardColor);
        }

    }

    public void UnselectCell(Cell cell)
    {
        cell.EUnitState = State.Standard;
        cell.EUnitOn = UnitOn.No;
        cell.ChangeColor(cell.CellStandardColor);
    }



    private UnitType CheckUnitType(Unit unit) => unit.Type; // Enemy|Player

}


