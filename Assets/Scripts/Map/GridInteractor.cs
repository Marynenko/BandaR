using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public class GridInteractor : Grid
{
    private Cell _cell;

    public List<Cell> Cells = new List<Cell>();
    

    //private void OnMouseEnter()
    //{
    //    RaycastHit unitPositionCell;

    //    if (Physics.Raycast(_unit.Ray, out unitPositionCell))
    //    {
    //        _unit.CurrentCell = unitPositionCell.collider.GetComponent<Cell>();
    //        _unit.CurrentCell.ChangeColor(_cell.CellUnitOnColor);
    //    }
    //    //CheckAndGiveCurrentCell();
    //    //CheckAndGiveCurrentCell(_unit.ReleaseBeam());
    //}

    //private void OnMouseExit()
    //{
    //    RaycastHit unitPositionCell;

    //    if (!Physics.Raycast(_unit.Ray, out unitPositionCell))
    //    {
    //        var cell = unitPositionCell.collider.GetComponent<Cell>();
    //        cell.ChangeColor(_cell.CellHoveringColor);
    //    }

    //    //_cell.ChangeColor(_cell.CellStandardColor);
    //    //CheckAndGiveCurrentCell(_unit.ReleaseBeam());

    //}
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
            unit.Status = UnitStatus.Unselected;
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
            if (!unitToCheck.Equals(unit))
                break;
            if (unitToCheck.Status == UnitStatus.Selected && unit.Status == unitToCheck.Status)
            {
                return false;
            }    
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


