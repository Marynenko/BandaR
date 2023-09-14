using System.Collections.Generic;
using UnityEngine;

public class TilesGrid : MonoBehaviour
{
    [SerializeField] private Transform _tilesPlace;
    // [SerializeField] private GameController _gameController;
    [SerializeField] private GameModel _gameModel;
    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private Vector2Int _gridSize;
    [SerializeField] private float _offset;

    public Interactor Interactor { get; private set; }
    public List<Unit> AllUnits { get; private set; }
    public Tile[,] Tiles { get; private set; }
    public Vector2Int GridSize => _gridSize;

    private void Awake()
    {
        Interactor = GetComponentInChildren<Interactor>();
        Tiles = new Tile[_gridSize.x, _gridSize.y];
    }
    
    public void StartCreating()
    {
        CreateGrid();
        LocateNeighborsTiles();
        GetAllExistedUnits();
        AddUnitsToTiles(); // передаем список AllExistedUnits вместо использования Grid.AllUnits
        _gameModel.StartGame();
    }

    public void CreateGrid()
    {
        var tileSize = _tilePrefab.GetComponent<MeshRenderer>().bounds.size;

        for (var x = 0; x < _gridSize.x; x++)
            for (var y = 0; y < _gridSize.y; y++)
            {
                // Чтобы сгенерировать клетку, нужно знать ее позицию.
                var position = new Vector3(x * (tileSize.x + _offset), 0, y * (tileSize.z + _offset));

                var tile = Instantiate(_tilePrefab, position, Quaternion.identity, _tilesPlace);
                tile.Initialize(x, y, Interactor, true, false); // тут передается Grid

                Tiles[x, y] = tile;
            }
    }

    public void LocateNeighborsTiles()
    {
        foreach (var tile in Tiles)
            tile.Neighbors = Interactor.PathConstructor.GetNearbyTiles(tile, this); // Добавили левую часть.
    }

    private void GetAllExistedUnits()
    {
        // Получить всех персонажей Player и Enemy на сцене
        var allUnits = AllUnits;
        AllUnits = new List<Unit>();

        var players = FindObjectsOfType<Player>();
        var enemies = FindObjectsOfType<Enemy>();

        // Добавить персонажей в список _allExistedUnits
        AllUnits.AddRange(players);
        AllUnits.AddRange(enemies);
    }

    public void AddUnitsToTiles()
    {
        foreach (var unit in AllUnits)
        {
            var unitTileCoordinates = GetTileCoordinatesFromPosition(unit.transform.position);
            var tile = Tiles[unitTileCoordinates.x, unitTileCoordinates.y];

            if (unitTileCoordinates != Vector2Int.one * int.MaxValue)
            {
                unit.InitializeUnit(tile);
            }
        }
    }

    public Vector2Int GetTileCoordinatesFromPosition(Vector3 position)
    {
        var x = Mathf.FloorToInt(position.x / _tilePrefab.GetComponent<MeshRenderer>().bounds.size.x);
        var y = Mathf.FloorToInt(position.z / _tilePrefab.GetComponent<MeshRenderer>().bounds.size.x);

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
    
    public void SetAvailableTiles()
    {
        foreach (var tile in Tiles)
            if (!tile.IsOccupied())
                tile.SetAvailable(true);
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
}

