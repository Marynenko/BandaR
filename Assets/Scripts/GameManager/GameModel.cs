using System;
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
    private bool _isNextPlayerActive = false;


    private void Start()
    {
        _endTurnButton.onClick.AddListener(_input.HandleEndTurnButtonClicked);
    }

    public void StartGame()
    {
        _players = _grid.AllUnits;
        ActivePlayer = _players[0]; // Назначаем первого игрока активным
        _isNextPlayerActive = false;
        ActivePlayer.Status = UnitStatus.Available;
        StartTurn();
    }


    private void Update()
    {
        if (ActivePlayer == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            var mousePosition = Input.mousePosition;
            _input.HandleLeftClick(mousePosition);

            _AI.UpdateUI(ActivePlayer, _endTurnButton);
        }
    }


    public void StartTurn()
    {
        if (IsGameOver())
            return;

        _grid.SetAvaialableCells();

        //ResetCellsAvailability();

        if (!SetUnitAvailability(ActivePlayer))
        {
            EndTurn();
            return;
        }

        Update();
    }

    public void EndTurn()
    {
        // Снимаем выделение с текущего юнита и доступность ячеек
        ResetCellsAvailability();
        ResetUnitsAvailability();
        UpdateScore();
        //SetAvaialableCells();

        if (IsGameOver())
        {
            EndGame();
        }
        else
        {
            ActivePlayer = GetNextPlayer(ActivePlayer);

            // Если следующий игрок - AI, то делаем ход
            if (ActivePlayer.Type == UnitType.Player)
            {
                StartTurn();
            }
            else if (ActivePlayer.Type == UnitType.Enemy)
            {
                ActivePlayer.Status = UnitStatus.Available;

                if (!SetUnitAvailability(ActivePlayer))
                {
                    EndTurn();
                    return;
                }

                _AI.Move(ActivePlayer);
            }
        }
    }

    private bool SetUnitAvailability(Unit unit)
    {
        if (unit.Status != UnitStatus.Available)
        {
            return false;
        }

        var availableMoves = _selector.GetAvailableMoves(unit.CurrentCell, unit.MovementPoints);
        if (availableMoves.Count > 0)
        {
            unit.SetAvailability(availableMoves);
            return true;
        }

        return false;
    }



    private void UnselectUnit()
    {
        _selector.UnselectUnit(ActivePlayer);
        ResetCellsAvailability();
        //// Unselect the current unit and reset cell availability
        //if (_selector.SelectedUnit != null)
        //{
        //    //_selector.SelectedUnit.Status = UnitStatus.Unselected;
        //    _selector.SelectedUnit = null;
        //    _interactor.SelectedUnit = null; // Добавил

        //    ResetCellsAvailability();
        //}
    }

    public void ResetCellsAvailability()
    {
        var currentCell = ActivePlayer.CurrentCell;
        currentCell.UnselectCell();

        // Set all cells to be available for selection
        _interactor.AvailableMoves.ForEach(move => move.UnselectCell());
    }


    private void ResetUnitsAvailability()
    {
        foreach (var unit in _grid.AllUnits.OfType<Unit>())
        {
            unit.Status = _isNextPlayerActive ? UnitStatus.Available : UnitStatus.Moved;
        }

        _isNextPlayerActive = !_isNextPlayerActive;
    }



    private Unit GetNextPlayer(Unit player)
    {
        var listOfUnits = _grid.AllUnits.OfType<Unit>().ToList();
        var index = listOfUnits.IndexOf(player);
        var nextIndex = (index + 1) % listOfUnits.Count;
        return listOfUnits[nextIndex];
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
        return cell.Coordinates.x >= 0 && cell.Coordinates.x < _grid.GridSize.x && cell.Coordinates.y >= 0 && cell.Coordinates.y < _grid.GridSize.y;
    }

    public bool IsUnitOwnedByCurrentPlayer(Unit unit)
    {
        return unit.Type == ActivePlayer.Type; // Может быть
    }

    public bool IsUnitAvailableForAction(Unit unit)
    {
        if (unit.Status == UnitStatus.Available || unit.Status == UnitStatus.Unavailable) // Тяп ляп
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
