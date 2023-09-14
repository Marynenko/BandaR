using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    #region Variables

    // Serialized fields
    [SerializeField] private MeshRenderer _meshRenderer;
    // [SerializeField] private Material _material;

    // Private fields
    private bool _available;
    private readonly int _distance;

    // Public Properties fields
    public List<Tile> Neighbors { get; set; }

    #region Variable -> Available
    public bool Available { get => _available; set => SetAvailable(value); }
    public void SetAvailable(bool isAvailable)
    {
        //if (_available != isAvailable)
        //{

        //}

        if (_available && !UnitOn)
        {
            _available = isAvailable;
            State = TileState.Standard; // Состояние доступное
            ChangeColor(TileState.Standard);
            UnitOn = false; // Игрока нет
        }
        else
        {
            //if (State == TileState.OccupiedByPlayer)
            //    ChangeColor(TileState.OccupiedByPlayer);
            //else if (State == TileState.OccupiedByEnemy)
            //    ChangeColor(ColorEnemyOnTile);
            ChangeColor(UnitOn ? State : TileState.Movement);
        }
    }
    #endregion

    // Public fields
    public Vector2 Coordinates; // Позиция Клетки.
    public Interactor Interactor;
    public TileState State; // Состояние клетки.
    public Passability Passability;
    public bool UnitOn; // Юнит на клетке или нет.

    public Color TileColorStandard;
    public Color ColorPlayerOnTile;
    public Color ColorEnemyOnTile;
    public Color ColorSelectedTile;
    public Color ColorMovementTile;

    public Color CurrentColor;
    
    // Static fields
    public static Dictionary<TileState, Color> StateColors;

    // Other variables
    internal readonly int MovementCost = 1;

    // Delegates
    public delegate void TileEvent(Tile tile);

    // Events
    public event TileEvent OnTileSelected;
    public event TileEvent OnTileDeselected;

    #endregion

    private void Awake()
    {
        StateColors ??= new Dictionary<TileState, Color>()
        {
            { TileState.Standard, TileColorStandard },
            { TileState.OccupiedByPlayer, ColorPlayerOnTile },
            { TileState.OccupiedByEnemy, ColorEnemyOnTile },
            { TileState.Selected, ColorSelectedTile },
            { TileState.Movement, ColorMovementTile }
        };
    }

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

        ChangeColor(State);
        CurrentColor = StateColors[State];
    }

    public void SelectTile()
    {
        UnitOn = true;
        Available = false; //true
        //Passability = Passability.Impassable;
        OnTileSelected?.Invoke(this);
    }

    public void UnselectTile()
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

    public void ChangeColor(TileState state)
    {
        _meshRenderer.material.color = StateColors[state];
        // _material.color = StateColors[state];


        //if (StateColors.(state, out var color))
        //{
        //    _meshRenderer.material.color = color;
        //}
    }
}

public enum Passability
{
    Impassable, // Непроходимый тайл (например, стена или вода)
    Passable // Тайл, на который юнит может сделать ход (если это необходимо в вашей игре)
}

public enum TileState
{
    Standard, // Ńňŕíäŕđňíîĺ ńîńňî˙íčĺ
    Selected, // Âűáđŕí ďîëüçîâŕňĺëĺě
    Movement, // Ďîëüçîâŕňĺëü âűáđŕë ýňîň ňŕéë äë˙ äâčćĺíč˙ ţíčňŕ
    OccupiedByPlayer, // Çŕí˙ň čăđîęîě
    OccupiedByEnemy, // Çŕí˙ň âđŕăîě
}