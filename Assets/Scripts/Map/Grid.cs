using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Grid : MonoBehaviour
{
    [SerializeField] private UIPortraitManager UIPortraitManager;
    [FormerlySerializedAs("tilesPlace")] [SerializeField] private Transform TilesPlace;
    [FormerlySerializedAs("tilePrefab")] [SerializeField] private Tile TilePrefab;
    [FormerlySerializedAs("gridSize")] [SerializeField] private Vector2Int GridSize;
    [FormerlySerializedAs("offset")] [SerializeField] private float Offset;
    private Selector Selector { get; set; }

    public List<Unit> AllUnits { get; private set; }
    public Tile[,] Tiles { get; private set; }
    public Vector2Int GridSizeGet => GridSize;

    private void Awake()
    {
        Selector = GetComponentInChildren<Selector>();
        Tiles = new Tile[GridSizeGet.x, GridSizeGet.y];
    }

    //   3842 - Готово


    public void StartCreating()
    {
        CreateGrid();
        LocateNeighborsTiles();
        GetAllExistedUnits();
        UIPortraitManager.AddPortraits(AllUnits);
        AddUnitsToTiles();
        GridUI.Instance.TurnManager.Launch();
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
            tile.Neighbors = Selector.PathConstructor.GetNeighborTiles(tile); // Добавили левую часть.
    }

    private void GetAllExistedUnits()
    {
        // Получить всех персонажей Player и Enemy на сцене
        AllUnits = new List<Unit>();

        var players = FindObjectsOfType<Player>();
        // var allies = FindObjectsOfType<Ally>();
        var enemies = FindObjectsOfType<Enemy>();

        // Добавить персонажей в список _allExistedUnits
        AllUnits.AddRange(players);
        // AllUnits.AddRange(allies);
        AllUnits.AddRange(enemies);
    }

    private void AddUnitsToTiles()
    {
        foreach (var unit in AllUnits)
        {
            unit.InitializeUnit(Tiles, UIPortraitManager);
        }
    }

    public void ClearColorTiles()
    {
        foreach (var tile in Tiles)
            if (!tile.IsAvailable())
                GridUI.Instance.HighlightTile(tile, TileState.Standard);
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

        if (tile.State == TileState.OccupiedByEnemy && unit.Stats.Type == UnitType.Enemy)
            return tile.State;
        if (tile.State == TileState.OccupiedByPlayer && unit.Stats.Type == UnitType.Player)
            return tile.State;
        return tile.State;
    }

    public bool CheckTileToUnitStandOn(Unit unit, Tile tile)
    {
        if (tile.State == TileState.OccupiedByEnemy && unit.Stats.Type == UnitType.Player)
            return true;
        return false;
    }
}