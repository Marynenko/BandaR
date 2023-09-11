using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Transform Parent;
    public Tile TilePrefab; 
    public Interactor Interactor { get; private set; }
    public GridGenerator Generator { get; private set; }
    public List<Unit> AllUnits { get; private set; }
    public Tile[,] Tiles { get; private set; }

    public Vector2Int GridSize;
    public float Offset;

    private void Awake()
    {
        Tiles = new Tile[GridSize.x, GridSize.y];
        AllUnits = new List<Unit>();
        Generator = GetComponent<GridGenerator>();
        Interactor = GetComponentInChildren<Interactor>();
        Interactor.enabled = true; // Включаем интерактор после инициализации сетки

    }
    public void CreateGrid()
    {
        var tileSize = TilePrefab.GetComponent<MeshRenderer>().bounds.size;

        for (var x = 0; x < GridSize.x; x++)
            for (var y = 0; y < GridSize.y; y++)
            {
                // Чтобы сгенерировать клетку, нужно знать ее позицию.
                var position = new Vector3(x * (tileSize.x + Offset), 0, y * (tileSize.z + Offset));

                var tile = Instantiate(TilePrefab, position, Quaternion.identity, Parent);
                tile.Initialize(x, y, Interactor, true, false); // тут передается Grid

                Tiles[x, y] = tile;
            }
    }

    public void LocateNeighborsTiles()
    {
        foreach (var tile in Tiles)
            tile.Neighbors = Interactor.PathConstructor.GetNearbyTiles(tile, this); // Добавили левую часть.
    }

    public void SetAvailableTiles()
    {
        foreach (var tile in Tiles)
            if (!tile.IsOccupied())
                tile.SetAvailable(true);
    }

    public void AddUnitsToTiles(List<Unit> units)
    {       
        foreach (var unit in units)
        {            
            var unitTileCoordinates = GetTileCoordinatesFromPosition(unit.transform.position);
            var tile = Tiles[unitTileCoordinates.x, unitTileCoordinates.y];

            if (unitTileCoordinates != Vector2Int.one * int.MaxValue)
            {
                unit.InitializeUnit(this, tile);
                // Добавить поиск ЮНИТОВ ПО КАРТЕ. ПРИДУМТЬ АЛГОРИТМ
                AllUnits.Add(unit);
            }
        }
    }

    public void RemoveUnit(Unit unit)
    {
        var unitToRemove = unit as Unit;
        var currentTile = unitToRemove.OccupiedTile;

        if (currentTile != null)
        {
            //currentTile.ClearUnit();
            AllUnits.Remove(unitToRemove);
            Destroy(unitToRemove.gameObject);
        }
    }

    public Vector2Int GetTileCoordinatesFromPosition(Vector3 position)
    {
        var x = Mathf.FloorToInt(position.x / TilePrefab.GetComponent<MeshRenderer>().bounds.size.x);
        var y = Mathf.FloorToInt(position.z / TilePrefab.GetComponent<MeshRenderer>().bounds.size.x);

        return new Vector2Int(x, y);
    }

    public bool TryGetTile(Vector2Int coordinate, out Tile tile)
    {
        if (coordinate.x >= 0 && coordinate.x < GridSize.x && coordinate.y >= 0 && coordinate.y < GridSize.y)
        {
            tile = Tiles[coordinate.x, coordinate.y];
            return true;
        }
        else
        {
            tile = null;
            return false;
        }
    }
}
