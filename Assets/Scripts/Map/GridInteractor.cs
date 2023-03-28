using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;
using UnityEngine.UIElements;

public class GridInteractor : Grid
{
    private List<Cell> _availableMoves;
    private List<Direction> directions = new List<Direction>()
    {
        new Direction(0, 1),   // Up
        new Direction(0, -1),  // Down
        new Direction(-1, 0),  // Left
        new Direction(1, 0)    // Right
    };

    private const float MAX_DISTANCE = 3f;

    public delegate void UnitSelectedEventHandler(Unit unit, UnitType unitType);
    public static event UnitSelectedEventHandler OnUnitSelected;
    public delegate void UnitActionEventHandler(UnitActionType actionType, Unit unit, Cell cell);
    public static event UnitActionEventHandler OnUnitAction;

    public IReadOnlyList<Cell> AvailableMoves => _availableMoves.AsReadOnly().ToList();
    public List<Cell> Cells;
    public Unit SelectedUnit { get; set; }

    private void Start() 
    {
        GameController.OnUnitAction += HandleUnitAction;
        OnUnitSelected += HandleUnitSelected;
        OnUnitAction += HandleUnitAction;
    }

    private void OnDestroy()
    {
        GameController.OnUnitAction += HandleUnitAction;
        OnUnitSelected -= HandleUnitSelected;
        OnUnitAction -= HandleUnitAction;
    }    

    public void SelectUnit(Unit unit)
    {
        if (SelectedUnit != null)
            UnselectUnit(SelectedUnit);

        OnUnitSelected?.Invoke(unit, unit.Type);
    }


    public void UnselectUnit(Unit unit)
    {
        unit.Status = UnitStatus.Unselected;
        unit.CurrentCell.ChangeColor(unit.CurrentCell.ColorStandardCell);
        unit.CurrentCell.UnitOn = StatusUnitOn.No;
        unit.CurrentCell.SetIsWalkable(true);
    }

    public void UpdateUnit(Unit unit)
    {
        // Обновить отображение юнита на игровом поле
        unit.UpdateVisuals();
    }

    public void SelectCell(Cell cell, UnitType unitType, bool clearSelectedCells = false, Color? selectedUnitColor = null)
    {
        if (clearSelectedCells)
        {
            UnselectCells();
        }

        List<Cell> availableMovesCopy;

        if (unitType == UnitType.Player || unitType == UnitType.Enemy)
        {
            _availableMoves = GetAvailableMoves(cell, unitType, 1);
            availableMovesCopy = _availableMoves.GetRange(0, _availableMoves.Count);
        }
        else
        {
            return;
        }

        if (selectedUnitColor.HasValue)
        {
            availableMovesCopy.ElementAt(0).ChangeColor(selectedUnitColor.Value);
        }
        else
        {
            if (unitType == UnitType.Player)
            {
                availableMovesCopy.ElementAt(0).ChangeColor(cell.ColorUnitOnCell);
            }
            else if (unitType == UnitType.Enemy)
            {
                availableMovesCopy.ElementAt(0).ChangeColor(cell.ColorEnemyOnCell);
            }
        }

        availableMovesCopy.Remove(cell);
        foreach (var moveCell in availableMovesCopy)
        {
            moveCell.ChangeColor(moveCell.ColorMovementCell);
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

            var Width = Generator.GridSize.x;
            var Height = Generator.GridSize.y;


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
            unit.MoveToCell(cell, this); // изменен вызов метода
        }
    }

    public void MoveUnit(Unit unit, Cell targetCell)
    {
        if (unit.CurrentCell != null)
        {
            unit.CurrentCell.ChangeColor(unit.CurrentCell.ColorStandardCell);

            Color unitColor = unit.Type == UnitType.Player ? unit.CurrentCell.ColorUnitOnCell : unit.CurrentCell.ColorEnemyOnCell; // получение цвета юнита в зависимости от его типа

            targetCell.ChangeColor(unitColor);
            targetCell.UnitOn = StatusUnitOn.Yes;
            unit.MoveToCell(targetCell, this); // изменен вызов метода

            if (OnUnitAction != null)
            {
                OnUnitAction.Invoke(UnitActionType.Move, unit, targetCell);
            }
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
        SelectCell(player.CurrentCell, UnitType.Player, true);
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
        SelectCell(enemy.CurrentCell, UnitType.Enemy, true);
        enemy.CurrentCell.UnitOn = StatusUnitOn.Yes;
    }

    private void HandleUnitAction(UnitActionType actionType, Unit unit, Cell cell)
    {
        if (actionType == UnitActionType.Move)
        {
            unit.MoveToCell(cell, this);
        }
    }

    public void HandleUnitDeselection(Unit selectedUnit, Unit unit)
    {
        UnselectUnit(selectedUnit);
        UnselectCells();
        SelectUnit(unit);
        HighlightAvailableMoves(AvailableMoves, unit.CurrentCell.ColorMovementCell);
    }

    public void HighlightAvailableMoves(IReadOnlyList<Cell> availableMoves, Color color)
    {
        UnselectCells();
        HighlightCell(availableMoves.First(), availableMoves.First().ColorUnitOnCell);
        availableMoves.Skip(1).ToList().ForEach(cell => HighlightCell(cell, color));
    }


    public void UnselectCells()
    {
        foreach (var cell in Cells)
        {
            cell.ChangeColor(cell.ColorStandardCell);
        }
    }

    public void HighlightCell(Cell cell, Color color)
    {
        cell.ChangeColor(color);
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




