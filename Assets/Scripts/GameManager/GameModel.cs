using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameModel : MonoBehaviour, IGameModel
{
    [SerializeField] private Grid _grid;
    [SerializeField] private InputPlayer _input;
    [SerializeField] private GameController _gameController;
    [SerializeField] private GridSelector _selector;
    [SerializeField] private GridInteractor _interactor;
    [SerializeField] private Button _endTurnButton;

    private ActionType _actionType;

    private Unit _activePlayer;
    private List<Unit> _players = new();

    private void Start()
    {
        _endTurnButton.onClick.AddListener(_input.HandleEndTurnButtonClicked);


    }
    public void StartTheGame()
    {
        _players = _grid.AllUnits;
        _activePlayer = _players[0]; // Назначаем первого игрока активным
        StartTurn();
    }

    private void Update()
    {
        UpdateUI();

        if (Input.GetMouseButtonDown(0))
        {
            var mousePosition = Input.mousePosition;
            _input.HandleLeftClick(mousePosition);

            // Проверяем, был ли клик на кнопку
            if (_endTurnButton.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(_endTurnButton.GetComponent<RectTransform>(), mousePosition))
            {
                EndTurn();
            }
        }
    }




    private void UpdateUI()
    {
        // Тут проблема оно не светится
        if (_selector.SelectedUnit == null || _selector.SelectedUnit.Status == UnitStatus.Moved)
        {
            var movedUnits = _grid.AllUnits.Where(u => u.Status == UnitStatus.Moved).ToList();
            if (movedUnits.Count == 0)
            {
                _endTurnButton.interactable = true;
            }
            else
            {
                _endTurnButton.interactable = false;
            }
        }
        else
        {
            _endTurnButton.interactable = false;
        }
    }
    private void MoveAI(Unit unit)
    {
        var availableMoves = _selector.GetAvailableMoves(unit.CurrentCell, unit.MovementPoints);
        if (availableMoves.Count == 0)
        {
            unit.Status = UnitStatus.Moved;
            return;
        }

        // Выбираем случайную доступную клетку для перемещения
        var randomIndex = Random.Range(0, availableMoves.Count);
        var targetCell = availableMoves[randomIndex];

        // Выполняем перемещение на выбранную клетку

        _interactor.PathConstructor.FindPathToTarget(unit.CurrentCell, targetCell, out List<Cell> Path, _grid);
        
        _gameController.MoveUnitAlongPath(unit, Path);

        // Обновляем состояние юнита
        unit.Status = UnitStatus.Moved;
        _interactor.UpdateUnit(unit);

        // Обновляем доступность ячеек после перемещения
        ResetCellsAvailability();
        var units = _grid.AllUnits.Where(u => u == _activePlayer).ToList();
    }




    public void StartTurn()
    {
        if (IsGameOver())
        {
            return;
        }

        UnselectUnit();
        ResetCellsAvailability();
        ResetUnitsAvailability();
        Update();
    }


    public void EndTurn()
    {
        // Снимаем выделение с текущего юнита и доступность ячеек
        UnselectUnit();
        ResetCellsAvailability();
        ResetUnitsAvailability();
        UpdateScore();

        if (IsGameOver())
        {
            EndGame();
        }
        else
        {
            _activePlayer.Status = UnitStatus.Moved;
            _activePlayer = GetNextPlayer(_activePlayer);

            // Если следующий игрок - AI, то делаем ход

            if (_activePlayer.Type == UnitType.Player)
            {
                SetUnitsAvailability();
                StartTurn();
            }
            else if (_activePlayer.Type == UnitType.Enemy)
            {
                MoveAI(_activePlayer);              
            }
        }
    }

    private Unit GetNextPlayer(Unit player)
    {
        var listOfUnits = _grid.AllUnits.OfType<Unit>().ToList();
        var index = listOfUnits.IndexOf(player);
        var nextIndex = (index + 1) % listOfUnits.Count;
        return listOfUnits[nextIndex];
    }

    private void SetUnitsAvailability()
    {
        foreach (var unit in _grid.AllUnits)
        {
            unit.Status = UnitStatus.Available;
        }
    }


    public bool IsGameOver()
    {
        // Implement game over condition here
        var alivePlayers = _players.Where(p => p.gameObject.activeInHierarchy == true).ToList();
        if (alivePlayers.Count == 1)
        {
            Debug.Log($"{alivePlayers[0].name} has won the game!");
            return true;
        }
        else if (alivePlayers.Count == 0)
        {
            Debug.Log("The game has ended in a draw.");
            return true;
        }
        return false;
    }

    public void EndGame()
    {
        // Implement game ending logic here
        _endTurnButton.interactable = false;
        // Дополнительный функционал завершения игры
    }

    public bool IsCellWithinBoardBounds(Cell cell)
    {
        return cell.Row >= 0 && cell.Row < _grid.GridSize.x && cell.Column >= 0 && cell.Column < _grid.GridSize.y;
    }

    public bool IsUnitOwnedByCurrentPlayer(Unit unit)
    {
        return unit.Type == _activePlayer.Type; // Может быть
    }

    public bool IsUnitAvailableForAction(Unit unit)
    {
        if (unit.Status == UnitStatus.Unselected || unit.Status == UnitStatus.Selected) // Тяп ляп
            return true;
        return false;
    }

    public bool IsActionAvailableForUnit(Unit unit, ActionType unitActionType)
    {
        // Check if the unit can perform the action in the current situation
        // For example, a unit cannot attack if there are no enemy units nearby
        return false;
    }

    private void ResetUnitsAvailability()
    {
        foreach (var unit in _grid.AllUnits.OfType<Unit>())
        {
            if (unit == _activePlayer)
            {
                unit.Status = UnitStatus.Available;
            }
            else
            {
                unit.Status = UnitStatus.Unavailable;
            }
        }
    }

    private void ResetCellsAvailability()
    {
        _activePlayer.CurrentCell.UnselectCell();
        // Set all cells to be available for selection
        var highlightedMoves = _interactor.AvailableMoves;
        foreach (var move in highlightedMoves)
        {
            move.UnselectCell();
        }
    }

    private void UnselectUnit()
    {
        // Unselect the current unit and reset cell availability
        if (_selector.SelectedUnit != null)
        {
            _selector.SelectedUnit.Status = UnitStatus.Unselected;
            _selector.SelectedUnit = null;
            _interactor.SelectedUnit =  _selector.SelectedUnit; // Добавил
            ResetCellsAvailability();
        }
    }

    private void UpdateScore()
    {
        // Update the score of both players
    }




}
