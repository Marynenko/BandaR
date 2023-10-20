using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class Unit : SoundsManager
{
    #region Variables

    // Fields
    [SerializeField] private UnitStats _stats;

    // Constants
    private const float MAX_DISTANCE = 4f;
    private const float HEIGHT_TO_PUT_UNIT_ON_TILE = 0.68f;

    // Private fields

    // Public properties
    public UnitStats Stats => _stats;
    public UnitStatus Status { get; set; }
    public Tile OccupiedTile { get; private set; }
    public Tile OccupiedTileSet
    {
        set => OccupiedTile = value;
    }

    // public fields
    public Tile Target;
    public UISign Sign;

    public Vector2Int SpawnCellVector2Int;
    public HashSet<Tile> AvailableMoves;
    public bool UnitIsMoving;

    #endregion

    #region Initialization

    public abstract void TrackAllEnemies();

    public void InitializeUnit(Tile[,] tiles)
    {
        var startTile = CompareSpawnPosToTile(tiles);
        transform.position = startTile.transform.position + Vector3.up * HEIGHT_TO_PUT_UNIT_ON_TILE;
        OccupiedTile = startTile;

        GridUI.Instance.TurnManager.ShowPortrait(this); // off

        OccupiedTile.State = Stats.Type switch
        {
            UnitType.Player => TileState.OccupiedByPlayer,
            UnitType.Ally => TileState.OccupiedByAlly,
            _ => TileState.OccupiedByEnemy
        };

        OccupiedTile.Available = false;
        Status = UnitStatus.Unavailable;

        _stats.EnergyForMove = 40;
        _stats.EnergyForAttack = _stats.Energy - 40;
        _stats.CountAttacks = 1;
    }

    private Tile CompareSpawnPosToTile(Tile[,] tiles) =>
        tiles.Cast<Tile>().FirstOrDefault(tile => tile.Coordinates.x == SpawnCellVector2Int.x
                                                  && tile.Coordinates.y == SpawnCellVector2Int.y);
    #endregion
}