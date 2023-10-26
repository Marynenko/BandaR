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
        if (_currentUnit == null) return;
        if (_currentUnit.Stats.Type is UnitType.Ally or UnitType.Enemy)
        {
            if (_currentUnit.Stats.StateFatigue >= 80 && !_isFinishMoveActive)
            {
                Debug.Log($"{_currentUnit.Stats.Name} won't move!");
                StartCoroutine(FinishMove(2f));
                return;
            }
            if (_isFinishMoveActive) return;

            StartMove();
        }
    }

    private IEnumerator FinishMove(float waitTime)
    {
        _isFinishMoveActive = true;
        yield return new WaitForSeconds(waitTime);
        _isFinishMoveActive = false;
        _currentUnit.UnitIsMoving = false;
        _currentUnit.Status = UnitStatus.Moved;
        UIManager.Instance.TurnManager.EndTurn();
        // todo TurnManager in UIManager.Instance from GridUI.Instance
        // _gameModel.HandleEndTurnButtonClicked(_currentUnit);
    }

    public void InitializeAI(Unit unit)
    {
        _currentUnit = unit;
    }

    private void StartMove()
    {
        if (_currentUnit.Status != UnitStatus.AIMove) return;
        if (!_isCoroutineRunning && _currentUnit.UnitIsMoving)
        {
            Move();

            if (!_isCoroutineRunning && !_currentUnit.UnitIsMoving)
            {
                var successfulFinish = _gameModel.HandleEndTurnButtonClicked(_currentUnit);
                if (!successfulFinish) return;
                _currentUnit = null;
                return;
            }
        }
        
        if (!_isCoroutineRunning && !_currentUnit.UnitIsMoving)
        {
            _isCoroutineRunning = true;
            StartCoroutine(SelectUnit());
        }
        
        if (!_currentUnit.UnitIsMoving) return;
        _isCoroutineRunning = false;
        _currentUnit.UnitIsMoving = true;
    }

    private IEnumerator SelectUnit()
    {
        if (_gameController.Selector.SelectedUnit == null)
            _gameController.Selector.SelectUnit(_currentUnit);
        if (!_currentUnit.UnitIsMoving)
        {
            // _isCoroutineRunning = true;
            yield return new WaitForSeconds(1.5f);
        }

        _currentUnit.UnitIsMoving = true;
    }

    private void Move()
    {
        if (_isCoroutineRunning) return;

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