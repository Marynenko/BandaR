using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameModel : MonoBehaviour
{
    [SerializeField] private AI _ai;
    [SerializeField] private TilesGrid _grid;
    [SerializeField] private InputPlayer _input;
    [SerializeField] private GameController _gameController;
    [SerializeField] private Selector _selector;
    [SerializeField] private Interactor _interactor;
    [SerializeField] private Button _endTurnButton;

    [HideInInspector] public Unit ActivePlayer;

    private readonly ActionType _actionType;
    private List<Unit> _players = new();

    private void Start()
    {
        _endTurnButton.onClick.AddListener(_input.HandleEndTurnButtonClicked);
    }

    public void StartGame()
    {
        _grid.StartCreating();
        _players = _grid.AllUnits;
        ActivePlayer = _players[0]; // Назначаем первого игрока активным
        ActivePlayer.Status = UnitStatus.Available;
        StartTurn();
    }


    private void Update()
    {
        if (ActivePlayer == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            var mousePosition = Input.mousePosition;
            _input.HandleLeftClick(mousePosition);

            _ai.UpdateUi(ActivePlayer, _endTurnButton);
        }
    }


    public void StartTurn()
    {
        if (IsGameOver())
            return;

        _grid.SetAvailableTiles();

        //ResetTilesAvailability();

        //if (!SetUnitAvailability(ActivePlayer))
        //{
        //    EndTurn();
        //    return;
        //}

        Update();
    }

    public void EndTurn()
    {
        // Снимаем выделение с текущего юнита и доступность ячеек
        //ResetTilesAvailability();
        //ActivePlayer.OccupiedTile.UnselectTile();
        ActivePlayer = GetNextPlayer(ActivePlayer);

        // Если все игроки уже "Moved", перезапускаем возможность ходить всем на "Unavailable"
        if (_players.All(p => p.Status == UnitStatus.Moved))
            ResetUnitsAvailability();

        UpdateScore();
        //SetAvailableTiles();

        if (IsGameOver())
            EndGame();

        // Если следующий игрок - игрок, делаем его доступным и обновляем доступные ходы
        if (ActivePlayer.Type == UnitType.Player)
        {
            ActivePlayer.Status = UnitStatus.Available;
            //SetUnitAvailability(ActivePlayer); 
            ActivePlayer.Stats.MovementPoints = ActivePlayer.Stats.MovementRange;

            if (SetUnitAvailability(ActivePlayer))
            {
                EndTurn();
                return;
            }
        }
        else if (ActivePlayer.Type == UnitType.Enemy)
        {
            // Если следующий игрок - AI, то делаем ход AI
            //ActivePlayer.Status = UnitStatus.Available;
            ActivePlayer.Status = UnitStatus.AIMove;

            if (SetUnitAvailability(ActivePlayer))
            {
                EndTurn();
                return;
            }

            _ai.Move(ActivePlayer);
        }

        // Дополнительные действия, если необходимо, после окончания хода
        // ...

        // Обновляем UI или выполняем другие действия, связанные с окончанием хода
        // ...
    }

    private bool SetUnitAvailability(Unit unit)
    {
        if (unit.Status != UnitStatus.Available && unit.Status != UnitStatus.AIMove)
        {
            return false;
        }

        if (unit.MovementPoints == 0 || unit.MovementPoints == 1)
        {
            unit.SetAvailability();
            return true;
        }

        return false;
    }


    private void UnselectUnit()
    {
        _selector.UnselectUnit(ActivePlayer);
        ResetTilesAvailability();
        //// Unselect the current unit and reset tile availability
        //if (_selector.SelectedUnit != null)
        //{
        //    //_selector.SelectedUnit.Status = UnitStatus.Unselected;
        //    _selector.SelectedUnit = null;
        //    _interactor.SelectedUnit = null; // Добавил

        //    ResetTilesAvailability();
        //}
    }

    public void ResetTilesAvailability()
    {
        var currentTile = ActivePlayer.OccupiedTile;
        currentTile.UnselectTile();

        // Set all cells to be available for selection
        _interactor.AvailableMoves.ForEach(move => move.UnselectTile());
    }

    public void ResetUnitsAvailability()
    {
        foreach (var unit in _grid.AllUnits.OfType<Unit>())
            unit.Status = UnitStatus.Unavailable;
    }


    private Unit GetNextPlayer(Unit player)
    {
        var listOfUnits = _grid.AllUnits.OfType<Unit>().ToList();
        var index = listOfUnits.IndexOf(player);
        var nextIndex = (index + 1) % listOfUnits.Count;
        // Проверяем, является ли следующий индекс последним игроком
        if (nextIndex == listOfUnits.Count)
        {
            // Если да, то возвращаем первого игрока из списка
            return listOfUnits[0];
        }

        // Если не последний игрок, возвращаем следующего игрока по индексу
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

    public bool IsTileWithinBoardBounds(Tile tile) =>
        tile.Coordinates.x >= 0 && tile.Coordinates.x < _grid.GridSize.x
                                && tile.Coordinates.y >= 0
                                && tile.Coordinates.y < _grid.GridSize.y;

    public bool IsUnitOwnedByCurrentPlayer(Unit unit) =>
        unit.Type == ActivePlayer.Type; // Может быть

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