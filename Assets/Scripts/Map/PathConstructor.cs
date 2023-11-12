using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PathConstructor : MonoBehaviour
{
    [SerializeField] private Grid Grid;
    private Tile _destinationTile;

    private readonly struct Direction
    {
        public int X { get; }
        public int Y { get; }

        public Direction(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    private readonly List<Direction> _direction = new()
    {
        new Direction(0, 1), // up
        new Direction(0, -1), // down
        new Direction(-1, 0), // left
        new Direction(1, 0) // right
    };

    public List<Tile> FindPathToTarget(Unit unit, Tile endTile)
    {
        if (unit == null)
        {
            return null;
        }

        var startTile = unit.OccupiedTile;
        _destinationTile = endTile;

        var gScore = new Dictionary<Tile, float>
        {
            [startTile] = 0
        };

        var fScore = new Dictionary<Tile, float>
        {
            [startTile] = UIManager.Heuristic(startTile, endTile)
        };

        var closedList = new HashSet<Tile>();
        var openList = new SortedDictionary<float, List<Tile>>
        {
            { fScore[startTile], new List<Tile> { startTile } }
        };
        var cameFrom = new Dictionary<Tile, Tile>();

        while (openList.Count > 0)
        {
            var currentTile = openList.First().Value[0];

            if (currentTile == endTile)
            {
                var path = ReconstructPath(cameFrom, endTile);
                _destinationTile = null;
                return path;
            }

            if (openList.First().Value.Count > 1)
                openList.First().Value.RemoveAt(0);
            else
                openList.Remove(openList.First().Key);
            closedList.Add(currentTile);

            var availableNeighbourTiles = GetAvailableNeighbourTiles(currentTile);
            foreach (var neighborTile in availableNeighbourTiles
                         .Where(neighborTile => !closedList.Contains(neighborTile)))
            {
                var tentativeScore = gScore[currentTile] + UIManager.GetDistance(currentTile, neighborTile);

                if (!gScore.ContainsKey(neighborTile))
                {
                    gScore[neighborTile] = float.MaxValue;
                }

                var isDistanceShorter = tentativeScore < gScore[neighborTile];
                if (!isDistanceShorter)
                {
                    continue;
                }

                cameFrom[neighborTile] = currentTile;
                gScore[neighborTile] = tentativeScore;
                fScore[neighborTile] = gScore[neighborTile] + UIManager.Heuristic(neighborTile, endTile);

                if (!openList.ContainsKey(fScore[neighborTile]))
                {
                    openList[fScore[neighborTile]] = new List<Tile> { neighborTile };
                }
                else
                {
                    openList[fScore[neighborTile]].Add(neighborTile);
                }
            }
        }
        //     foreach (var neighborTile in availableNeighbourTiles
            //                  .Where(neighborTile => !closedList.Contains(neighborTile)))
            //     {
            //         var tentativeScore = gScore[currentTile] + UIManager.GetDistance(currentTile, neighborTile);
            //
            //
            //         if (!gScore.ContainsKey(neighborTile))
            //         {
            //             gScore[neighborTile] = float.MaxValue;
            //         }
            //
            //         var isDistanceShorter = tentativeScore < gScore[neighborTile];
            //         if (!isDistanceShorter) continue;
            //
            //         cameFrom[neighborTile] = currentTile;
            //         gScore[neighborTile] = tentativeScore;
            //         fScore[neighborTile] = gScore[neighborTile] + UIManager.Heuristic(neighborTile, endTile);
            //
            //         if (!openList.ContainsKey(fScore[neighborTile]))
            //         {
            //             openList[fScore[neighborTile]] = new List<Tile> { neighborTile };
            //         }
            //         else
            //         {
            //             openList[fScore[neighborTile]].Add(neighborTile);
            //         }
            //     }

        // Если путь до целевого тайла не найден, выбираем ближайший доступный тайл
        var closestTile = GetClosestAvailableTile(startTile, endTile, closedList);
        if (closestTile != null)
        {
            var path = ReconstructPath(cameFrom, closestTile);
            _destinationTile = null;
            return path;
        }

        _destinationTile = null;
        return new List<Tile>();
    }

    private Tile GetClosestAvailableTile(Tile startTile, Tile endTile, HashSet<Tile> closedList)
    {
        var openList = new Queue<Tile>();
        openList.Enqueue(startTile);

        while (openList.Count > 0)
        {
            var currentTile = openList.Dequeue();

            if (currentTile == endTile)
            {
                return currentTile;
            }

            var availableNeighbourTiles = GetAvailableNeighbourTiles(currentTile);
            foreach (var neighborTile in availableNeighbourTiles
                         .Where(neighborTile => !closedList.Contains(neighborTile)))
            {
                openList.Enqueue(neighborTile);
            }
        }

        return null;
    }

    private List<Tile> ReconstructPath(IReadOnlyDictionary<Tile, Tile> cameFrom, Tile currentTile)
    {
        List<Tile> path = new() { currentTile };
        while (cameFrom.ContainsKey(currentTile))
        {
            currentTile = cameFrom[currentTile];
            path.Insert(0, currentTile);
        }

        return path;
    }

    public List<Tile> GetAvailableNeighbourTiles(Tile tile)
    {
        List<Tile> nearbyTiles = new();

        foreach (var direction in _direction)
        {
            var coordinate = new Vector2Int(Convert.ToInt32(tile.Coordinates.x + direction.X),
                Convert.ToInt32(tile.Coordinates.y + direction.Y));

            if (TryGetTile(coordinate, out var neighbor) && neighbor != tile)
            {
                // Проверяем, является ли клетка доступной для движения
                if (_destinationTile == null)
                {
                    if (neighbor.IsAvailable())
                        nearbyTiles.Add(neighbor);
                }
                else
                {
                    if (neighbor.IsAvailable() || neighbor == _destinationTile)
                        nearbyTiles.Add(neighbor);
                }
            }
        }

        tile.Neighbors = nearbyTiles;
        return nearbyTiles;
    }

    public List<Tile> GetNeighbours(Tile tile)
    {
        List<Tile> nearbyTiles = new();

        foreach (var direction in _direction)
        {
            var coordinate = new Vector2Int(Convert.ToInt32(tile.Coordinates.x + direction.X),
                Convert.ToInt32(tile.Coordinates.y + direction.Y));

            if (TryGetTile(coordinate, out var neighbor) && neighbor != tile)
            {
                nearbyTiles.Add(neighbor);
            }
        }

        tile.Neighbors = nearbyTiles;
        return nearbyTiles;
    }

    public IEnumerable<Tile> GetAvailableMoves(Tile tile, int maxMoves)
    {
        var visitedTiles = new HashSet<Tile>();
        var availableMoves = new List<Tile>();

        var queue = new Queue<(Tile, float)>();
        queue.Enqueue((tile, maxMoves));

        while (queue.Count > 0)
        {
            var (currentTile, remainingMoves) = queue.Dequeue();

            visitedTiles.Add(currentTile);
            availableMoves.Add(currentTile);

            if (remainingMoves <= 1)
                continue;

            var neighbourTiles = GetAvailableNeighbourTiles(currentTile);
            foreach (var neighbour in neighbourTiles)
            {
                var tileIsVisited = visitedTiles.Contains(neighbour);

                if (!tileIsVisited)
                {
                    if (neighbour.MovementCost <= remainingMoves)
                        queue.Enqueue((neighbour, remainingMoves - neighbour.MovementCost));
                }
            }
        }

        return availableMoves;
    }

    private bool TryGetTile(Vector2Int coordinate, out Tile tile)
    {
        if (coordinate.x >= 0 && coordinate.x < Grid.GridSizeGet.x && coordinate.y >= 0 &&
            coordinate.y < Grid.GridSizeGet.y)
        {
            tile = Grid.Tiles[coordinate.x, coordinate.y];
            return true;
        }

        tile = null;
        return false;
    }

    public List<Unit> GetEnemyFromNeighbours(Unit unit)
    {
        var neighbours = GetNeighbours(unit.OccupiedTile);
        var type = unit.Stats.Type;
        var enemyUnits = new List<Unit>();

        foreach (var neighbour in neighbours)
        {
            if (!neighbour.Available)
            {
                if (type == UnitType.Enemy &&
                    neighbour.State is TileState.OccupiedByPlayer or TileState.OccupiedByAlly)
                {
                    enemyUnits.Add(GetUnitFromNeighbour(neighbour));
                }
                else if (type != UnitType.Enemy && neighbour.State == TileState.OccupiedByEnemy)
                {
                    enemyUnits.Add(GetUnitFromNeighbour(neighbour));
                }
            }
        }

        return enemyUnits;
    }


    private Unit GetUnitFromNeighbour(Tile neighbour)
    {
        return Grid.AllUnits.FirstOrDefault(unit => unit.OccupiedTile == neighbour);
    }

    public void GetEnemies(Unit currentUnit)
    {
        // var firstEnemy = currentUnit.Enemies[0];
        // var distance = UIManager.GetDistance(currentUnit.OccupiedTile, firstEnemy.OccupiedTile);
        //
        // _targetEnemy = firstEnemy;
        //
        // foreach (var enemy in currentUnit.Enemies)
        // {
        //     var localDistance = UIManager.GetDistance(currentUnit.OccupiedTile, enemy.OccupiedTile);
        //
        //     if (!(distance > localDistance)) continue;
        //
        //     distance = localDistance;
        //     _targetEnemy = enemy;
        // }
    }

    private Dictionary<Unit, List<Tile>> GetPathsToEnemies(Unit unit)
    {
        // var enemies = GetEnemyFromNeighbours(unit);
        var enemies = unit.Enemies;
        var paths = new Dictionary<Unit, List<Tile>>();

        foreach (var enemy in enemies)
        {
            var path = FindPathToTarget(unit, enemy.OccupiedTile);
            paths[enemy] = path;
        }

        return paths;
    }

    public List<Tile> GetOptimalPath(Unit unit)
    {
        var paths = GetPathsToEnemies(unit);
        List<Tile> optimalPath = null;
        var shortestDistance = float.MaxValue;

        foreach (var path in paths)
        {
            var distance = path.Value.Sum(tile => tile.MovementCost);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                optimalPath = path.Value;
            }
        }

        return optimalPath;
    }
}