using System.Collections;
using System.Linq;
using UnityEngine;

public class AI : MonoBehaviour
{
    private GameController _gameController;
    private GameModel _gameModel;
    private Unit _currentUnit;

    private bool _isCoroutineRunning;
    private bool _isFinishMoveActive;

    private void OnEnable()
    {
        _gameController = gameObject.GetComponentInParent<GameController>();
        _gameModel = gameObject.GetComponentInParent<GameModel>();
    }

    private void Update()
    {
        // Call 3
        if (_currentUnit == null)
            return;

        if (_currentUnit.Stats.Type == UnitType.Player)
            return;

        if (_currentUnit.Status != UnitStatus.AIMove)
            return;

        // if (_gameModel._attackIsFinished)
        // {
        //     _currentUnit.UnitIsMoving = false;
        //     _gameModel._attackIsFinished = false;
        //     UIManager.Instance.TurnManager.EndTurn();
        //     return;
        // }

        if (!_isFinishMoveActive && _currentUnit.Stats.StateFatigue >= 80)
        {
            Debug.Log($"{_currentUnit.Stats.Name} won't move!");
            StartCoroutine(FinishMove(1.5f));
            return;
        }

        if (_isFinishMoveActive)
            return;


        StartMove();
    }

    private IEnumerator FinishMove(float waitTime)
    {
        _isFinishMoveActive = true;
        yield return new WaitForSeconds(waitTime);
        _isFinishMoveActive = false;

        _currentUnit.UnitIsMoving = false;
        _isCoroutineRunning = false;
        _currentUnit.Status = UnitStatus.Moved;
        UIManager.Instance.TurnManager.EndTurn();
    }

    public void InitializeAI(Unit unit)
    {
        _currentUnit = unit;
    }

    private void StartMove()
    {
        if (!_isCoroutineRunning && _currentUnit.UnitIsMoving)
        {
            Move();

            if (HandleEndTurnButton())
                return;
        }

        // if (HandleEndTurnButton()) 
        //     return;

        if (_isCoroutineRunning || _currentUnit.UnitIsMoving)
            return;

        StartCoroutine(SelectUnit());

        // if (!_currentUnit.UnitIsMoving)
        //     return;
        //
        // _isCoroutineRunning = false;
        // _currentUnit.UnitIsMoving = true;
    }

    private bool HandleEndTurnButton()
    {
        if (!_isCoroutineRunning && !_currentUnit.UnitIsMoving)
        {
            var successfulFinish = _gameModel.HandleEndTurnButtonClicked(_currentUnit);

            if (!successfulFinish)
            {
                if (_gameModel._attackIsFinished)
                {
                    HandleEndTurnButton();
                }

                return false;
            }

            _currentUnit = null;
            return true;
        }

        return false;
    }

    private IEnumerator SelectUnit()
    {
        if (_gameController.Selector.SelectedUnit == null)
            _gameController.Selector.SelectUnit(_currentUnit);

        if (!_currentUnit.UnitIsMoving)
        {
            _isCoroutineRunning = true;
            yield return new WaitForSeconds(1.5f);
        }

        _isCoroutineRunning = false;
        _currentUnit.UnitIsMoving = true;
    }

    private void Move()
    {
        if (_isCoroutineRunning) 
            return;

        var targetEnemy = _currentUnit switch
        {
            Enemy unit => unit.Enemies.OrderBy(e =>
                UIManager.GetDistance(_currentUnit.OccupiedTile, e.OccupiedTile)).FirstOrDefault(),
            Ally unit => unit.Enemies.OrderBy(e =>
                UIManager.GetDistance(_currentUnit.OccupiedTile, e.OccupiedTile)).FirstOrDefault(),
            _ => null
        };

        if (targetEnemy != null)
            _gameController.HandleTileClick(targetEnemy.OccupiedTile);
    }
}