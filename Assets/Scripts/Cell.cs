﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum State
{
    Standard,
    Selected,
    Movement,
    Impassable,
    Reachable,
    Default
}

public enum UnitOnStatus // test v.1
{
    Yes,
    No
}

public class Cell : MonoBehaviour
{
    public GridInteractor GICell;
    public List<Cell> Neighbours { get; private set; }

    [SerializeField] private MeshRenderer MeshRenderer;

    [HideInInspector] public State UnitState; // Состояние клетки.
    [HideInInspector] public UnitOnStatus UnitOn; // Юнит на клетке или нет.    
    [HideInInspector] public Vector2 Position; // Позиция Клетки.

    public Color CellStandardColor; //Стандартный цвет клетки.
    public Color CellUnitOnColor; // Цвет клетки на которой стоит гл. герой.
    public Color CellEnemyOnColor; // Цвет клетки на которой стоит враг.
    public Color CellSelectedColor; // Цвет клетки - выбранной
    public Color CellMovementColor; // Цвет клетки - Для движения

    public int Row { get; private set; }
    public int Column { get; private set; }    

    private bool _isWalkable;

    // Юнит который на клетке. ДОПИЛИТЬ

    public void ChangeColor(Color color)
    {
        MeshRenderer.material.color = color;
    }
    public void Initialize(int row, int column, GridInteractor gridInteractor, bool isWalkable)
    {
        name = $"X: {row} Y: {column}";
        Row = row;
        Column = column;
        GICell = gridInteractor;
        _isWalkable = isWalkable;
        Position = new Vector2(row, column);
        Neighbours = new List<Cell>(4);
    }

    public bool IsWalkable()
    {
        return _isWalkable && UnitOn == UnitOnStatus.No;
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

    public void PlaceUnit(Unit unit)
    {
        GICell.SelectedUnit = unit;
        UnitOn = UnitOnStatus.Yes;
    }

    public void RemoveUnit(Unit unit)
    {
        unit = null;
        UnitOn = UnitOnStatus.No;
    }

    public void FindNeighbors(List<Cell> cells)
    {
        int width = Convert.ToInt32(GICell.Cells.Max(cell => cell.Position.x) + 1);

        // Находим индекс текущей клетки в списке всех клеток
        int index = cells.IndexOf(this);

        // Находим соседние клетки по индексам в списке
        Cell up = (index >= width) ? cells[index - width] : null;
        Cell down = (index < cells.Count - width) ? cells[index + width] : null;
        Cell left = (index % width != 0) ? cells[index - 1] : null;
        Cell right = (index % width != width - 1) ? cells[index + 1] : null;

        // Добавляем соседние клетки в список
        if (up != null) Neighbours.Add(up);
        if (down != null) Neighbours.Add(down);
        if (left != null) Neighbours.Add(left);
        if (right != null) Neighbours.Add(right);
    }
}

