﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class Grid : MonoBehaviour
{
    [SerializeField] private Transform tilesPlace;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private float offset;
    private Selector Selector { get; set; }
    
    public List<Unit> AllUnits { get; private set; }
    public Tile[,] Tiles { get; private set; }
    public Vector2Int GridSize => gridSize;

    private void Awake()
    {
        Selector = GetComponentInChildren<Selector>();
        Tiles = new Tile[gridSize.x, gridSize.y];
    }
    
    //   3842 - Готово


    public void StartCreating()
    {
        CreateGrid();
        LocateNeighborsTiles();
        GetAllExistedUnits();
        AddUnitsToTiles();
        GridUI.Instance.TurnManager.Launch();
    }

    private void CreateGrid()
    {
        var tileSize = tilePrefab.GetComponent<MeshRenderer>().bounds.size;

        for (var x = 0; x < gridSize.x; x++)
        for (var y = 0; y < gridSize.y; y++)
        {
            // Чтобы сгенерировать клетку, нужно знать ее позицию.
            var position = new Vector3(x * (tileSize.x + offset), 0, y * (tileSize.z + offset));

            var tile = Instantiate(tilePrefab, position, Quaternion.identity, tilesPlace);
            tile.Initialize(x, y, true, false); // тут передается Grid

            Tiles[x, y] = tile;
        }
    }

    private void LocateNeighborsTiles()
    {
        foreach (var tile in Tiles)
            tile.Neighbors = Selector.PathConstructor.GetNeighborTiles(tile); // Добавили левую часть.
    }

    private void GetAllExistedUnits()
    {
        // Получить всех персонажей Player и Enemy на сцене
        AllUnits = new List<Unit>();

        var players = FindObjectsOfType<Player>();
        var enemies = FindObjectsOfType<Enemy>();

        // Добавить персонажей в список _allExistedUnits
        AllUnits.AddRange(players);
        AllUnits.AddRange(enemies);
    }

    private void AddUnitsToTiles()
    {
        foreach (var unit in AllUnits)
        {
            unit.InitializeUnit(Tiles);
        }
    }

    public void SetAvailableTiles()
    {
        foreach (var tile in Tiles)
            if (!tile.IsAvailable())
                tile.SetAvailable(true);
    }

    public void RemoveUnit(Unit unit)
    {
        var unitToRemove = unit;
        var currentTile = unitToRemove.OccupiedTile;

        if (currentTile != null)
        {
            //currentTile.ClearUnit();
            AllUnits.Remove(unitToRemove);
            Destroy(unitToRemove.gameObject);
        }
    }

    public TileState GetStateAndCheckUnitOn(Unit unit, Tile tile)
    {
        // return tile.State switch
        // {
        //     TileState.OccupiedByEnemy when unit.Type == UnitType.Enemy => tile.State,
        //     TileState.OccupiedByPlayer when unit.Type == UnitType.Player => tile.State,
        //     _ => tile.State
        // };
        
        if (tile.State == TileState.OccupiedByEnemy && unit.Type == UnitType.Enemy)
            return tile.State;
        if (tile.State == TileState.OccupiedByPlayer && unit.Type == UnitType.Player)
            return tile.State;
        return tile.State;
    }
    
    
    public bool CheckTileToUnitStandOn(Unit unit, Tile tile)
    {
        if (tile.State == TileState.OccupiedByEnemy && unit.Type == UnitType.Player)
            return true;
        return false;
    }
}