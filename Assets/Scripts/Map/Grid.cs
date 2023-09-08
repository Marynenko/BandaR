using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public Transform Parent;
    public Tile TilePrefab; 
    public GridInteractor Interactor { get; private set; }
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
        Interactor = GetComponentInChildren<GridInteractor>();
        Interactor.enabled = true; // Включаем интерактор после инициализации сетки

    }
    public void CreateGrid()
    {
        var TileSize = TilePrefab.GetComponent<MeshRenderer>().bounds.size;

        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
            {
                // Чтобы сгенерировать клетку, нужно знать ее позицию.
                var position = new Vector3(x * (TileSize.x + Offset), 0, y * (TileSize.z + Offset));

                var tile = Instantiate(TilePrefab, position, Quaternion.identity, Parent);
                tile.Initialize(x, y, Interactor, true, false); // тут передается Grid

                Tiles[x, y] = tile;
            }
    }

    public void LocateNeighboursTiles()
    {
        foreach (var tile in Tiles)
            tile.Neighbours = Interactor.PathConstructor.GetNeighbourTiles(tile, this); // Добавли левую часть.
    }

    public void SetAvaialableTiles()
    {
        foreach (var tile in Tiles)
            if (!tile.IsOccupied())
                tile.SetAvailable(true);
    }

    public void AddUnitsToTiles(List<Unit> units)
    {       
        foreach (var unit in units)
        {            
            Vector2Int unitTileCoordinates = GetTileCoordinatesFromPosition(unit.transform.position);
            Tile tile = Tiles[unitTileCoordinates.x, unitTileCoordinates.y];

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
        Tile currentTile = unitToRemove.OccupiedTile;

        if (currentTile != null)
        {
            //currentTile.ClearUnit();
            AllUnits.Remove(unitToRemove);
            Destroy(unitToRemove.gameObject);
        }
    }

    public Vector2Int GetTileCoordinatesFromPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / TilePrefab.GetComponent<MeshRenderer>().bounds.size.x);
        int y = Mathf.FloorToInt(position.z / TilePrefab.GetComponent<MeshRenderer>().bounds.size.x);

        return new Vector2Int(x, y);
    }

}
