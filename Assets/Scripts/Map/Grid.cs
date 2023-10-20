using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Grid : MonoBehaviour
{
    [SerializeField] private UIPortraitManager UIPortraitManager;
    [SerializeField] private Transform TilesPlace;
    [SerializeField] private Tile TilePrefab;
    [SerializeField] private Vector2Int GridSize;
    [SerializeField] private float Offset;
    private Selector Selector { get; set; }

    public List<Unit> AllUnits { get; private set; }
    public Tile[,] Tiles { get; private set; }
    public Vector2Int GridSizeGet => GridSize;

    private void Awake()
    {
        Selector = GetComponentInChildren<Selector>();
        Tiles = new Tile[GridSizeGet.x, GridSizeGet.y];
    }

    public void StartCreating()
    {
        CreateGrid();
        LocateNeighborsTiles();
        GetAllExistedUnits();
        UIPortraitManager.AddPortraits(AllUnits);
        AddUnitsToTiles();
        GridUI.Instance.TurnManager.Launch();
        TrackEnemies();
    }

    private void TrackEnemies()
    {
        foreach (var unit in AllUnits)
            unit.TrackAllEnemies();
    }

    private void CreateGrid()
    {
        var tileSize = TilePrefab.GetComponent<MeshRenderer>().bounds.size;

        for (var x = 0; x < GridSizeGet.x; x++)
        for (var y = 0; y < GridSizeGet.y; y++)
        {
            // Чтобы сгенерировать клетку, нужно знать ее позицию.
            var position = new Vector3(x * (tileSize.x + Offset), 0, y * (tileSize.z + Offset));

            var tile = Instantiate(TilePrefab, position, Quaternion.identity, TilesPlace);
            tile.Initialize(x, y, true, false); // тут передается Grid

            Tiles[x, y] = tile;
        }
    }

    private void LocateNeighborsTiles()
    {
        foreach (var tile in Tiles)
            tile.Neighbors = Selector.PathConstructor.GetAvailableNeighbourTiles(tile); // Добавили левую часть.
    }

    private void GetAllExistedUnits()
    {
        // Получить всех персонажей Player и Enemy на сцене
        AllUnits = new List<Unit>();

        var players = FindObjectsOfType<Player>();
        var allies = FindObjectsOfType<Ally>();
        var enemies = FindObjectsOfType<Enemy>();

        // Добавить персонажей в список _allExistedUnits
        AllUnits.AddRange(players);
        AllUnits.AddRange(allies);
        AllUnits.AddRange(enemies);
    }

    private void AddUnitsToTiles()
    {
        foreach (var unit in AllUnits)
        {
            unit.InitializeUnit(Tiles);
        }
    }

    public void RemoveUnit(Unit unit)
    {
        var currentTile = unit.OccupiedTile;

        if (currentTile != null)
        {
            //currentTile.ClearUnit();
            AllUnits.Remove(unit);
            Destroy(unit.gameObject);
        }
    }

    public TileState GetStateAndCheckUnitOn(Unit unit, Tile tile)
    {
        return tile.State switch
        {
            TileState.OccupiedByEnemy when unit.Stats.Type == UnitType.Enemy => tile.State,
            TileState.OccupiedByPlayer when unit.Stats.Type == UnitType.Player => tile.State,
            _ => tile.State
        };
    }

    public bool CheckTileToUnitStandOn(Unit unit, Tile tile)
    {
        return tile.State == TileState.OccupiedByEnemy && unit.Stats.Type == UnitType.Player;
    }
}