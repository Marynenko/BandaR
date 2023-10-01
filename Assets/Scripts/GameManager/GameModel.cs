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
        if (activePlayer.UnitIsMoving || activePlayer.Status == UnitStatus.AIMove)
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
    }
    
    public void HandleEndTurnButtonClicked()
    {
        UIManager.Instance.MenuAction.HideMenu();
        activePlayer.Stats.MovementPoints = 0;
        selector.UnselectUnit(activePlayer);
        activePlayer.OccupiedTile.Available = false;
        activePlayer.OccupiedTile.State = activePlayer.Type == UnitType.Player ? TileState.OccupiedByPlayer : TileState.OccupiedByEnemy;
        
        activePlayer.Status = UnitStatus.Moved;

        // Проверяем, был ли игрок перемещен в этом ходе
        if (activePlayer.Status == UnitStatus.Moved)
        {
            // Передаем ход следующему игроку
            EndTurn();    
        }
    }

    private void EndTurn()
    {
        activePlayer = GetNextPlayer(activePlayer);
        
        if (IsGameOver())
            EndGame();
        
        // Если следующий игрок - игрок, делаем его доступным и обновляем доступные ходы
        if (activePlayer.Type == UnitType.Player)
        {
            activePlayer.Status = UnitStatus.Available;
            activePlayer.Stats.MovementPoints = activePlayer.Stats.MovementRange;
        }
        else if (activePlayer.Type == UnitType.Enemy)
        {
            activePlayer.Status = UnitStatus.AIMove;
            activePlayer.Stats.MovementPoints = activePlayer.Stats.MovementRange;
            ai.InitializeAI(activePlayer);
        }
    }

    private Unit GetNextPlayer(Unit player)
    {
        var listOfUnits = grid.AllUnits.ToList();
        var index = listOfUnits.IndexOf(player);
        var nextIndex = (index + 1) % listOfUnits.Count;
        // Проверяем, является ли следующий индекс последним игроком
        return nextIndex == listOfUnits.Count ?
            // Если да, то возвращаем первого игрока из списка
            listOfUnits[0] :
            // Если не последний игрок, возвращаем следующего игрока по индексу
            listOfUnits[nextIndex];
    }

    private bool IsGameOver()
    {
        // Implement game over condition here
        var alivePlayers = _units.Where(p => p.gameObject.activeInHierarchy == true).ToList();
        switch (alivePlayers.Count)
        {
            case 1:
                Debug.Log($"{alivePlayers[0].name} has won the game!"); // TODO пока на карте есть только enemy или только players
                return true;
            case 0:
                Debug.Log("The game has ended in a draw.");
                return true;
            default:
                return false;
        }
    }

    private void EndGame()
    {
        // Implement game ending logic here
        // endTurnButton.interactable = false;
        // Дополнительный функционал завершения игры
    }
}