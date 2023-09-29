using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Grid : MonoBehaviour
{
    [SerializeField] private Transform tilesPlace;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Vector2Int gridSize;
    [SerializeField] private float offset;

    public Selector Selector { get; private set; }
    public List<Unit> AllUnits { get; private set; }
    public Tile[,] Tiles { get; private set; }
    public Vector2Int GridSize => gridSize;

    private void Awake()
    {
        Selector = GetComponentInChildren<Selector>();
        Tiles = new Tile[gridSize.x, gridSize.y];
    }

    public void StartCreating()
    {
        CreateGrid();
        LocateNeighborsTiles();
        GetAllExistedUnits();
        AddUnitsToTiles();
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
            if (!tile.IsOccupied())
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
}