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

    public List<Cell> FindPathToTarget(Cell startCell, Cell endCell, out List<Cell> Path, Grid grid)
    {
        Path = new List<Cell>();

        // хранит стоимость пути от начальной €чейки до текущей €чейки.
        Dictionary<Cell, float> gScore = new() 
        {
            [startCell] = 0
        };

        // хранит оценку стоимости пути от начальной €чейки через текущую €чейку до конечной €чейки (эвристическа€ оценка).
        Dictionary<Cell, float> fScore = new() 
        {
            [startCell] = Heuristic(startCell, endCell)
        };

        List<Cell> closedList = new(); // список €чеек, которые уже были проверены.
        List<Cell> openList = new() { startCell }; // список €чеек, которые нужно проверить (соседние €чейки текущей €чейки).

        // список €чеек, откуда пришли в текущую €чейку. Ёто позволит потом восстановить путь от начальной €чейки до конечной.
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

                float tentativeScore = gScore[currentCell] + GetDistance(currentCell, neighborCell);

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
        // »спользуем эвристику ћанхэттенского рассто€ни€ дл€ оценки стоимости пути
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
            int coordinateX = Convert.ToInt32(cell.Coordinates.x + direction.XOffset);
            int coordinateY = Convert.ToInt32(cell.Coordinates.y + direction.YOffset);

            if (coordinateX >= 0 && coordinateX < grid.GridSize.x && coordinateY >= 0 && coordinateY < grid.GridSize.y)
            {
                var neighbour = grid.Cells[coordinateX, coordinateY];
                if (neighbour != null && neighbour != cell)
                {
                    neighbours.Add(neighbour);
                }
            }
        }

        cell.Neighbours = neighbours;
        return neighbours;
    }

    public float GetDistance(Cell cell1, Cell cell2)
    {
        // ≈сли €чейки равны, то рассто€ние между ними равно 0
        if (cell1 == cell2)
            return 0;

        // «десь мы можем использовать любой алгоритм дл€ вычислени€ рассто€ни€ между €чейками.
        // Ќапример, можно использовать евклидово рассто€ние:
        float dx = cell1.Coordinates.x - cell2.Coordinates.x;
        float dy = cell1.Coordinates.y - cell2.Coordinates.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
