using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathConstructor : MonoBehaviour
{
    private Grid _grid;
    private Unit _currentUnit;
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

    private void OnEnable()
    {
        _grid = GetComponentInParent<Grid>();
    }

    public List<Tile> FindPathToTarget(Unit unit, Tile endTile, out List<Tile> path)
    {
        path = new List<Tile>();
        var startTile = unit.OccupiedTile;
        _currentUnit = unit;
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
                path = ReconstructPath(cameFrom, endTile);
                _destinationTile = null;
                return path;
            }

            if (openList.First().Value.Count > 1)
                openList.First().Value.RemoveAt(0);
            else
                openList.Remove(openList.First().Key);
            closedList.Add(currentTile);

            foreach (var neighborTile in GetNeighborTiles(currentTile)
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

    public IEnumerable<Tile> GetNeighborTiles(Tile tile)
    {
        List<Tile> nearbyTiles = new();
        // var tileOccupied = _grid.CheckTileToUnitStandOn(_currentUnit, tile);
        // if (tileOccupied)
        //     return nearbyTiles;

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

    public IEnumerable<Tile> GetAvailableMoves(Tile tile, int maxMoves)
    {
        var visitedTiles = new HashSet<Tile>();
        var availableMoves = new List<Tile>();

        var queue = new Queue<(Tile, int)>();
        queue.Enqueue((tile, maxMoves));

        while (queue.Count > 0)
        {
            var (currentTile, remainingMoves) = queue.Dequeue();

            visitedTiles.Add(currentTile);
            availableMoves.Add(currentTile);

            if (remainingMoves <= 1) continue;
            foreach (var neighbour in GetNeighborTiles(currentTile))
                if (!(visitedTiles.Contains(neighbour)))
                {
                    var cost = neighbour.MovementCost;
                    if (cost <= remainingMoves)
                        queue.Enqueue((neighbour, remainingMoves - cost));
                }
        }

        return availableMoves;
    }

    private bool TryGetTile(Vector2Int coordinate, out Tile tile)
    {
        if (coordinate.x >= 0 && coordinate.x < _grid.GridSizeGet.x && coordinate.y >= 0 &&
            coordinate.y < _grid.GridSizeGet.y)
        {
            tile = _grid.Tiles[coordinate.x, coordinate.y];
            return true;
        }
        else
        {
            tile = null;
            return false;
        }
    }

    // public float GetDistance(Tile tileFrom, Tile tileTo)
    // {
    //     if (tileFrom == tileTo)
    //         return 0;
    //     var dx = tileFrom.Coordinates.x - tileTo.Coordinates.x;
    //     var dy = tileFrom.Coordinates.y - tileTo.Coordinates.y;
    //     return Mathf.Sqrt(dx * dx + dy * dy);
    // }
}