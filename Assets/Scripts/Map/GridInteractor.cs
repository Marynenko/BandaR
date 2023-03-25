using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridInteractor : Grid
{
    [SerializeField] private List<Cell> _availableMoves;

    public delegate void UnitSelectedEventHandler(Unit unit, UnitType unitType);
    public static event UnitSelectedEventHandler OnUnitSelected;
    public delegate void UnitActionEventHandler(UnitActionType actionType, Unit unit, Cell cell);
    public static event UnitActionEventHandler OnUnitAction;
    public delegate void EnemySelectedEventHandler(Unit enemy);
    public static event EnemySelectedEventHandler OnEnemySelected;
    public delegate void PlayerSelectedEventHandler(Unit player);
    public static event PlayerSelectedEventHandler OnPlayerSelected;

    public List<Cell> Cells;
    public Unit SelectedUnit { get; set; }

    public void InitializeActions()
    {
        Initialization();
        OnUnitSelected += HandleUnitSelected;
        OnUnitAction += HandleUnitAction;
        OnEnemySelected += HandleEnemySelected;
        OnPlayerSelected += HandlePlayerSelected;
    }

    List<Direction> directions = new List<Direction>()
    {
        new Direction(0, 1),   // Up
        new Direction(0, -1),  // Down
        new Direction(-1, 0),  // Left
        new Direction(1, 0)    // Right
    };

    public void SelectUnit(Unit unit)
    {
        if (SelectedUnit != null)
            UnselectUnit(SelectedUnit);
        
        if (unit.Type == UnitType.Enemy)
        {
            OnEnemySelected?.Invoke(unit);
        }
        else if (unit.Type == UnitType.Player)
        {
            OnPlayerSelected?.Invoke(unit);
        }
    }

    public void UnselectUnit(Unit unit)
    {
        unit.Status = UnitStatus.Unselected;
        OnUnitSelected?.Invoke(unit, unit.Type);
        unit.CurrentCell.ChangeColor(unit.CurrentCell.ColorStandardCell);
        unit.CurrentCell.UnitOn = StatusUnitOn.No;
    }

    public void SelectCell(Cell cell, UnitType unitType)
    {
        UnselectCells();
        List<Cell> availableMovesCopy;
        if (unitType == UnitType.Player)
        {
            _availableMoves = GetAvailableMoves(cell, unitType, 1);
            availableMovesCopy = _availableMoves.GetRange(0, _availableMoves.Count);
        }
        else if (unitType == UnitType.Enemy)
        {
            _availableMoves = GetAvailableMoves(cell, unitType, 1);
            availableMovesCopy = _availableMoves.GetRange(0, _availableMoves.Count);
        }
        else
        {
            return;
        }
        availableMovesCopy.Remove(cell);
        foreach (var moveCell in availableMovesCopy)
        {
            moveCell.ChangeColor(moveCell.ColorMovementCell);
        }
    }


    public void MoveUnit(Unit unit, Cell targetCell)
    {
        if (unit.CurrentCell != null)
        {
            unit.CurrentCell.ChangeColor(unit.CurrentCell.ColorStandardCell);
            unit.CurrentCell.UnitOn = StatusUnitOn.No;
        }

        targetCell.ChangeColor(targetCell.ColorUnitOnCell);
        targetCell.UnitOn = StatusUnitOn.Yes;
        unit.MoveToCell(targetCell);

        if (OnUnitAction != null)
        {
            OnUnitAction.Invoke(UnitActionType.Move, unit, targetCell);
        }
    }

    private void HandleUnitSelected(Unit unit, UnitType unitType)
    {
        if (unitType == UnitType.Player)
        {
            HandlePlayerSelected(unit);
        }
        else if (unitType == UnitType.Enemy)
        {
            HandleEnemySelected(unit);
        }
    }

    private void HandlePlayerSelected(Unit player)
    {
        if (SelectedUnit != null)
        {
            UnselectUnit(SelectedUnit);
        }

        SelectedUnit = player;
        player.Status = UnitStatus.Selected;
        player.CurrentCell.ChangeColor(player.CurrentCell.ColorSelectedCell);
        player.CurrentCell.UnitOn = StatusUnitOn.Yes;
    }

    private void HandleEnemySelected(Unit enemy)
    {
        if (SelectedUnit != null)
        {
            UnselectUnit(SelectedUnit);
        }
        

        SelectedUnit = enemy;
        enemy.Status = UnitStatus.Selected;
        enemy.CurrentCell.ChangeColor(enemy.CurrentCell.ColorEnemyOnCell);
    }

    private void HandleUnitAction(UnitActionType actionType, Unit unit, Cell cell)
    {
        if (actionType == UnitActionType.Move)
        {
            unit.Move(cell);
        }
    }

    public List<Cell> GetAvailableMoves(Cell cell, UnitType unitType, int maxMoves)
    {
        var visitedCells = new HashSet<Cell>();
        var AvailableMoves = new List<Cell>();

        var queue = new Queue<(Cell, int)>();
        queue.Enqueue((cell, maxMoves));

        while (queue.Count > 0)
        {
            var (currentCell, remainingMoves) = queue.Dequeue();

            visitedCells.Add(currentCell);
            AvailableMoves.Add(currentCell);

            if (remainingMoves > 0)
            {
                foreach (var neighbour in GetNeighbourCells(currentCell))
                {
                    if (!visitedCells.Contains(neighbour) && neighbour.IsWalkable() && neighbour.UnitOn == StatusUnitOn.No)
                    {
                        queue.Enqueue((neighbour, remainingMoves - 1));
                    }
                }
            }
        }

        return AvailableMoves;
    }

    public List<Cell> GetNeighbourCells(Cell cell)
    {
        List<Cell> neighbours = new List<Cell>();

        foreach (Direction direction in directions)
        {
            int neighbourX = cell.Row + direction.XOffset;
            int neighbourY = cell.Column + direction.YOffset;

            var Width = _gridGenerator.GridSize.x;
            var Height = _gridGenerator.GridSize.y;


            if (neighbourX >= 0 && neighbourX < Width && neighbourY >= 0 && neighbourY < Height)
            {
                Cell neighbour = GridCells[neighbourX, neighbourY];
                if (neighbour != null)
                {
                    neighbours.Add(neighbour);
                }
            }
        }

        return neighbours;
    }

    public bool AreUnitsAdjacent(Unit unit1, Unit unit2)
    {
        var distance = Vector3.Distance(unit1.transform.position, unit2.transform.position);
        return distance <= 1f; // или другое значение, в зависимости от размеров клетки и модели юнитов
    }

    public List<Cell> FindPathToTarget(Cell startCell, Cell endCell)
    {
        Dictionary<Cell, float> gScore = new Dictionary<Cell, float>();
        gScore[startCell] = 0;

        Dictionary<Cell, float> fScore = new Dictionary<Cell, float>();
        fScore[startCell] = Heuristic(startCell, endCell);

        List<Cell> closedList = new List<Cell>();
        List<Cell> openList = new List<Cell>() { startCell };

        Dictionary<Cell, Cell> cameFrom = new Dictionary<Cell, Cell>();

        while (openList.Count > 0)
        {
            Cell currentCell = openList.OrderBy(cell => fScore.TryGetValue(cell, out float value) ? value : float.MaxValue).FirstOrDefault();


            if (currentCell == endCell)
            {
                return ReconstructPath(cameFrom, endCell);
            }

            openList.Remove(currentCell);
            closedList.Add(currentCell);

            foreach (Cell neighborCell in GetNeighbourCells(currentCell))
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

    private List<Cell> ReconstructPath(Dictionary<Cell, Cell> cameFrom, Cell currentCell)
    {
        List<Cell> path = new List<Cell>() { currentCell };

        while (cameFrom.ContainsKey(currentCell))
        {
            currentCell = cameFrom[currentCell];
            path.Insert(0, currentCell);
        }

        return path;
    }

    private float GetDistanceBetweenCells(Cell cell1, Cell cell2)
    {
        // Здесь мы можем использовать любой алгоритм для вычисления расстояния между ячейками.
        // Например, можно использовать евклидово расстояние:
        float dx = cell1.Coordinates.x - cell2.Coordinates.x;
        float dy = cell1.Coordinates.y - cell2.Coordinates.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    private float Heuristic(Cell a, Cell b)
    {
        // Используем эвристику Манхэттенского расстояния для оценки стоимости пути
        return Mathf.Abs(a.Coordinates.x - b.Coordinates.x) + Mathf.Abs(a.Coordinates.y - b.Coordinates.y);
    }

    public void MoveUnitAlongPath(Unit unit, List<Cell> path)
    {
        // Двигаем юнита поочередно на каждую ячейку из списка
        foreach (var cell in path)
        {
            unit.Move(cell); // было MoveTo
        }
    }

    public void UnselectCells()
    {
        foreach (var cell in Cells)
        {
            cell.ChangeColor(cell.ColorStandardCell);
        }
    }
}

public class Direction
{
    public int XOffset { get; private set; }
    public int YOffset { get; private set; }

    public Direction(int xOffset, int yOffset)
    {
        XOffset = xOffset;
        YOffset = yOffset;
    }
}




