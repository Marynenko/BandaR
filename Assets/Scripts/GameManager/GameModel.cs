using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameModel : MonoBehaviour, IGameModel
{
    [SerializeField] private AI _AI;
    [SerializeField] private Grid _grid;
    [SerializeField] private InputPlayer _input;
    [SerializeField] private GameController _gameController;
    [SerializeField] private GridSelector _selector;
    [SerializeField] private GridInteractor _interactor;
    [SerializeField] private Button _endTurnButton;

    [HideInInspector] public Unit ActivePlayer;

    private ActionType _actionType;
    private List<Unit> _players = new();

    private void Start()
    {
        _endTurnButton.onClick.AddListener(_input.HandleEndTurnButtonClicked);
    }

    public void StartTheGame()
    {
        _players = _grid.AllUnits;
        ActivePlayer = _players[0]; // Назначаем первого игрока активным
        StartTurn();
    }

    private void Update()
    {       
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
            ActivePlayer.Status = UnitStatus.Moved;
            ActivePlayer = GetNextPlayer(ActivePlayer);

            // Если следующий игрок - AI, то делаем ход

            if (ActivePlayer.Type == UnitType.Player)
            {
                SetUnitsAvailability();
                StartTurn();
            }
            else if (ActivePlayer.Type == UnitType.Enemy)
            {
                _AI.Move(ActivePlayer);              
            }
        }
    }

    private void UnselectUnit()
    {
        // Unselect the current unit and reset cell availability
        if (_selector.SelectedUnit != null)
        {
            //_selector.SelectedUnit.Status = UnitStatus.Unselected;
            _selector.SelectedUnit = null;
            _interactor.SelectedUnit = null; // Добавил

            ResetCellsAvailability();
        }
    }

    public void ResetCellsAvailability()
    {
        ActivePlayer.CurrentCell.UnselectCell();
        // Set all cells to be available for selection
        var highlightedMoves = _interactor.AvailableMoves;
        foreach (var move in highlightedMoves)
        {
            move.UnselectCell();
        }
    }

    private void ResetUnitsAvailability()
    {
        foreach (var unit in _grid.AllUnits.OfType<Unit>())
        {
            if (unit == ActivePlayer)
            {
                unit.Status = UnitStatus.Available;
            }
            else
            {
                unit.Status = UnitStatus.Unavailable;
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

    private void UpdateScore()
    {
        // Update the score of both players
    }
    #region Не использую пока что
    public bool IsCellWithinBoardBounds(Cell cell)
    {
        return cell.Row >= 0 && cell.Row < _grid.GridSize.x && cell.Column >= 0 && cell.Column < _grid.GridSize.y;
    }

    public bool IsUnitOwnedByCurrentPlayer(Unit unit)
    {
        return unit.Type == ActivePlayer.Type; // Может быть
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
    #endregion
}
