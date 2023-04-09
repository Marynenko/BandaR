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

    public List<Cell> FindPathToTarget(Cell startCell, Cell endCell, out List<Cell> Path, Grid grid)
    {
        Path = new List<Cell>();

        Dictionary<Cell, float> gScore = new()
        {
            [startCell] = 0
        };

        Dictionary<Cell, float> fScore = new()
        {
            [startCell] = Heuristic(startCell, endCell)
        };

        List<Cell> closedList = new(); // список ячеек, которые уже были проверены.
        List<Cell> openList = new() { startCell }; // список ячеек, которые еще не были проверены.

        Dictionary<Cell, Cell> cameFrom = new();

        while (openList.Count > 0)
        {
            var currentCell = openList.OrderBy(cell => fScore.TryGetValue(cell, out float value) ? value : float.MaxValue).FirstOrDefault();


            if (currentCell == endCell)
            {
                return ReconstructPath(cameFrom, endCell, out Path);
            }

            openList.Remove(currentCell);
            closedList.Add(currentCell);

            foreach (var neighborCell in GetNeighbourCells(currentCell, grid))
            {
                if (closedList.Contains(neighborCell))
                {
                    continue;
                }

                float tentativeScore = gScore[currentCell] + GetDistanceBetweenCells(currentCell, neighborCell);

                if (!openList.Contains(neighborCell))
                {
                    openList.Add(neighborCell);
                }
                else if (tentativeScore >= (gScore.TryGetValue(neighborCell, out float gScoreNeighbor) ? gScoreNeighbor : float.MaxValue))

                {
                    continue;
                }

                cameFrom[neighborCell] = currentCell;
                gScore[neighborCell] = tentativeScore;
                fScore[neighborCell] = gScore[neighborCell] + Heuristic(neighborCell, endCell);
            }
        }

        return new List<Cell>();
    }

    private float Heuristic(Cell a, Cell b)
    {
        // Используем эвристику Манхэттенского расстояния для оценки стоимости пути
        return Mathf.Abs(a.Coordinates.x - b.Coordinates.x) + Mathf.Abs(a.Coordinates.y - b.Coordinates.y);
    }

    private List<Cell> ReconstructPath(Dictionary<Cell, Cell> cameFrom, Cell currentCell, out List<Cell> Path)
    {
        List<Cell> path = new() { currentCell };

        while (cameFrom.ContainsKey(currentCell))
        {
            currentCell = cameFrom[currentCell];
            path.Insert(0, currentCell);
        }

        Path = path;
        return Path;
    }

    public List<Cell> GetNeighbourCells(Cell cell, Grid grid)
    {
        List<Cell> neighbours = new();

        foreach (Direction direction in directions)
        {
            int neighbourX = cell.Row + direction.XOffset;
            int neighbourY = cell.Column + direction.YOffset;

            if (neighbourX >= 0 && neighbourX < grid.GridSize.x && neighbourY >= 0 && neighbourY < grid.GridSize.y)
            {
                var neighbour = grid.Cells[neighbourX, neighbourY];
                if (neighbour != null && neighbour != cell)
                {
                    neighbours.Add(neighbour);
                }
            }
        }

        return neighbours;
    }

    private float GetDistanceBetweenCells(Cell cell1, Cell cell2)
    {
        // Если ячейки равны, то расстояние между ними равно 0
        if (cell1 == cell2)
            return 0;

        // Здесь мы можем использовать любой алгоритм для вычисления расстояния между ячейками.
        // Например, можно использовать евклидово расстояние:
        float dx = cell1.Coordinates.x - cell2.Coordinates.x;
        float dy = cell1.Coordinates.y - cell2.Coordinates.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
