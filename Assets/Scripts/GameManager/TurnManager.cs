using System;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private GameModel gameModel;
    [SerializeField] private AI ai;
    [SerializeField] private Queue<Unit> players;
    [SerializeField] private UIGroupPortraits groupPortraits;

    public Queue<Unit> Players => players;
    public AI AI => ai;

    private const float HEIGHT_TO_PUT_UNIT_ON_TILE = 0.68f;
    private Unit _previousPlayer;
    private Unit _activePlayer;


    private void Update()
    {
        PassMove();
    }

    private void PassMove()
    {
        if (_activePlayer == null) return;
        if (_activePlayer.Status != UnitStatus.Moved) return;
        // Unit.OnMove?.Invoke(_activePlayer);
        EndTurn();
    }

    public void Launch()
    {
        players = new Queue<Unit>(grid.AllUnits);
    }

    public void SetCurrentPlayer(ref Unit unit)
    {
        _activePlayer = unit;
    }

    private void EndTurn()
    {
        _previousPlayer = _activePlayer;
        HighlightPlayer(_previousPlayer); // off

        if (players.Contains(_activePlayer))
        {
            if (players.Peek() == _activePlayer)
                players.Dequeue();
        }

        _activePlayer = GetNextPlayer();
        players.Enqueue(_previousPlayer);

        if (IsGameOver())
            EndGame();

        HighlightPlayer(_activePlayer, true); // on

        // Если следующий игрок - игрок, делаем его доступным и обновляем доступные ходы
        if (_activePlayer.Stats.Type == UnitType.Player)
        {
            _activePlayer.Status = UnitStatus.Available;
            _activePlayer.Stats.MovementPoints = _activePlayer.Stats.MovementRange;
        }
        else if (_activePlayer.Stats.Type == UnitType.Enemy)
        {
            _activePlayer.Status = UnitStatus.AIMove;
            _activePlayer.Stats.MovementPoints = _activePlayer.Stats.MovementRange;
            ai.InitializeAI(_activePlayer);
        }
    }

    private Unit GetNextPlayer()
    {
        return players.Count > 0 ? players.Peek() : null;
    }

    public void HighlightPlayer(Unit unit, bool isMoving = false)
    {
        _previousPlayer = unit;
        var animator = _previousPlayer.Sign.GetComponent<Animator>();
        var unitImg = groupPortraits.GetPlayerPortrait(_previousPlayer);
        var unitImgScript = groupPortraits.GetPlayerBackground(unitImg);
        if (isMoving)
        {
            unitImgScript.TurnOnAlpha();
            _previousPlayer.Sign.gameObject.SetActive(true);
            animator.enabled = true;
        }
        else
        {
            unitImgScript.TurnOffAlpha();
            _previousPlayer.Sign.gameObject.SetActive(false);
            animator.enabled = false;
        }
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