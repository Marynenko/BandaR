using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AI : MonoBehaviour
{
    private GameController _gameController;
    private GameModel _gameModel;
    private Grid _grid;
    
    private void OnEnable()
    {
        _gameController = gameObject.GetComponentInParent<GameController>();
        _gameModel = gameObject.GetComponentInParent<GameModel>();
        _grid = _gameController.Grid;
    }

    public void StartMove(Unit unit)
    {
        StartCoroutine(Move(unit));
    }

    public IEnumerator Move(Unit unit)
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
            _gameController.Selector.PathConstructor.FindPathToTarget(unit.OccupiedTile, targetTile, out List<Tile> path);
            _gameController.MoveUnitAlongPath(unit, path);
            unit.Status = UnitStatus.Moved;
        }
        else
        {
            _gameModel.EndTurn();
        }

        _gameController.Selector.UnselectUnit(unit);
        _gameModel.EndTurn();
    }
}
