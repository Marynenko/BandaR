using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private State _currentState; // Состояние клетки.
    private bool _available;
    private int _distance;

    public GridInteractor Interactor;
    public List<Tile> Neighbours { get; set; }
    public State CurrentState { get => _currentState; set => _currentState = value; }
    public bool Available { get => _available; set => SetAvailable(value); }

    [HideInInspector] public Vector2 Coordinates; // Позиция Клетки.

    public bool UnitOn; // Юнит на клетке или нет.    

    public Color ColorStandardTile; //Стандартный цвет клетки.
    public Color ColorUnitOnTile; // Цвет клетки на которой стоит гл. герой.
    public Color ColorEnemyOnTile; // Цвет клетки на которой стоит враг.
    public Color ColorSelectedTile; // Цвет клетки - выбранной
    public Color ColorMovementTile; // Цвет клетки - Для движения

    internal readonly int MovementCost = 1;

    public void Initialize(int row, int column, GridInteractor gridInteractor, bool isAwailable, bool unitOn)
    {
        name = $"X: {row} Y: {column}";
        Interactor = gridInteractor;
        Available = isAwailable;
        UnitOn = unitOn;
        CurrentState = State.Standard;
        Coordinates = new Vector2(row, column);
        Neighbours = new List<Tile>(4);  
    }

    public void SetAvailable(bool isAvailable)
    {
        _available = isAvailable;

        if (_available && !UnitOn)
        {
            _available = true;
            ChangeColor(ColorStandardTile);
        }
        else
        {
            _available = false;
            if (UnitOn)
            {
                if (CurrentState == State.OccupiedByPlayer)
                    ChangeColor(ColorUnitOnTile);
                else if (CurrentState == State.OccupiedByEnemy)
                    ChangeColor(ColorEnemyOnTile);
            }
            else
                ChangeColor(ColorMovementTile);
        }
    }

    public void ChangeColor(Color color)    {
        GetComponent<MeshRenderer>().material.color = color;
    }

    public Vector3 GetTileSize() => GetComponent<MeshRenderer>().bounds.size;

    public void SetReachable(int movementPoints, bool isReachable)
    {
        if (movementPoints >= 0)
        {
            CurrentState = isReachable ? State.Reachable : State.Impassable;
            foreach (Tile neighbor in Neighbours)
                if (neighbor.Available && neighbor.CurrentState != State.Impassable && neighbor.MovementCost <= movementPoints)
                    neighbor.SetReachable(movementPoints - neighbor.MovementCost, isReachable);
        }
    }

    public void SelectTile()
    {
        UnitOn = true;
        Available = false; //true
    }

    public void UnselectTile()
    {
        // На момент когда я нажимаю на это, тогда у меня все выделения уже исчезли.
        UnitOn = false; // Игрока нет
        CurrentState = State.Reachable; // Состояние доступное
        Available = true;
        UnhighlightAvailableMoves();
    }

    public void UnhighlightAvailableMoves()
    {
        // Идем по всем клеткам на игровом поле
        foreach (var tile in Neighbours)
            tile.UnhighlightTile();
    }

    public void UnhighlightTile()
    {
        ChangeColor(ColorStandardTile);
        UnitOn = false;
        CurrentState = State.Reachable;
        Available = true;
    }

    public bool IsAvailableForUnit(Unit unit)
    {
        if (IsOccupied())
            return false;

        var distance = Vector3.Distance(unit.transform.position, transform.position);
        if (distance > unit.MovementPoints)
            return false;
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