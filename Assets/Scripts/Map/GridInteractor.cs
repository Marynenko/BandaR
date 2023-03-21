using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class GridInteractor : Grid
{
    public List<Cell> Cells = new List<Cell>();

    private Unit _unitSelected;

    public void ChangeUnitStats(Unit unit, UnitStatus status)
    {
        if (status == UnitStatus.Selected)
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

    public void SelectCell(Cell cell, UnitStatus status)
    {
        if (status == UnitStatus.Selected)
        {
            cell.UnitOn = UnitOnStatus.Yes;
            cell.EUnitState = State.Movement;
            cell.ChangeColor(cell.CellUnitOnColor);
        }
        else
        {
            cell.UnitOn = UnitOnStatus.No;
            cell.EUnitState = State.Selected;
            cell.ChangeColor(cell.CellStandardColor);
        }

    }

    public void UnselectCell(Cell cell)
    {
        cell.EUnitState = State.Standard;
        cell.UnitOn = UnitOnStatus.No;
        cell.ChangeColor(cell.CellStandardColor);
    }



    private UnitType CheckUnitType(Unit unit) => unit.Type; // Enemy|Player

}


