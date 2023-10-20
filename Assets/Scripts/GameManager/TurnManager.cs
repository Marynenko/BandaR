using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private Grid Grid;
    [SerializeField] private GameModel GameModel;
    [SerializeField] private AI AI;
    [SerializeField] private Queue<Unit> Players;

    [FormerlySerializedAs("GroupPortraits")] [SerializeField]
    private UIPortraitManager PortraitManager;

    public Queue<Unit> PlayersGet => Players;

    private Unit _previousPlayer;
    private Unit _activePlayer;

    private delegate void OffSign(); 
        
    private void Update()
    {
        // Call 4
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
        Players = new Queue<Unit>(Grid.AllUnits);
    }

    public void SetCurrentPlayer(ref Unit unit)
    {
        _activePlayer = unit;
    }

    private void EndTurn()
    {
        _previousPlayer = _activePlayer;
        ShowPortrait(_previousPlayer); // off

        if (Players.Contains(_activePlayer))
        {
            if (Players.Peek() == _activePlayer)
                Players.Dequeue();
        }

        _activePlayer = GetNextPlayer();
        Players.Enqueue(_previousPlayer);

        if (IsGameOver())
            EndGame();

        ShowPortrait(_activePlayer, true); // on

        // Если следующий игрок - игрок, делаем его доступным и обновляем доступные ходы
        if (_activePlayer.Stats.Type is UnitType.Player)
        {
            _activePlayer.Status = UnitStatus.Available;
            _activePlayer.Stats.MovementPoints = _activePlayer.Stats.MovementRange;
            UIManager.Instance.AttackManager.Attacks.AttacksPrefab = _activePlayer.AttacksPrefab;

        }
        else if (_activePlayer.Stats.Type is UnitType.Enemy or UnitType.Ally)
        {
            _activePlayer.Status = UnitStatus.AIMove;
            _activePlayer.Stats.MovementPoints = _activePlayer.Stats.MovementRange;
            UIManager.Instance.AttackManager.Attacks.AttacksPrefab = _activePlayer.AttacksPrefab;
            AI.InitializeAI(_activePlayer);
        }
    }

    private Unit GetNextPlayer()
    {
        return Players.Count > 0 ? Players.Peek() : null;
    }

    public void ShowPortrait(Unit unit, bool isMoving = false)
    {
        _previousPlayer = unit;
        var animator = _previousPlayer.Sign.GetComponent<Animator>();
        var unitImg = PortraitManager.GetPlayerPortrait(_previousPlayer);
        var unitImgScript = PortraitManager.GetPlayerBackground(unitImg);
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