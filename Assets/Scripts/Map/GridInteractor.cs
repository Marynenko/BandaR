using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;
using UnityEngine.UIElements;

public class GridInteractor : MonoBehaviour
{
    private Grid _grid;
    private List<Cell> _availableMoves;
    private readonly List<Direction> directions = new()
    {
        new Direction(0, 1),   // Up
        new Direction(0, -1),  // Down
        new Direction(-1, 0),  // Left
        new Direction(1, 0)    // Right
    };

    //private const float MAX_DISTANCE = 3f;

    public delegate void UnitSelectedEventHandler(Unit unit, UnitType unitType);
    public static event UnitSelectedEventHandler OnUnitSelected;
    public delegate void UnitActionEventHandler(UnitActionType actionType, Unit unit, Cell cell);
    public static event UnitActionEventHandler OnUnitAction;

    public IReadOnlyList<Cell> AvailableMoves => _availableMoves.AsReadOnly().ToList();

    public Unit SelectedUnit { get; set; }

    public void OnEnable()
    {
        //GameController.OnUnitAction += HandleUnitAction;
        _grid = GetComponentInParent<Grid>();
        _availableMoves = new List<Cell>();
        OnUnitSelected += HandleUnitSelected;
        OnUnitAction += HandleUnitAction;
    }
    private void OnDestroy()
    {
        //GameController.OnUnitAction += HandleUnitAction;
        OnUnitSelected -= HandleUnitSelected;
        OnUnitAction -= HandleUnitAction;
    }    

    public void SelectUnit(Unit unit)
    {
        if (SelectedUnit != null)
        {
            SelectedUnit.CurrentCell.UnselectCell();
            UnselectUnit(SelectedUnit);
        }

        OnUnitSelected?.Invoke(unit, unit.Type);
    }


    public void UnselectUnit(Unit unit)
    {
        //unit.Status = UnitStatus.Unselected;
        SelectedUnit = null;
        unit.CurrentCell.ClearUnit();
    }

    public void UpdateUnit(Unit unit)
    {
        // Обновить отображение юнита на игровом поле
        unit.UpdateVisuals();
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
        player.CurrentCell.SetUnit(player);
        SelectedUnit = player;
        player.Status = UnitStatus.Selected;
        player.CurrentCell.SelectCell();
        SelectCellToMove(player.CurrentCell, UnitType.Player, true);
        player.CurrentCell.CellStatus = UnitOn.Yes; // тут или перед SelectCellToMove?
    }

    private void HandleEnemySelected(Unit enemy)
    {
        enemy.CurrentCell.SetUnit(enemy);
        SelectedUnit = enemy;
        enemy.Status = UnitStatus.Selected;
        enemy.CurrentCell.SelectCell();
        SelectCellToMove(enemy.CurrentCell, UnitType.Enemy, true);
        enemy.CurrentCell.CellStatus= UnitOn.Yes;
    }

    private void HandleUnitAction(UnitActionType actionType, Unit unit, Cell cell)
    {
        if (actionType == UnitActionType.Move)
        {
            unit.MoveToCell(cell);
        }
    }

    public void HandleUnitDeselection(Unit selectedUnit, Unit unit)
    {
        UnselectUnit(selectedUnit);
        unit.CurrentCell.UnselectCell();
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
        var Cells = _grid.Cells;
        foreach (var cell in Cells)
        {
            cell.ChangeColor(cell.ColorStandardCell);
        }
    }

    public void HighlightCell(Cell cell, Color color)
    {
        cell.ChangeColor(color);
    }


    public void SelectCellToMove(Cell cell, UnitType unitType, bool clearSelectedCells = false, Color? selectedUnitColor = null)
    {
        if (clearSelectedCells)
        {
            UnselectCells();
        }

        List<Cell> availableMovesCopy;

        if (unitType == UnitType.Player || unitType == UnitType.Enemy)
        {
            _availableMoves = GetAvailableMoves(cell, 1);
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

    public List<Cell> GetAvailableMoves(Cell cell, int maxMoves)
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
                    if (!visitedCells.Contains(neighbour) && neighbour.IsWalkable() && neighbour.CellStatus == UnitOn.No)
                    {
                        queue.Enqueue((neighbour, remainingMoves - 1));
                    }
                }
            }
        }

        cell.Neighbours = AvailableMoves;
        return AvailableMoves;
    }

    public List<Cell> GetNeighbourCells(Cell cell)
    {
        List<Cell> neighbours = new();

        foreach (Direction direction in directions)
        {
            int neighbourX = cell.Row + direction.XOffset;
            int neighbourY = cell.Column + direction.YOffset;

            if (neighbourX >= 0 && neighbourX < _grid.GridSize.x && neighbourY >= 0 && neighbourY < _grid.GridSize.y)
            {
                var neighbour = _grid.Cells[neighbourX, neighbourY];
                if (neighbour != null && neighbour != cell)
                {
                    neighbours.Add(neighbour);
                }
            }
        }

        return neighbours;
    }

    public List<Cell> FindPathToTarget(Cell startCell, Cell endCell, out List<Cell> Path)
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

            foreach (var neighborCell in GetNeighbourCells(currentCell))
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

    public void MoveUnitAlongPath(Unit unit, List<Cell> path)
    {
        // Двигаем юнита поочередно на каждую ячейку из списка
        foreach (var cell in path)
        {
            unit.MoveToCell(cell); // изменен вызов метода
        }
    }

    public void MoveUnit(Unit unit, Cell targetCell)
    {
        if (unit.CurrentCell != null)
        {
            unit.CurrentCell.ChangeColor(unit.CurrentCell.ColorStandardCell);

            Color unitColor = unit.Type == UnitType.Player ? unit.CurrentCell.ColorUnitOnCell : unit.CurrentCell.ColorEnemyOnCell; // получение цвета юнита в зависимости от его типа

            targetCell.ChangeColor(unitColor);
            targetCell.CellStatus = UnitOn.Yes;
            unit.MoveToCell(targetCell); // изменен вызов метода

            OnUnitAction?.Invoke(UnitActionType.Move, unit, targetCell);
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




