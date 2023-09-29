using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI : MonoBehaviour
{
    private GameController _gameController;
    private GameModel _gameModel;
    private Grid _grid;
    private Unit _unit;
    private bool _isCoroutineRunning = false;

    private void OnEnable()
    {
        _gameController = gameObject.GetComponentInParent<GameController>();
        _gameModel = gameObject.GetComponentInParent<GameModel>();
        _grid = _gameController.Grid;
    }

    private void Update()
    {
        if (_unit == null || _isCoroutineRunning) return;
        if (_unit.UnitIsMoving)
        {
            StartCoroutine(Move(_unit));
            _isCoroutineRunning = true;
        }
    }

    public void StartMove(Unit unit)
    {
        _unit = unit;
        unit.UnitIsMoving = true;
    }

    private IEnumerator Move(Unit unit)
    {
        var localSelector = _gameController.Selector;
        // Выбор юнита
        if (localSelector.SelectedUnit == null)
            localSelector.SelectUnit(unit);
        yield return new WaitForSeconds(1.5f);
        
        _isCoroutineRunning = false;

        if (!_isCoroutineRunning)
        {
            // Находим всех враждебных юнитов
            var enemies = _grid.AllUnits.Where(u => u.Type != unit.Type).ToArray();

            // Выбираем ближайшего врага
            var targetEnemy = enemies.OrderBy(e =>
                localSelector.PathConstructor.GetDistance(unit.OccupiedTile, e.OccupiedTile)).FirstOrDefault();

            // Если нашли врага, движемся к нему
            if (targetEnemy != null)
            {
                var targetTile = targetEnemy.OccupiedTile;
                _gameController.HandleTileClick(targetTile);
            }
        }

        // if (!unit.UnitIsMoving)
        //     localSelector.MoveMore();

        if(!_isCoroutineRunning && unit.UnitIsMoving)
            _gameModel.EndTurn();
    }
}