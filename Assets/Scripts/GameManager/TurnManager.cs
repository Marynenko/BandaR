using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TurnManager : MonoBehaviour
{
    [FormerlySerializedAs("_grid")] [SerializeField] private Grid Grid;
    [FormerlySerializedAs("_gameModel")] [SerializeField] private GameModel GameModel;
    [FormerlySerializedAs("_ai")] [SerializeField] private AI AI;
    [FormerlySerializedAs("_players")] [SerializeField] private Queue<Unit> Players;
    [FormerlySerializedAs("_portraitManager")] [SerializeField] private UIPortraitManager PortraitManager;

    public Queue<Unit> PlayersGet => Players;
    private Unit _previousPlayer;
    private Unit _activePlayer;
    private bool _isFinishMoveActive;

    private void Update()
    {
        // Call 4
        if (_activePlayer == null) return;
        if (_activePlayer.Status != UnitStatus.Moved) return;
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

        _activePlayer = GetNextPlayer(); // ��������� �����
        Players.Enqueue(_previousPlayer);

        if (IsGameOver())
            EndGame();

        HighlightPortrait(_activePlayer, true); // on


        // ���� ��������� ����� - �����, ������ ��� ��������� � ��������� ��������� ����
        if (_activePlayer.Stats.Type is UnitType.Player)
        {
            if (_activePlayer.Stats.StateFatigue >= 100 && !_isFinishMoveActive)
            {
                Debug.Log($"{_activePlayer.Stats.Name} won't move!");
                StartCoroutine(FinishMove(2f));
                return;
            }
            
            UIManager.Instance.AttackManager.MovementIndicators.gameObject.transform.GetChild(0).gameObject.SetActive(true);
            _activePlayer.Status = UnitStatus.Available;
            UIManager.Instance.AttackManager.Attacks.InitializeAttacks(_activePlayer.AttacksPrefab);
            UIManager.Instance.AttackManager.MovementIndicators
                .Launch(_activePlayer.Stats.Energy, _activePlayer.Stats.StateFatigue);
        }
        else if (_activePlayer.Stats.Type is UnitType.Enemy or UnitType.Ally)
        {
            UIManager.Instance.AttackManager.MovementIndicators.gameObject.transform.GetChild(0).gameObject.SetActive(false);
            _activePlayer.Status = UnitStatus.AIMove;
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
        _activePlayer.Stats.MovementPoints = _activePlayer.Stats.MovementRange;
        if (_activePlayer.Stats.CountAttacks != _activePlayer.Stats.MaxCountAttacks)
            _activePlayer.Stats.StateFatigue -= Mathf.Round(60f * 0.4f);
        else _activePlayer.Stats.StateFatigue -= 60f;
        _activePlayer.Stats.CountAttacks = _activePlayer.Stats.MaxCountAttacks;
        _activePlayer.Stats.Energy = uiManager.MovementIndicators.EnergyMax;
        _activePlayer.Stats.EnergyForMove = 40f;
        _activePlayer.Stats.EnergyForAttack = 60f;
        _activePlayer.Stats.StateFatigue = Mathf.Clamp(_activePlayer.Stats.StateFatigue, 0, 100);
        // uiManager._attacks._attacksPrefab = _activePlayer.AttacksPrefab;

        if (_activePlayer.Stats.Type is UnitType.Player)
            uiManager.MovementIndicators.Launch(uiManager.MovementIndicators.EnergyMax, _activePlayer.Stats.StateFatigue);
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
        // �������������� ���������� ���������� ����
    }

    #endregion
}