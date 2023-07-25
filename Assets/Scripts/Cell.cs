using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private State _currentState; // Состояние клетки.
    private bool _available;
    private int _distance;

    public GridInteractor Interactor;
    public List<Cell> Neighbours { get; set; }
    public State CurrentState { get => _currentState; set => _currentState = value; }
    public bool Available { get => _available; set => SetAvailable(value); }

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
        Available = isAwailable;
        UnitOn = unitOn;
        CurrentState = State.Standard;
        Coordinates = new Vector2(row, column);
        Neighbours = new List<Cell>(4);  
    }

    public void SetAvailable(bool isAvailable)
    {
        _available = isAvailable;

        if (_available && !UnitOn)
        {
            _available = true;
            ChangeColor(ColorStandardCell);
        }
        else
        {
            _available = false;
            if (UnitOn)
            {
                if (CurrentState == State.OccupiedByPlayer)
                    ChangeColor(ColorUnitOnCell);
                else if (CurrentState == State.OccupiedByEnemy)
                    ChangeColor(ColorEnemyOnCell);
            }

            else
                ChangeColor(ColorMovementCell);
        }
    }

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
                if (neighbor.Available && neighbor.CurrentState != State.Impassable && neighbor.MovementCost <= movementPoints)
                {
                    neighbor.SetReachable(movementPoints - neighbor.MovementCost, isReachable);
                }
            }
        }
    }

    public void SelectCell()
    {
        //ChangeColor(ColorSelectedCell);
        UnitOn = true;
        Available = false; //true
    }

    public void UnselectCell()
    {
        // На момент когда я нажимаю на это, тогда у меня все выделения уже исчезли.
        ChangeColor(ColorStandardCell); // Меняю цвет
        UnitOn = false; // Игрока нет
        CurrentState = State.Reachable; // Состояние доступное
        Available = true;
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
        CurrentState = State.Reachable;
        Available = true;
    }

    public bool IsAvailableForUnit(Unit unit)
    {
        if (IsOccupied())
        {
            return false;
        }

        var distance = Vector3.Distance(unit.transform.position, transform.position);
        if (distance > unit.MovementPoints)
        {
            return false;
        }

        return true;
    }



    public bool IsOccupied()
    {
        if (CurrentState == State.OccupiedByEnemy || CurrentState == State.OccupiedByPlayer)
            return true;
        if (UnitOn == true || Available == false)
            return true;
        else
            return false;
    }
}

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