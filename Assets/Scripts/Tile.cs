﻿using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Tile : MonoBehaviour
{
    #region Variables

    // Private fields
    private MeshRenderer _meshRenderer;
    private bool _available;
    private readonly int _distance;

    // Public Properties fields
    public IEnumerable<Tile> Neighbors { get; set; }

    #region Variable -> Available
    public bool Available { get => _available; set => SetAvailable(value); }
    public void SetAvailable(bool isAvailable)
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
            // switch (State)
            // {
            //     case (TileState.OccupiedByPlayer):
            //         ChangeColor(State);
            //         break;
            //     case (TileState.OccupiedByEnemy):
            //         ChangeColor(State);
            //         break;
            //     default:
            //         ChangeColor(State);
            //         break;
            // }

            // ChangeColor(UnitOn ? State : TileState.Movement);
        }
    }

    #endregion

    // Public fields
    public Vector2 Coordinates; // Позиция Клетки.
    public TileState State; // Состояние клетки.
    public bool UnitOn; // Юнит на клетке или нет.

    public Color TileColorStandard;
    public Color ColorPlayerOnTile;
    public Color ColorEnemyOnTile;
    public Color ColorSelectedTile;
    public Color ColorMovementTile;

    // Static fields
    private static Dictionary<TileState, Color> _stateColors;

    // Other variables
    internal readonly int MovementCost = 1;
    #endregion

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        
        _stateColors ??= new Dictionary<TileState, Color>()
        {
            { TileState.Standard, TileColorStandard },
            { TileState.OccupiedByPlayer, ColorPlayerOnTile },
            { TileState.OccupiedByEnemy, ColorEnemyOnTile },
            { TileState.Selected, ColorSelectedTile },
            { TileState.Movement, ColorMovementTile }
        };
    }

    public void Initialize(int row, int column, bool isAvailable, bool unitOn)
    {
        name = $"X: {row} Y: {column}";
        _available = isAvailable;
        UnitOn = unitOn;
        State = TileState.Standard;
        Coordinates = new Vector2(row, column);
        Neighbors = new List<Tile>(4);

        ChangeColor(State);
    }

    public void SelectTile()
    {
        UnitOn = true;
        Available = false; //true
    }

    public void UnselectTile()
    {
        UnitOn = false;
        Available = true; // TODO выяснить нужно это или GridUI.HighlightTile?
        GridUI.HighlightTiles(Neighbors, TileState.Standard);
    }

    public bool IsOccupied() =>
        State == TileState.OccupiedByEnemy || State == TileState.OccupiedByPlayer || !_available || UnitOn;

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
    OccupiedByEnemy
}