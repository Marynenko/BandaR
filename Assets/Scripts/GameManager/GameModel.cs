using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameModel : MonoBehaviour
{
    [SerializeField] private AI ai;
    [SerializeField] private Grid grid;
    [SerializeField] private InputPlayer input;
    [SerializeField] private GameController gameController;
    [SerializeField] private Selector selector;

    [HideInInspector] public Unit activePlayer;

    private List<Unit> _units = new();

    private void Start()
    {
        grid.StartCreating();
        StartGame();
    }

    private void StartGame()
    {
        _units = grid.AllUnits;
        activePlayer = _units[0]; // Назначаем первого игрока активным
        activePlayer.Status = UnitStatus.Available;
        StartTurn();
    }


    private void Update()
    {
        if (activePlayer == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            var mousePosition = Input.mousePosition;
            input.HandleLeftClick(mousePosition);
            
        }
    }


    private void StartTurn()
    {
        if (IsGameOver())
            return;

        grid.SetAvailableTiles();

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
        activePlayer = GetNextPlayer(activePlayer);

        // Если все игроки уже "Moved", перезапускаем возможность ходить всем на "Unavailable"
        // if (_units.All(u => u.Status == UnitStatus.Moved))
        //     ResetUnitsAvailability();

        // UpdateScore();
        //SetAvailableTiles();

        if (IsGameOver())
            EndGame();

        // Если следующий игрок - игрок, делаем его доступным и обновляем доступные ходы
        if (activePlayer.Type == UnitType.Player)
        {
            activePlayer.Status = UnitStatus.Available;
            //SetUnitAvailability(ActivePlayer); 
            activePlayer.Stats.MovementPoints = activePlayer.Stats.MovementRange;

            if (SetUnitAvailability(activePlayer))
            {
                EndTurn();
            }
        }
        else if (activePlayer.Type == UnitType.Enemy)
        {
            // Если следующий игрок - AI, то делаем ход AI
            //ActivePlayer.Status = UnitStatus.Available;
            activePlayer.Status = UnitStatus.AIMove;
            activePlayer.Stats.MovementPoints = activePlayer.Stats.MovementRange;
            
            if (SetUnitAvailability(activePlayer))
            {
                EndTurn();
                return;
            }

            ai.InitializeAI(activePlayer);
        }
    }
    
    public void HandleEndTurnButtonClicked()
    {
        UIManager.Instance.MenuAction.HideMenu();
        activePlayer.Stats.MovementPoints = 0;
        selector.UnselectUnit(activePlayer);
        activePlayer.Status = UnitStatus.Moved;

        // Проверяем, был ли игрок перемещен в этом ходе
        if (activePlayer.Status == UnitStatus.Moved)
        {
            // Передаем ход следующему игроку
            EndTurn();    
        }
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

    private Unit GetNextPlayer(Unit player)
    {
        var listOfUnits = grid.AllUnits.ToList();
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

    private bool IsGameOver()
    {
        // Implement game over condition here
        var alivePlayers = _units.Where(p => p.gameObject.activeInHierarchy == true).ToList();
        if (alivePlayers.Count == 1)
        {
            Debug.Log($"{alivePlayers[0].name} has won the game!"); // TODO пока на карте есть только enemy или только players
            return true;
        }
        else if (alivePlayers.Count == 0) // TODO это невозможно
        {
            Debug.Log("The game has ended in a draw.");
            return true;
        }

        return false;
    }

    private void EndGame()
    {
        // Implement game ending logic here
        // endTurnButton.interactable = false;
        // Дополнительный функционал завершения игры
    }

    private void UpdateScore()
    {
        // Update the score of both players
    }

    #region Не использую пока что

    public bool IsTileWithinBoardBounds(Tile tile) =>
        tile.Coordinates.x >= 0 && tile.Coordinates.x < grid.GridSize.x
                                && tile.Coordinates.y >= 0
                                && tile.Coordinates.y < grid.GridSize.y;

    public bool IsUnitOwnedByCurrentPlayer(Unit unit) =>
        unit.Type == activePlayer.Type; // Может быть

    public bool IsUnitAvailableForAction(Unit unit)
    {
        if (unit.Status == UnitStatus.Available || unit.Status == UnitStatus.Unavailable) // Тяп ляп
            return true;
        return false;
    }

    #endregion
}