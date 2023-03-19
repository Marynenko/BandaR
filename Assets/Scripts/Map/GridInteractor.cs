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

    public void SelectCell(Cell cell)
    {

        cell.EUnitState = State.Standard;
        cell.EUnitOn = UnitOn.Yes;
        cell.ChangeColor(cell.CellUnitOnColor);
    }

    public void UnselectCell(Cell cell) 
    {
        cell.EUnitState = State.Standard;
        cell.EUnitOn = UnitOn.No;
        cell.ChangeColor(cell.CellStandardColor);
    }

    internal void ChangeUnitStats(Unit unit, UnitStatus status)
    {
        if (status == UnitStatus.Unselected)
        {
            unit.Status = UnitStatus.Selected;
            SelectCell(unit.CurrentCell);
            Debug.Log("Unit Selected!");
        }
        else
        {
            unit.Status = UnitStatus.Unselected;
            UnselectCell(unit.CurrentCell);
            Debug.Log("Unit Unselected!");
        }
    }

}


