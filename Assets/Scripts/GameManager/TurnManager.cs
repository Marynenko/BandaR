using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private Grid Grid;
    [SerializeField] private Queue<Unit> Players;
    [SerializeField] private UIPortraitManager PortraitManager;
    public AI AI;

    public Queue<Unit> PlayersGet => Players;

    private Unit _previousPlayer;
    private Unit _activePlayer;
    private bool _isFinishMoveActive;

    private void Update()
    {
        // Call 4
        if (_activePlayer == null)
            return;

        if (_activePlayer.Stats.Type != UnitType.Player) return;
        
        if (_activePlayer.Status != UnitStatus.Moved)
            return;

        // if (!AI.IsTurnFinished)
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

    public void EndTurn()
    {
        _previousPlayer = _activePlayer;
        HighlightPortrait(_previousPlayer); // off

        if (Players.Contains(_activePlayer))
        {
            if (Players.Peek() == _activePlayer)
                Players.Dequeue();
        }

        SetUnitStats();

        _activePlayer = GetNextPlayer(); // Следующий игрок
        Players.Enqueue(_previousPlayer);

        if (IsGameOver())
            EndGame();

        HighlightPortrait(_activePlayer, true); // on


        // Если следующий игрок - игрок, делаем его доступным и обновляем доступные ходы
        var instanceAM = UIManager.Instance.AttackManager;
        if (_activePlayer.Stats.Type is UnitType.Player)
        {
            if (_activePlayer.Stats.StateFatigue >= 100 && !_isFinishMoveActive)
            {
                Debug.Log($"{_activePlayer.Stats.Name} won't move!");
                StartCoroutine(FinishMove(2f));
                return;
            }

            _activePlayer.Status = UnitStatus.Available;
            instanceAM.MovementIndicators.gameObject.transform.GetChild(0).gameObject
                .SetActive(true);
            instanceAM.Attacks.InitializeAttacks(_activePlayer.AttacksPrefab);
            instanceAM.MovementIndicators
                .Launch(_activePlayer.Stats.Energy, _activePlayer.Stats.StateFatigue);
        }
        else if (_activePlayer.Stats.Type is UnitType.Enemy or UnitType.Ally)
        {
            _activePlayer.Status = UnitStatus.AIMove;
            instanceAM.MovementIndicators.gameObject.transform.GetChild(0).gameObject
                .SetActive(false);
            AI.InitializeAI(_activePlayer);
        }
    }


    private IEnumerator FinishMove(float waitTime)
    {
        _isFinishMoveActive = true;
        yield return new WaitForSeconds(waitTime);

        _isFinishMoveActive = false;
        EndTurn();
    }

    private void SetUnitStats()
    {
        var uiManager = UIManager.Instance.AttackManager;
        var stats = _activePlayer.Stats;

        stats.MovementPoints = stats.MovementRange;

        if (stats.CountAttacks != stats.MaxCountAttacks)
            stats.StateFatigue -= Mathf.Round(60f * 0.4f);
        else
            stats.StateFatigue -= 60f;

        stats.CountAttacks = stats.MaxCountAttacks;
        stats.Energy = uiManager.MovementIndicators.EnergyMax;
        stats.EnergyForMove = stats.MovementRange * Tile.EnergyCost - Tile.EnergyCost;
        stats.StateFatigue = Mathf.Clamp(stats.StateFatigue, 0, 100);

        foreach (var unit in Grid.AllUnits)
        {
            unit.OccupiedTile.Available = false;
            unit.OccupiedTile.State = unit.Stats.Type switch
            {
                UnitType.Player => TileState.OccupiedByPlayer,
                UnitType.Enemy => TileState.OccupiedByEnemy,
                UnitType.Ally => TileState.OccupiedByAlly,
                _ => unit.OccupiedTile.State
            };
            UIManager.Instance.GridUI.HighlightTile(unit.OccupiedTile, TileState.Standard);
        }
    }

    private Unit GetNextPlayer()
    {
        return Players.Count > 0 ? Players.Peek() : null;
    }

    public void HighlightPortrait(Unit unit, bool isMoving = false)
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