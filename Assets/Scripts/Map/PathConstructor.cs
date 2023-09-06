using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathConstructor : MonoBehaviour
{
    private readonly List<Direction> directions = new()
    {
        new Direction(0, 1),   // Up
        new Direction(0, -1),  // Down
        new Direction(-1, 0),  // Left
        new Direction(1, 0)    // Right
    };

    public List<Tile> FindPathToTarget(Tile startTile, Tile endTile, out List<Tile> Path, Grid grid)
    {
        Path = new List<Tile>();

        // ������ ��������� ���� �� ��������� ������ �� ������� ������.
        Dictionary<Tile, float> gScore = new() 
        {
            [startTile] = 0
        };

        // ������ ������ ��������� ���� �� ��������� ������ ����� ������� ������ �� �������� ������ (������������� ������).
        Dictionary<Tile, float> fScore = new() 
        {
            [startTile] = Heuristic(startTile, endTile)
        };

        List<Tile> closedList = new(); // ������ �����, ������� ��� ���� ���������.
        List<Tile> openList = new() { startTile }; // ������ �����, ������� ����� ��������� (�������� ������ ������� ������).

        // ������ �����, ������ ������ � ������� ������. ��� �������� ����� ������������ ���� �� ��������� ������ �� ��������.
        Dictionary<Tile, Tile> cameFrom = new();

        while (openList.Count > 0)
        {
            var currentTile = openList.OrderBy(tile => fScore.TryGetValue(tile, out float value) ? value : float.MaxValue).FirstOrDefault();
            if (currentTile == endTile)
                return ReconstructPath(cameFrom, endTile, out Path);

            openList.Remove(currentTile);
            closedList.Add(currentTile);

            foreach (var neighborTile in GetNeighbourTiles(currentTile, grid))
            {
                if (closedList.Contains(neighborTile))
                    continue;

                float tentativeScore = gScore[currentTile] + GetDistance(currentTile, neighborTile);

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

    private float Heuristic(Tile a, Tile b)
    {
        // ���������� ��������� �������������� ���������� ��� ������ ��������� ����
        return Mathf.Abs(a.Coordinates.x - b.Coordinates.x) + Mathf.Abs(a.Coordinates.y - b.Coordinates.y);
    }

    private List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile currentTile, out List<Tile> Path)
    {
        List<Tile> path = new() { currentTile };

        while (cameFrom.ContainsKey(currentTile))
        {
            currentTile = cameFrom[currentTile];
            path.Insert(0, currentTile);
        }

        Path = path;
        return Path;
    }

    public List<Tile> GetNeighbourTiles(Tile tile, Grid grid)
        {
        List<Tile> neighbours = new();

        foreach (Direction direction in directions)
        {
            int coordinateX = Convert.ToInt32(tile.Coordinates.x + direction.XOffset);
            int coordinateY = Convert.ToInt32(tile.Coordinates.y + direction.YOffset);

            if (coordinateX >= 0 && coordinateX < grid.GridSize.x && coordinateY >= 0 && coordinateY < grid.GridSize.y)
            {
                var neighbour = grid.Tiles[coordinateX, coordinateY];
                if (neighbour != null && neighbour != tile)
                    neighbours.Add(neighbour);
            }
        }

        tile.Neighbours = neighbours;
        return neighbours;
    }

    public float GetDistance(Tile tileFrom, Tile tileTo)
    {
        // ���� ������ �����, �� ���������� ����� ���� ����� 0
        if (tileFrom == tileTo)
            return 0;

        // ����� �� ����� ������������ ����� �������� ��� ���������� ���������� ����� ��������.
        // ��������, ����� ������������ ��������� ����������:
        float dx = tileFrom.Coordinates.x - tileTo.Coordinates.x;
        float dy = tileFrom.Coordinates.y - tileTo.Coordinates.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
