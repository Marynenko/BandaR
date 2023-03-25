using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UI.CanvasScaler;

public enum State
{
    Standard,
    Selected,
    Movement,
    Impassable,
    Reachable,
    Default
}

public enum StatusUnitOn // test v.1
{
    Yes,
    No
}

public class Cell : MonoBehaviour
{
    private const float POSITION_Y = .8f;
    private const float MAX_DISTANCE = 3f;

    private List<Unit> _units = new List<Unit>();
    private bool _isWalkable;

    public GridInteractor GICell;
    public List<Cell> Neighbors { get; set; }

    [SerializeField] private MeshRenderer MeshRenderer;

    [HideInInspector] public State UnitState; // Состояние клетки.
    [HideInInspector] public StatusUnitOn UnitOn; // Юнит на клетке или нет.    
    [HideInInspector] public Vector2 Coordinates; // Позиция Клетки.

    public Color CellStandardColor; //Стандартный цвет клетки.
    public Color CellUnitOnColor; // Цвет клетки на которой стоит гл. герой.
    public Color CellEnemyOnColor; // Цвет клетки на которой стоит враг.
    public Color CellSelectedColor; // Цвет клетки - выбранной
    public Color CellMovementColor; // Цвет клетки - Для движения

    public int Row { get; private set; }
    public int Column { get; private set; }

    // Юнит который на клетке. ДОПИЛИТЬ

    public void SetUnit(Unit unit)
    {
        _units.Add(unit);
        unit.transform.position = new Vector3(transform.position.x, POSITION_Y, transform.position.z);
    }

    public void RemoveUnit(Unit unit)
    {
        if (_units.Contains(unit))
        {
            _units.Remove(unit);
        }
    }

    public void Initialize(int row, int column, GridInteractor gridInteractor, bool isWalkable, StatusUnitOn unitOn)
    {
        name = $"X: {row} Y: {column}";
        Row = row;
        Column = column;
        GICell = gridInteractor;
        _isWalkable = isWalkable;
        UnitOn = unitOn;
        UnitState = State.Default;
        Coordinates = new Vector2(row, column);
        Neighbors = new List<Cell>(4);
    }

    public bool IsWalkable()
    {
        return _isWalkable && UnitOn == StatusUnitOn.No;
    }

    #region Set Color
    public void ChangeColor(Color color)
    {
        MeshRenderer.material.color = color;
    }
    public void SetHighlight()
    {
        MeshRenderer.material.color = CellSelectedColor;
    }

    public void SetStandard()
    {
        MeshRenderer.material.color = CellStandardColor;
    }

    public void SetMovement()
    {
        MeshRenderer.material.color = CellMovementColor;
    }
    #endregion


    //public void FindNeighbors()
    //{
    //    var cells = new List<Cell>();
    //    int width = Convert.ToInt32(GICell.Cells.Max(cell => cell.Coordinates.x) + 1);

    //    // Находим индекс текущей клетки в списке всех клеток
    //    int index = cells.IndexOf(this);

    //    // Находим соседние клетки по индексам в списке
    //    Cell up = (index >= width) ? cells[index - width] : null;
    //    Cell down = (index < cells.Count - width) ? cells[index + width] : null;
    //    Cell left = (index % width != 0) ? cells[index - 1] : null;
    //    Cell right = (index % width != width - 1) ? cells[index + 1] : null;

    //    // Добавляем соседние клетки в список
    //    if (up != null) Neighbors.Add(up);
    //    if (down != null) Neighbors.Add(down);
    //    if (left != null) Neighbors.Add(left);
    //    if (right != null) Neighbors.Add(right);
    //}




}



