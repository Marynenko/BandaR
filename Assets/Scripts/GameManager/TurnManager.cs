using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private GameModel gameModel;
    [SerializeField] private AI ai;
    [SerializeField] private Queue<Unit> players;
    public Queue<Unit> Players => players;
    [SerializeField] private UIGroupPortraits groupPortraits;

    private Unit _currentPlayer;

    public void Launch()
    {
        players = new Queue<Unit>(grid.AllUnits);
    }

    public void HighlightPlayer(Unit unit, bool isMoving = false)
    {
        _currentPlayer = unit;
        var unitImg = groupPortraits.GetPlayerPortrait(unit);
        var unitImgScript = groupPortraits.GetPlayerBackground(unitImg);
        if (isMoving)
        {
            unitImgScript.TurnOnAlpha();
        }
        else
        {
            unitImgScript.TurnOffAlpha();
        }
    }

    public void EndTurn(ref Unit activePlayer)
    {
        _currentPlayer = activePlayer;
        HighlightPlayer(activePlayer);

        if (players.Contains(activePlayer))
        {
            if (players.Peek() == activePlayer)
                players.Dequeue();
        }

        activePlayer = GetNextPlayer();
        players.Enqueue(_currentPlayer);

        HighlightPlayer(activePlayer, true);

        if (IsGameOver())
            EndGame();

        // Если следующий игрок - игрок, делаем его доступным и обновляем доступные ходы
        if (activePlayer.Type == UnitType.Player)
        {
            activePlayer.Status = UnitStatus.Available;
            activePlayer.Stats.MovementPoints = activePlayer.MovementRange;
        }
        else if (activePlayer.Type == UnitType.Enemy)
        {
            activePlayer.Status = UnitStatus.AIMove;
            activePlayer.Stats.MovementPoints = activePlayer.MovementRange;
            ai.InitializeAI(activePlayer);
        }
    }

    private Unit GetNextPlayer()
    {
        return players.Count > 0 ? players.Dequeue() : null;
    }
    
    #region EndGame
    private bool IsGameOver()
    {
        return false;
        // foreach (var player in players)
        // {
        //     if (player.gameObject.activeInHierarchy) break;
        //     if (player.gameObject.activeInHierarchy == false)
        //     {
        //         Debug.Log("You won!");
        //         return false;
        //     }
        // }
    }

    private void EndGame()
    {
        // Implement game ending logic here
        // endTurnButton.interactable = false;
        // Дополнительный функционал завершения игры
    }
    #endregion
}