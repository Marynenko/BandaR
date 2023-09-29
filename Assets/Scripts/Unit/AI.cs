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

    private void OnEnable()
    {
        _gameController = gameObject.GetComponentInParent<GameController>();
        _gameModel = gameObject.GetComponentInParent<GameModel>();
        _grid = _gameController.Grid;
    }

    private void Update()
    {
        // if (_unit == null) return; // TODO в будущем через Update пробовать
        // if (_unit.UnitIsMoving)
        //     StartCoroutine(Move(_unit));
    }

    public void StartMove(Unit unit)
    {
        // _unit = unit;
        // unit.UnitIsMoving = true;
        StartCoroutine(Move(_unit));
    }

    private IEnumerator Move(Unit unit)
    {
        // Выбор юнита
        _gameController.Selector.SelectUnit(unit);
        yield return new WaitForSeconds(1.5f);

        // Находим всех враждебных юнитов
        var enemies = _grid.AllUnits.Where(u => u.Type != unit.Type).ToArray();

        // Выбираем ближайшего врага
        var targetEnemy = enemies.OrderBy(e =>
            _gameController.Selector.PathConstructor.GetDistance(unit.OccupiedTile, e.OccupiedTile)).FirstOrDefault();

        // Если нашли врага, движемся к нему
        if (targetEnemy != null)
        {
            var targetTile = targetEnemy.OccupiedTile;
            _gameController.Selector.PathConstructor.FindPathToTarget(unit.OccupiedTile, targetTile,
                out List<Tile> path);
            _gameController.MoveUnitAlongPath(unit, path);
            unit.Status = UnitStatus.Moved;
        }
        else
        {
            _gameModel.EndTurn();
        }

        _gameController.Selector.UnselectUnit(unit);
        GridUI.HighlightTiles(unit.AvailableMoves, TileState.Standard);
        _gameModel.EndTurn();
    }
}