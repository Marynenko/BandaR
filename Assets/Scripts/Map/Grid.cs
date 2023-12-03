using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private Selector Selector;
    [SerializeField] private UIPortraitManager UIPortraitManager;
    [SerializeField] private Transform TilesPlace;
    [SerializeField] private Tile TilePrefab;
    [SerializeField] private Vector2Int GridSize;
    [SerializeField] private float Offset;

    public List<Unit> AllUnits { get; private set; }
    public Tile[,] Tiles { get; private set; }
    public Vector2Int GridSizeGet => GridSize;

    private void Awake()
    {
        Tiles = new Tile[GridSizeGet.x, GridSizeGet.y];
    }

    public void UpdateAllUnits()
    {
        Unit unitToRemove = null;

        foreach (var unit in AllUnits.Where(unit => unit.Stats.Health <= 0))
        {
            unitToRemove = unit;
            break;
        }

        if (unitToRemove != null)
            AllUnits.Remove(unitToRemove);
    }

    public void StartCreating()
    {
        CreateGrid();
        LocateNeighborsTiles();
        GetAllExistedUnits();
        UIPortraitManager.AddPortraits(AllUnits);
        AddUnitsToTiles();
        UIManager.Instance.TurnManager.Launch();
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
            tile.Neighbors = UIManager.Instance.PathConstructor.GetAvailableAdjacentTiles(tile);
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
        
        AllUnits.Sort((x, y) => y.Stats.Speed.CompareTo(x.Stats.Speed));
    }

    private void AddUnitsToTiles()
    {
        foreach (var unit in AllUnits)
        {
            unit.InitializeUnit(Tiles);
        }
    }
}