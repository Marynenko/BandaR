using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Tile : MonoBehaviour
{
    #region Variables

    public int CostEnergy;
    
    // Private fields
    private MeshRenderer _meshRenderer;
    private bool _available;
    
    private readonly int _distance;

    // Public Properties fields
    public List<Tile> Neighbors { get; set; }

    #region Variable -> Available
    public bool Available { get => _available; set => SetAvailable(value); }
    private void SetAvailable(bool isAvailable)
    {
        _available = isAvailable;
        UnitOn = !_available;

        if (_available || !UnitOn)
        {
            State = TileState.Standard; // Состояние доступное
            ChangeColor(TileState.Standard);
        }
        else
        {
            ChangeColor(State);
        }
    }

    #endregion

    // Public fields
    public Vector2Int Coordinates; // Позиция Клетки.
    public TileState State; // Состояние клетки.
    public bool UnitOn; // Юнит на клетке или нет.

    public Color ColorTileStandard;
    public Color ColorPlayerOnTile;
    public Color ColorAllyOnTile;
    public Color ColorEnemyOnTile;
    public Color ColorSelectedTile;
    public Color ColorMovementTile;
    public Color ColorPossibleAttack;

    // Static fields
    private static Dictionary<TileState, Color> _stateColors;

    // Other variables
    internal const int MovementCost = 1;

    #endregion

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        
        _stateColors ??= new Dictionary<TileState, Color>()
        {
            { TileState.Standard, ColorTileStandard },
            { TileState.OccupiedByPlayer, ColorPlayerOnTile },
            { TileState.OccupiedByAlly, ColorAllyOnTile },
            { TileState.OccupiedByEnemy, ColorEnemyOnTile },
            { TileState.Selected, ColorSelectedTile },
            { TileState.Movement, ColorMovementTile },
            { TileState.AttackTarget, ColorPossibleAttack}
        };
    }
    
    public void Initialize(int row, int column, bool isAvailable, bool unitOn)
    {
        name = $"X: {row} Y: {column}";
        _available = isAvailable;
        UnitOn = unitOn;
        State = TileState.Standard;
        Coordinates = new Vector2Int(row, column);
        Neighbors = new List<Tile>(4);
        CostEnergy = 20;

        ChangeColor(State);
    }

    public void SelectTile()
    {
        Available = false;
    }

    public void UnselectTile()
    {
        UnitOn = false;
        Available = true;
        UIManager.Instance.GridUI.HighlightTiles(Neighbors, TileState.Standard);
    }

    public bool IsAvailable() => Available;
    
    public void ChangeColor(TileState state)
    {
        _meshRenderer.material.color = _stateColors[state];
    }
}

public enum TileState   
{
    Standard, 
    Selected, 
    Movement, 
    OccupiedByPlayer,
    OccupiedByAlly,
    OccupiedByEnemy,
    AttackTarget
}