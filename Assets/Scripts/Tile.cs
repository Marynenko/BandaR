using System;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    #region Variables
    // Serialized fields
    [SerializeField] private MeshRenderer _meshRenderer;

    // Private fields
    private bool _available;    
    private readonly int _distance;

    // Public Properties fields
    public List<Tile> Neighbors { get; set; }

    #region Variable -> Available
    public bool Available { get => _available; set => SetAvailable(value); }
    public void SetAvailable(bool isAvailable)
    {
        if (_available != isAvailable)
        {
            _available = isAvailable;

            if (_available && !UnitOn)
            {
                ChangeColor(ColorStandardTile);
                UnitOn = false; // Игрока нет
                State = TileState.Standard; // Состояние доступное
                //Passability = Passability.Passable; // Могу встать сюда
            }
            else
            {
                if (UnitOn)
                {
                    if (State == TileState.OccupiedByPlayer)
                        ChangeColor(ColorUnitOnTile);
                    else if (State == TileState.OccupiedByEnemy)
                        ChangeColor(ColorEnemyOnTile);
                }
                else
                    ChangeColor(ColorMovementTile);
            }
        }
    }
    #endregion

    // Public fields
    public Vector2 Coordinates; // Позиция Клетки.
    public Interactor Interactor;
    public TileState State; // Состояние клетки.
    public Passability Passability;
    public bool UnitOn; // Юнит на клетке или нет.
    
    public Color ColorStandardTile; //Стандартный цвет клетки.
    public Color ColorUnitOnTile; // Цвет клетки на которой стоит гл. герой.
    public Color ColorEnemyOnTile; // Цвет клетки на которой стоит враг.
    public Color ColorSelectedTile; // Цвет клетки - выбранной
    public Color ColorMovementTile; // Цвет клетки - Для движения

    // Other variables
    internal readonly int MovementCost = 1;

    // Delegates
    public delegate void TileEvent(Tile tile);

    // Events
    public event TileEvent OnTileSelected;
    public event TileEvent OnTileDeselected;

    #endregion

    public void Initialize(int row, int column, Interactor interactor, bool isAvailable, bool unitOn)
    {
        name = $"X: {row} Y: {column}";
        Interactor = interactor;
        _available = isAvailable;
        UnitOn = unitOn;
        State = TileState.Standard;
        Passability = Passability.Passable;
        Coordinates = new Vector2(row, column);
        Neighbors = new List<Tile>(4);  
    }

    public void SelectTile()
    {
        UnitOn = true;
        Available = false; //true
        //Passability = Passability.Impassable;
        OnTileSelected?.Invoke(this);
    }

    public void DeselectTile()
    {
        Available = true;
        UnhighlightAvailableMoves();
        OnTileDeselected?.Invoke(this);
    }

    public void UnhighlightAvailableMoves()
    {
        // Идем по всем клеткам на игровом поле
        foreach (var tile in Neighbors)
            tile.UnhighlightTile();
    }

    public void UnhighlightTile() => Available = true;

    public bool IsAvailableForUnit(Unit unit) =>
        !IsOccupied() && Vector3.Distance(unit.transform.position, transform.position) <= unit.MovementPoints;

    public bool IsOccupied() => 
        State == TileState.OccupiedByEnemy || State == TileState.OccupiedByPlayer || !_available || UnitOn;

    public void ChangeColor(Color color)    
    {
        _meshRenderer.material.color = color;
    }   
}

public enum TileState
{
    Standard, // Стандартное состояние
    //Selected, // Выбран пользователем
    Movement, // Пользователь выбрал этот тайл для движения юнита
    OccupiedByPlayer, // Занят игроком
    OccupiedByEnemy, // Занят врагом
}

public enum Passability
{
    Impassable, // Непроходимый тайл (например, стена или вода)
    Passable // Тайл, на который юнит может сделать ход (если это необходимо в вашей игре)
}