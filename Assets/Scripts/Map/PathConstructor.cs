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

    public List<Tile> FindPathToTarget(Tile startCell, Tile endCell, out List<Tile> Path, Grid grid)
    {
        Path = new List<Tile>();

        // ������ ��������� ���� �� ��������� ������ �� ������� ������.
        Dictionary<Tile, float> gScore = new() 
        {
            [startCell] = 0
        };

        // ������ ������ ��������� ���� �� ��������� ������ ����� ������� ������ �� �������� ������ (������������� ������).
        Dictionary<Tile, float> fScore = new() 
        {
            [startCell] = Heuristic(startCell, endCell)
        };

        List<Tile> closedList = new(); // ������ �����, ������� ��� ���� ���������.
        List<Tile> openList = new() { startCell }; // ������ �����, ������� ����� ��������� (�������� ������ ������� ������).

        // ������ �����, ������ ������ � ������� ������. ��� �������� ����� ������������ ���� �� ��������� ������ �� ��������.
        Dictionary<Tile, Tile> cameFrom = new();

        while (openList.Count > 0)
        {
            var currentCell = openList.OrderBy(cell => fScore.TryGetValue(cell, out float value) ? value : float.MaxValue).FirstOrDefault();
            if (currentCell == endCell)
                return ReconstructPath(cameFrom, endCell, out Path);

            openList.Remove(currentCell);
            closedList.Add(currentCell);

            foreach (var neighborCell in GetNeighbourCells(currentCell, grid))
            {
                if (closedList.Contains(neighborCell))
                    continue;

                float tentativeScore = gScore[currentCell] + GetDistance(currentCell, neighborCell);

                if (!openList.Contains(neighborCell))
                    openList.Add(neighborCell);
                else if (tentativeScore >= (gScore.TryGetValue(neighborCell, out float gScoreNeighbor) ? gScoreNeighbor : float.MaxValue))
                    continue;

                cameFrom[neighborCell] = currentCell;
                gScore[neighborCell] = tentativeScore;
                fScore[neighborCell] = gScore[neighborCell] + Heuristic(neighborCell, endCell);
            }
        }

        return new List<Tile>();
    }

    private float Heuristic(Tile a, Tile b)
    {
        // ���������� ��������� �������������� ���������� ��� ������ ��������� ����
        return Mathf.Abs(a.Coordinates.x - b.Coordinates.x) + Mathf.Abs(a.Coordinates.y - b.Coordinates.y);
    }

    private List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile currentCell, out List<Tile> Path)
    {
        List<Tile> path = new() { currentCell };

        while (cameFrom.ContainsKey(currentCell))
        {
            currentCell = cameFrom[currentCell];
            path.Insert(0, currentCell);
        }

        Path = path;
        return Path;
    }

    public List<Tile> GetNeighbourCells(Tile cell, Grid grid)
        {
        List<Tile> neighbours = new();

        foreach (Direction direction in directions)
        {
            int coordinateX = Convert.ToInt32(cell.Coordinates.x + direction.XOffset);
            int coordinateY = Convert.ToInt32(cell.Coordinates.y + direction.YOffset);

            if (coordinateX >= 0 && coordinateX < grid.GridSize.x && coordinateY >= 0 && coordinateY < grid.GridSize.y)
            {
                var neighbour = grid.Cells[coordinateX, coordinateY];
                if (neighbour != null && neighbour != cell)
                    neighbours.Add(neighbour);
            }
        }

        cell.Neighbours = neighbours;
        return neighbours;
    }

    public float GetDistance(Tile cell1, Tile cell2)
    {
        // ���� ������ �����, �� ���������� ����� ���� ����� 0
        if (cell1 == cell2)
            return 0;

        // ����� �� ����� ������������ ����� �������� ��� ���������� ���������� ����� ��������.
        // ��������, ����� ������������ ��������� ����������:
        float dx = cell1.Coordinates.x - cell2.Coordinates.x;
        float dy = cell1.Coordinates.y - cell2.Coordinates.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
