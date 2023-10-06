using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class AI : MonoBehaviour
{
    private GameController _gameController;
    private GameModel _gameModel;
    private Grid _grid;
    private Unit _currentUnit;

    public Tile Target;

    private bool _isCoroutineRunning = false;

    private void OnEnable()
    {
        _gameController = gameObject.GetComponentInParent<GameController>();
        _gameModel = gameObject.GetComponentInParent<GameModel>();
        _grid = _gameController.Grid;
    }

    private void Update()
    {
        if (_currentUnit == null) return;
        StartMove();
    }

    public void InitializeAI(Unit unit)
    {
        _currentUnit = unit;
        // _currentUnit.UnitIsMoving = true;
    }

    private void StartMove()
    {
        if (_currentUnit.Status != UnitStatus.AIMove) return;
        if (!_isCoroutineRunning && _currentUnit.UnitIsMoving)
        {
            Move();

            if (!_isCoroutineRunning && !_currentUnit.UnitIsMoving)
            {
                if (!_gameModel.HandleEndTurnButtonClicked(_currentUnit)) return;
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
        var localSelector = _gameController.Selector;
        if (localSelector.SelectedUnit == null)
            localSelector.SelectUnit(_currentUnit);
        if (!_currentUnit.UnitIsMoving)
        {
            // _isCoroutineRunning = true;
            yield return new WaitForSeconds(1.5f);
        }

        _currentUnit.UnitIsMoving = true;
    }

    private void Move()
    {
        var localSelector = _gameController.Selector;

        if (_isCoroutineRunning) return;
        var enemies = _grid.AllUnits.Where(u => u.Stats.Type != _currentUnit.Stats.Type).ToArray();
        var targetEnemy = enemies.OrderBy(e =>
            localSelector.PathConstructor.GetDistance(_currentUnit.OccupiedTile, e.OccupiedTile)).FirstOrDefault();

        if (targetEnemy == null) return;
        _gameController.HandleTileClick(targetEnemy.OccupiedTile);
    }
}