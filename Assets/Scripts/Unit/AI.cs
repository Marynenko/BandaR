using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AI : MonoBehaviour
{
    [SerializeField] private GameController _gameController;
    [SerializeField] private GameModel _gameModel;
    [SerializeField] private Button _endTurnButton;

    private void Update()
    {
        UpdateUI(_gameModel.ActivePlayer, _endTurnButton);
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
    }


    public void Move(Unit unit)
    {
        _gameModel.ActivePlayer = unit;
        var availableMoves = _gameController.Selector.GetAvailableMoves(unit.CurrentCell, unit.MovementPoints);
        if (availableMoves.Count == 0)
        {
            unit.Status = UnitStatus.Moved;
            return;
        }

        // Выбираем случайную доступную клетку для перемещения
        var randomIndex = Random.Range(0, availableMoves.Count);
        var targetCell = availableMoves[randomIndex];

        // Выполняем перемещение на выбранную клетку
        _gameController.Interactor.PathConstructor.FindPathToTarget(unit.CurrentCell, targetCell, out List<Cell> Path, _gameController.Grid);

        _gameController.MoveUnitAlongPath(unit, Path);

        // Обновляем состояние юнита
        unit.Status = UnitStatus.Moved;
        _gameController.Interactor.UpdateUnit(unit);

        // Обновляем доступность ячеек после перемещения
        _gameModel.ResetCellsAvailability();
        var units = _gameController.Grid.AllUnits.Where(u => u == _gameModel.ActivePlayer).ToList();
        _gameModel.EndTurn();
    }

}
