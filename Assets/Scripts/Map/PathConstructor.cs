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

    private readonly List<Direction> _direction;

    public List<Tile> FindPathToTarget(Tile startTile, Tile endTile, out List<Tile> path, Grid grid)
    {
        path = new List<Tile>();

        // хранит стоимость пути от начальной €чейки до текущей €чейки.
        Dictionary<Tile, float> gScore = new() // 
        {
            [startTile] = 0
        };

        // хранит оценку стоимости пути от начальной €чейки через текущую €чейку до конечной €чейки (эвристическа€ оценка).
        Dictionary<Tile, float> fScore = new() 
        {
            [startTile] = Heuristic(startTile, endTile)
        };

        HashSet<Tile> closedList = new(); // список €чеек, которые уже были проверены
        SortedList<float, Tile> openList = new() { { fScore[startTile], startTile } }; // список €чеек, которые нужно проверить (соседние €чейки текущей €чейки).

        // список €чеек, откуда пришли в текущую €чейку. Ёто позволит потом восстановить путь от начальной €чейки до конечной.
        Dictionary<Tile, Tile> cameFrom = new();

        while (openList.Count > 0)
        {
            var currentTile = openList.Values[0];
            if (currentTile == endTile)
                return ReconstructPath(cameFrom, endTile, out path);

            openList.RemoveAt(0);
            closedList.Add(currentTile);


            foreach(var neighborTile in GetNearbyTiles(currentTile, grid).Where(neighborTile => !closedList.Contains(neighborTile)))
            {
                if (currentTile != null)
                {
                    var tentativeScore = gScore[currentTile] + GetDistance(currentTile, neighborTile);

                    if (!openList.ContainsValue(neighborTile))
                        openList.Add(fScore[neighborTile], neighborTile);
                    else if (tentativeScore >= (gScore.TryGetValue(neighborTile, out var gScoreNeighbor) ? gScoreNeighbor : float.MaxValue))
                        continue;

                    cameFrom[neighborTile] = currentTile;
                    gScore[neighborTile] = tentativeScore;
                }

                fScore[neighborTile] = gScore[neighborTile] + Heuristic(neighborTile, endTile);
                if (openList.ContainsValue(neighborTile))
                    openList.Remove(openList.Keys[openList.IndexOfValue(neighborTile)]);
                openList.Add(fScore[neighborTile], neighborTile);
            }
        }

        return new List<Tile>();
    }

    // »спользуем эвристику ћанхэттенского рассто€ни€ дл€ оценки стоимости пути
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

            if (grid.TryGetTile(coordinate, out var neighbor) && neighbor != tile)
                nearbyTiles.Add(neighbor);
        }

        tile.Neighbors = nearbyTiles;
        return nearbyTiles;
    }

    public float GetDistance(Tile tileFrom, Tile tileTo)
    {
        // ≈сли €чейки равны, то рассто€ние между ними равно 0
        if (tileFrom == tileTo)
            return 0;

        // «десь мы можем использовать любой алгоритм дл€ вычислени€ рассто€ни€ между €чейками.
        // Ќапример, можно использовать евклидово рассто€ние:
        var dx = tileFrom.Coordinates.x - tileTo.Coordinates.x;
        var dy = tileFrom.Coordinates.y - tileTo.Coordinates.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
