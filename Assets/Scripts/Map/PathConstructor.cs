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


        Dictionary<Tile, float> gScore = new()
        {
            [startTile] = 0
        };

        Dictionary<Tile, float> fScore = new()
        {
            [startTile] = Heuristic(startTile, endTile)
        };

        HashSet<Tile> closedList = new();
        var openList = new SortedDictionary<float, List<Tile>>()
            { { fScore[startTile], new List<Tile> { startTile } } };
        Dictionary<Tile, Tile> cameFrom = new();

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

            foreach (var neighborTile in GetAvailableNeighbourTiles(currentTile)
                         .Where(neighborTile => !closedList.Contains(neighborTile)))
            {
                if (currentTile != null)
                {
                    var tentativeScore = gScore[currentTile] + UIManager.GetDistance(currentTile, neighborTile);

                    fScore.TryAdd(neighborTile, float.MaxValue);

                    if (!openList.ContainsKey(fScore[neighborTile]))
                        openList[fScore[neighborTile]] = new List<Tile> { neighborTile };
                    else if (tentativeScore >= (gScore.TryGetValue(neighborTile, out var gScoreNeighbor)
                                 ? gScoreNeighbor
                                 : float.MaxValue))
                        continue;

                    cameFrom[neighborTile] = currentTile;
                    gScore[neighborTile] = tentativeScore;
                }

                fScore[neighborTile] = gScore[neighborTile] + Heuristic(neighborTile, endTile);
                if (openList.ContainsKey(fScore[neighborTile]))
                    openList[fScore[neighborTile]].Remove(neighborTile);
                if (!openList.ContainsKey(fScore[neighborTile]))
                    openList[fScore[neighborTile]] = new List<Tile> { neighborTile };
                else
                    openList[fScore[neighborTile]].Add(neighborTile);
            }
        }

        _destinationTile = null;
        return new List<Tile>();
    }

    private float Heuristic(Tile currentTile, Tile endTile)
    {
        var dx = Math.Abs(currentTile.Coordinates.x - endTile.Coordinates.x);
        var dy = Math.Abs(currentTile.Coordinates.y - endTile.Coordinates.y);

        if (dx > dy)
            return 1.001f * dx + dy;
        return dx + 1.001f * dy;
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
                // ѕровер€ем, €вл€етс€ ли клетка доступной дл€ движени€
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
                    if (Tile.MovementCost <= remainingMoves)
                        queue.Enqueue((neighbour, remainingMoves - Tile.MovementCost));
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
}