using System.Collections;
using UnityEngine;

public class AI : MonoBehaviour
{
    private GameController _gameController;
    private GameModel _gameModel;
    private Unit _currentUnit;
    private Unit _targetEnemy;

    private bool _isCoroutineForUnitSelectedOn;
    private bool _isTurnStarted;
    private bool _isUnitReadyToAttack;

    public bool IsTurnFinished;


    private void OnEnable()
    {
        _gameController = gameObject.GetComponentInParent<GameController>();
        _gameModel = gameObject.GetComponentInParent<GameModel>();
    }

    private void Update()
    {
        if (_currentUnit == null) return;

        if (_currentUnit.Status != UnitStatus.AIMove) return;

        if (!IsTurnFinished && _currentUnit.Stats.StateFatigue >= 80)
        {
            Debug.Log($"{_currentUnit.Stats.Name} won't move!");
            StartCoroutine(FinishMove());
            return;
        }

        if (IsTurnFinished) return;

        StartMove();
    }

    public void InitializeAI(Unit unit)
    {
        _currentUnit = unit;
    }

    private IEnumerator FinishMove()
    {
        IsTurnFinished = true;
        yield return new WaitForSeconds(1.5f);
        IsTurnFinished = false;

        _targetEnemy = null;
        _isUnitReadyToAttack = false;
        _isTurnStarted = false;
        _gameModel.IsAttackFinished = false;
        _currentUnit = null;

        UIManager.Instance.TurnManager.EndTurn();
    }

    private void StartMove()
    {
        if (!_isTurnStarted)
        {
            StartCoroutine(SelectUnit());
            _isTurnStarted = true;
        }

        if (_isCoroutineForUnitSelectedOn) return;

        if (!_isUnitReadyToAttack)
        {
            AIMoveUnit();

            var onPosition = _gameModel.MatchPositionsPlayerAndDestination(_currentUnit);
            if (onPosition)
                _isUnitReadyToAttack = true;
            else
                _currentUnit.UnitIsMoving = false;
        }

        if (!_isUnitReadyToAttack) return;
        if (_gameModel.IsAttackStarted) return;

        HandleEndTurn();
    }

    private void HandleEndTurn()
    {
        IsTurnFinished = _gameModel.HandleEndTurnButtonClicked(_currentUnit);

        if (!IsTurnFinished)
        {
            if (_gameModel.IsAttackFinished)
                StartCoroutine(FinishMove());
            else
                _gameModel.HandleEndTurnButtonClicked(_currentUnit);
        }
        else
            StartCoroutine(FinishMove());
    }

    private IEnumerator SelectUnit()
    {
        if (_gameController.Selector.SelectedUnit == null)
            _gameController.Selector.SelectUnit(_currentUnit);

        if (!_currentUnit.UnitIsMoving)
        {
            _isCoroutineForUnitSelectedOn = true;
            yield return new WaitForSeconds(1.5f);
            _isCoroutineForUnitSelectedOn = false;
        }
    }

    private void MoveOld()
    {
        _currentUnit.UnitIsMoving = true;
    
        if (_targetEnemy == null)
        {
            // UIManager.Instance.PathConstructor.GetEnemies();
        }
   
        if (_targetEnemy != null)
            _gameController.HandleAITurn(_currentUnit);
    }
    

    private void AIMoveUnit()
    {
        _currentUnit.UnitIsMoving = true;
        _gameController.HandleAITurn(_currentUnit);
    }

}