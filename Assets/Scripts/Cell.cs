using System.Collections.Generic;
using UnityEngine;

public enum State
{
    Standard, // Стандартное состояние
    //Selected, // Выбран пользователем
    Movement, // Пользователь выбрал этот тайл для движения юнита
    Impassable, // Непроходимый тайл (например, стена или вода)
    Reachable, // Тайл, на который юнит может сделать ход (если это необходимо в вашей игре)
    OccupiedByPlayer, // Занят игроком
    OccupiedByEnemy, // Занят врагом
}

public class Cell : MonoBehaviour
{
    private bool _awailable;
    private int _distance;

    public GridInteractor Interactor;
    public List<Cell> Neighbours { get; set; }

    [HideInInspector] public State CurrentState; // Состояние клетки.
    [HideInInspector] public Vector2 Coordinates; // Позиция Клетки.

    public bool UnitOn; // Юнит на клетке или нет.    

    public Color ColorStandardCell; //Стандартный цвет клетки.
    public Color ColorUnitOnCell; // Цвет клетки на которой стоит гл. герой.
    public Color ColorEnemyOnCell; // Цвет клетки на которой стоит враг.
    public Color ColorSelectedCell; // Цвет клетки - выбранной
    public Color ColorMovementCell; // Цвет клетки - Для движения

    internal readonly int MovementCost = 1;

    public void Initialize(int row, int column, GridInteractor gridInteractor, bool isAwailable, bool unitOn)
    {
        name = $"X: {row} Y: {column}";
        Interactor = gridInteractor;
        _awailable = isAwailable;
        UnitOn = unitOn;
        CurrentState = State.Standard;
        Coordinates = new Vector2(row, column);
        Neighbours = new List<Cell>(4);        
    }

    public bool IsAwailable()
    {
        return _awailable && !UnitOn;
    }

    public bool SetAwailable(bool atribute) => _awailable = atribute;

    public void ChangeColor(Color color)    {
        GetComponent<MeshRenderer>().material.color = color;
    }

    public Vector3 GetCellSize() => GetComponent<MeshRenderer>().bounds.size;

    public void SetReachable(int movementPoints, bool isReachable)
    {
        if (movementPoints >= 0)
        {
            CurrentState = isReachable ? State.Reachable : State.Impassable;
            foreach (Cell neighbor in Neighbours)
            {
                if (neighbor._awailable && neighbor.CurrentState != State.Impassable && neighbor.MovementCost <= movementPoints)
                {
                    neighbor.SetReachable(movementPoints - neighbor.MovementCost, isReachable);
                }
            }
        }
    }

    public void SelectCell()
    {
        ChangeColor(ColorSelectedCell);
        UnitOn = true;
        SetAwailable(false);
    }

    public void UnselectCell()
    {
        // На момент когда я нажимаю на это, тогда у меня все выделения уже исчезли.
        ChangeColor(ColorStandardCell);
        UnitOn = false;
        CurrentState = State.Reachable;
        SetAwailable(true);
        UnhighlightAvailableMoves();
    }

    public void UnhighlightAvailableMoves()
    {
        // Идем по всем клеткам на игровом поле
        foreach (var cell in Neighbours)
        {
            cell.UnhighlightCell();
        }
    }

    public void UnhighlightCell()
    {
        ChangeColor(ColorStandardCell);
        UnitOn = false;
        SetState(State.Reachable);
        SetAwailable(true);
    }

    public void SetUnit(Unit unit)
    {
        //_currentUnit = unit;
        var color = CurrentState == State.OccupiedByPlayer ? ColorUnitOnCell : ColorEnemyOnCell;
        ChangeColor(color);
        //SetState()
    }

    public void SetState(State state) => CurrentState = state;

}