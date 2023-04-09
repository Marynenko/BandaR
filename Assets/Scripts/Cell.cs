using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public enum State
{
    Standard,
    Selected,
    Movement,
    Impassable,
    Reachable,
}

public enum UnitOn // test v.1
{
    Yes,
    No
}

public class Cell : MonoBehaviour
{
    private IUnit _currentUnit;
    private bool _isWalkable;
    private int _distance;

    public GridInteractor Interactor;
    public List<Cell> Neighbours { get; set; }
    public int Row { get; private set; }
    public int Column { get; private set; }

    [SerializeField] private MeshRenderer MeshRenderer;
    [HideInInspector] public State CurrentState; // Состояние клетки.
    [HideInInspector] public UnitOn CellStatus; // Юнит на клетке или нет.    
    [HideInInspector] public Vector2 Coordinates; // Позиция Клетки.

    public Color ColorStandardCell; //Стандартный цвет клетки.
    public Color ColorUnitOnCell; // Цвет клетки на которой стоит гл. герой.
    public Color ColorEnemyOnCell; // Цвет клетки на которой стоит враг.
    public Color ColorSelectedCell; // Цвет клетки - выбранной
    public Color ColorMovementCell; // Цвет клетки - Для движения

    internal readonly int MovementCost = 1;

    public void Initialize(int row, int column, GridInteractor gridInteractor, bool isWalkable, UnitOn unitOn)
    {
        name = $"X: {row} Y: {column}";
        Row = row;
        Column = column;
        Interactor = gridInteractor;
        _isWalkable = isWalkable;
        CellStatus = unitOn;
        CurrentState = State.Standard;
        Coordinates = new Vector2(row, column);
        Neighbours = new List<Cell>(4);
    }

    public bool IsWalkable()
    {
        return _isWalkable && CellStatus == UnitOn.No;
    }

    public bool SetIsWalkable(bool atribute) => _isWalkable = atribute;

    public void ChangeColor(Color color)    {
        MeshRenderer.material.color = color;
    }

    public Vector3 GetCellSize() => MeshRenderer.bounds.size;

    public void SetReachable(int movementPoints, bool isReachable)
    {
        if (movementPoints >= 0)
        {
            CurrentState = isReachable ? State.Reachable : State.Impassable;
            foreach (Cell neighbor in Neighbours)
            {
                if (neighbor._isWalkable && neighbor.CurrentState != State.Impassable && neighbor.MovementCost <= movementPoints)
                {
                    neighbor.SetReachable(movementPoints - neighbor.MovementCost, isReachable);
                }
            }
        }
    }

    public void SetState(State state, int distance = 0)
    {
        CurrentState = state;
        _distance = distance;
    }

    public void SelectCell()
    {
        var unit = _currentUnit as Unit; // Тут где-то ошибка завтра првоерить 02.04
        var unitColor = unit.Type == UnitType.Player ? unit.CurrentCell.ColorSelectedCell : unit.CurrentCell.ColorEnemyOnCell; // получение цвета юнита в зависимости от его типа
        ChangeColor(unitColor);
        CellStatus = UnitOn.Yes;
        CurrentState = State.Impassable;
        SetIsWalkable(false);
    }

    public void UnselectCell()
    {
        ChangeColor(ColorStandardCell);
        CellStatus = UnitOn.No;
        CurrentState = State.Reachable;
        SetIsWalkable(true);
    }

    public void SetUnit(IUnit unit) => _currentUnit = unit;
    public void ClearUnit() => _currentUnit = null;
}