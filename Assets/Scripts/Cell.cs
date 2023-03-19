using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public enum State
{
    Standard,
    Selected,
    Movement
}

public enum UnitOn // test v.1
{
    Yes,
    No
}

public class Cell : MonoBehaviour
{
    public GridInteractor GICell;
    [SerializeField] private MeshRenderer MeshRenderer;


    [HideInInspector] public State EUnitState; // Состояние клетки.
    [HideInInspector] public UnitOn EUnitOn; // Юнит на клетке или нет.    
    [HideInInspector] public Vector2 Position; // Позиция Клетки.


    public Color CellStandardColor; //Стандартный цвет клетки.
    public Color CellHoveringColor;// Цвет при наведении на клетку.
    public Color CellUnitOnColor; // Цвет клетки на которой стоит гл. герой.
    public Color CellEnemyOnColor; // Цвет клетки на которой стоит враг.

    // Юнит который на клетке. ДОПИЛИТЬ

    public void ChangeColor(Color color)
    {
        MeshRenderer.material.color = color;
    }

    //private void OnMouseEnter()
    //{
        //var grid = GICell;

        //foreach (var unit in grid.Units)
        //{
        //    if (unit.Status != UnitStatus.Selected)
        //        if (unit.CurrentCell.Position == Position)
        //        {
        //            if (unit.Type == UnitType.Enemy)
        //            {
        //                ChangeColor(CellEnemyOnColor);
        //            }
        //        }
        //}

    //}

    //private void OnMouseExit()
    //{
    //    var grid = GICell;

    //    foreach (var unit in grid.Units)
    //    {
    //        if (unit.Status != UnitStatus.Selected)
    //            ChangeColor(CellStandardColor);
    //    }
    //}

    //private void OnMouseDown()
    //{
    //    var grid = GICell;

    //    foreach (var unit in  grid.Units)
    //    {
    //        if (unit.CheckUnitOnCell(this) == true)
    //        {
    //            if (unit.Type == UnitType.Player)
    //            {
    //                unit.CurrentCell.ChangeColor(CellUnitOnColor);
    //                //CheckOtherUnitCell(unit, unitIsOnCell);                    
    //            }
    //        }
    //    }
    //}

}
