using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AI : MonoBehaviour
{
    [SerializeField] private Button _endTurnButton;
    private GameController _gameController;
    private GameModel _gameModel;

    private void OnEnable()
    {
        _gameController = gameObject.GetComponentInParent<GameController>();
        _gameModel = gameObject.GetComponentInParent<GameModel>();
    }

    public void UpdateUI<T>(T _activePlayer, Button button) where T : Unit
    {
        var currentPlayer = _activePlayer;

        if (currentPlayer == null)
        {
            // если ActivePlayer == null, то делаем кнопку недоступной
            button.interactable = false;
            return;
        }

        var canEndTurn = currentPlayer.Status == UnitStatus.Moved;

        if (!canEndTurn)
        {
            var movedUnits = _gameController.Grid.AllUnits.Where(u => u.Status == UnitStatus.Moved && u == currentPlayer).ToList();
            if (movedUnits.Count == 0)
            {
                canEndTurn = true;
            }
        }

        button.interactable = canEndTurn;

        // Дополнение: проверяем, выбран ли юнит и сделан ли уже ход
        var isUnitSelected = currentPlayer != null;
        if (isUnitSelected && !(currentPlayer.Status == UnitStatus.Moved))
        {
            canEndTurn = false;
        }
    }


    public void Move(Unit unit)
    {
        //_gameModel.ActivePlayer = unit;
        var localController = _gameController;
        var localGrid = localController.Grid;
        var localInteractor = localController.Interactor;
        var localSelector = localController.Selector;

        var availableMoves = localSelector.GetAvailableMoves(unit.OccupiedTile, unit.MovementPoints);
        if (availableMoves.Count == 0)
        {
            unit.Status = UnitStatus.Moved;
            return;
        }

        // Находим всех враждебных юнитов
        var enemies = localGrid.AllUnits.Where(u => u.Type != unit.Type).ToArray();

        // Выбираем ближайшего врага
        var targetEnemy = enemies.OrderBy(e => localInteractor.PathConstructor.GetDistance(unit.OccupiedTile, e.OccupiedTile)).FirstOrDefault();

        // Если нашли врага, движемся к нему
        if (targetEnemy != null)
        {
            var targetTile = targetEnemy.OccupiedTile;
            localInteractor.PathConstructor.FindPathToTarget(unit.OccupiedTile, targetTile, out List<Tile> Path, _gameController.Grid);
            _gameController.MoveUnitAlongPath(unit, Path);
        }
        else
        {
            // Если врагов нет, движемся к случайной доступной клетке
            var randomIndex = Random.Range(0, availableMoves.Count);
            var targetTile = availableMoves[randomIndex];
            localInteractor.PathConstructor.FindPathToTarget(unit.OccupiedTile, targetTile, out List<Tile> Path, _gameController.Grid);
            _gameController.MoveUnitAlongPath(unit, Path);
        }

        // Обновляем состояние юнита
        localInteractor.UpdateUnit(unit);
        localSelector.UnselectUnit(unit); // Можно убрать наверное

        // Обновляем доступность ячеек после перемещения
        unit.OccupiedTile.DeselectTile();
        unit.Status = UnitStatus.Moved;
        unit.Stats.MovementPoints = unit.Stats.MovementRange; // Добавил, должно работать

        _gameModel.EndTurn();
    }
}
