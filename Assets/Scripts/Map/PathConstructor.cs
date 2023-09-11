using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathConstructor : MonoBehaviour
{
    public readonly struct Direction
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
        new Direction(0, 1),  // Вверх
        new Direction(0, -1), // Вниз
        new Direction(-1, 0), // Влево
        new Direction(1, 0)   // Вправо
    };

    public List<Tile> FindPathToTarget(Tile startTile, Tile endTile, out List<Tile> Path, Grid grid)
    {
        Path = new List<Tile>();

        Dictionary<Tile, float> gScore = new()
        {
            [startTile] = 0
        };

        
        Dictionary<Tile, float> fScore = new()
        {
            [startTile] = Heuristic(startTile, endTile)
        };

        List<Tile> closedList = new();
        List<Tile> openList = new() { startTile }; 

        
        Dictionary<Tile, Tile> cameFrom = new();

        while (openList.Count > 0)
        {
            var currentTile = openList.OrderBy(tile => fScore.TryGetValue(tile, out var value) ? value : float.MaxValue).FirstOrDefault();
            if (currentTile == endTile)
                return ReconstructPath(cameFrom, endTile, out Path);

            openList.Remove(currentTile);
            closedList.Add(currentTile);

            foreach (var neighborTile in GetNearbyTiles(currentTile, grid))
            {
                if (closedList.Contains(neighborTile))
                    continue;

                var tentativeScore = gScore[currentTile] + GetDistance(currentTile, neighborTile);

                if (!openList.Contains(neighborTile))
                    openList.Add(neighborTile);
                else if (tentativeScore >= (gScore.TryGetValue(neighborTile, out float gScoreNeighbor) ? gScoreNeighbor : float.MaxValue))
                    continue;

                cameFrom[neighborTile] = currentTile;
                gScore[neighborTile] = tentativeScore;
                fScore[neighborTile] = gScore[neighborTile] + Heuristic(neighborTile, endTile);
            }
        }

        return new List<Tile>();
    }

    //public List<Tile> FindPathToTarget(Tile startTile, Tile endTile, out List<Tile> path, Grid grid)
    //{
    //    path = new List<Tile>();

    //    Dictionary<Tile, float> gScore = new()
    //    {
    //        [startTile] = 0
    //    };

    //    Dictionary<Tile, float> fScore = new()
    //    {
    //        [startTile] = Heuristic(startTile, endTile)
    //    };

    //    HashSet<Tile> closedList = new();
    //    SortedList<float, Tile> openList = new() { { fScore[startTile], startTile } };
    //    Dictionary<Tile, Tile> cameFrom = new();

    //    while (openList.Count > 0)
    //    {
    //        var currentTile = openList.Values[0];
    //        if (currentTile == endTile)
    //            return ReconstructPath(cameFrom, endTile, out path);

    //        openList.RemoveAt(0);
    //        closedList.Add(currentTile);

    //        foreach (var neighborTile in GetNearbyTiles(currentTile, grid).Where(neighborTile => !closedList.Contains(neighborTile)))
    //        {
    //            if (currentTile != null)
    //            {
    //                var tentativeScore = gScore[currentTile] + GetDistance(currentTile, neighborTile);

    //                if (!fScore.ContainsKey(neighborTile))
    //                    fScore[neighborTile] = float.MaxValue;

    //                if (!openList.ContainsValue(neighborTile))
    //                    openList.Add(fScore[neighborTile], neighborTile);
    //                else if (tentativeScore >= (gScore.TryGetValue(neighborTile, out var gScoreNeighbor) ? gScoreNeighbor : float.MaxValue))
    //                    continue;

    //                cameFrom[neighborTile] = currentTile;
    //                gScore[neighborTile] = tentativeScore;
    //            }

    //            fScore[neighborTile] = gScore[neighborTile] + Heuristic(neighborTile, endTile);
    //            if (openList.ContainsValue(neighborTile))
    //                openList.Remove(openList.Keys[openList.IndexOfValue(neighborTile)]);
    //            openList.Add(fScore[neighborTile], neighborTile);
    //        }
    //    }

    //    return new List<Tile>();
    //}


    // Используем эвристику Манхэттенского расстояния для оценки стоимости пути
    private float Heuristic(Tile a, Tile b) =>
        Mathf.Abs(a.Coordinates.x - b.Coordinates.x) + Mathf.Abs(a.Coordinates.y - b.Coordinates.y);


    private List<Tile> ReconstructPath(IReadOnlyDictionary<Tile, Tile> cameFrom, Tile currentTile, out List<Tile> resultPath)
    {
        List<Tile> path = new() { currentTile };

        while (cameFrom.ContainsKey(currentTile))
        {
            currentTile = cameFrom[currentTile];
            path.Insert(0, currentTile);
        }

        resultPath = path;
        return resultPath;
    }

    public List<Tile> GetNearbyTiles(Tile tile, Grid grid)
    {
        List<Tile> nearbyTiles = new();

        foreach (var direction in _direction)
        {
            var coordinate = new Vector2Int(Convert.ToInt32(tile.Coordinates.x + direction.X),
                Convert.ToInt32(tile.Coordinates.y + direction.Y));

            if (grid.Generator.TryGetTile(coordinate, out var neighbor) && neighbor != tile)
                nearbyTiles.Add(neighbor);
        }

        tile.Neighbors = nearbyTiles;
        return nearbyTiles;
    }

    public float GetDistance(Tile tileFrom, Tile tileTo)
    {
        // Если ячейки равны, то расстояние между ними равно 0
        if (tileFrom == tileTo)
            return 0;

        // Здесь мы можем использовать любой алгоритм для вычисления расстояния между ячейками.
        // Например, можно использовать евклидово расстояние:
        var dx = tileFrom.Coordinates.x - tileTo.Coordinates.x;
        var dy = tileFrom.Coordinates.y - tileTo.Coordinates.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
