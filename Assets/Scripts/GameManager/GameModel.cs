using UnityEngine;

public class GameModel : MonoBehaviour, IGameModel
{
    [SerializeField] private GameController _controller;
    [SerializeField] private Grid _grid;
    [SerializeField] private GridSelector _selector;
    [SerializeField] private GridInteractor _interactor;

    private Unit _activePlayer;
    private Unit _nextPlayer;

    private bool _gameEnded;

    public void StartGame()
    {
        // ...

        StartTurn(_activePlayer);
    }

    public void StartTurn(Unit player)
    {
        if (_gameEnded)
        {
            return;
        }

        _activePlayer = player;
        _nextPlayer = GetOpponent(player); // Тут еще подумать
        ResetUnitsAvailability();
        ResetCellsAvailability();
        UpdateScore();
        HighlightAvailableUnits();
    }

    public void EndTurn(Unit activePlayer, Unit _nextPlayer)
    {

        _activePlayer = activePlayer;
        this._nextPlayer = _activePlayer;

        _activePlayer.CurrentCell.UnselectCell();

        var highlightedMoves = _interactor.AvailableMoves;
        foreach (var move in highlightedMoves)
        {
            move.UnselectCell();
        }


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
            StartTurn(_nextPlayer);
        }
    }

    public bool IsCellWithinBoardBounds(Cell cell)
    {
        return cell.Row >= 0 && cell.Row < _grid.GridSize.x && cell.Column >= 0 && cell.Column < _grid.GridSize.y;
    }

    public bool IsCellAvailable(Cell cell)
    {
        return cell.IsAwailable();
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

    public bool IsMoveAllowed(Cell sourceCell, Cell targetCell)
    {
        return false;
        // Check if the move is allowed according to the game rules
    }

    public bool IsGameOver()
    {
        return false;
    }

    private void EndGame()
    {
        _gameEnded = true;
        // ...
    }

    private void ResetUnitsAvailability()
    {
        // Set all units to be available for action
    }

    private void ResetCellsAvailability()
    {
        // Set all cells to be available for selection
    }

    private void HighlightAvailableUnits()
    {
        // Highlight all units that are available for action
    }

    private void UnselectUnit()
    {
        // Unselect the current unit and reset cell availability
        _selector.UnselectUnit(_activePlayer);
    }

    private void UpdateScore()
    {
        // Update the score of both players
    }

    private Unit GetOpponent(Unit player)
    {
        return null;
        // Get the opponent of the specified player
    }
}
