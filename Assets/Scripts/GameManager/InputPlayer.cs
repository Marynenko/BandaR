using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class InputPlayer : MonoBehaviour, IInputHandler
{
    [SerializeField] private GameController _gameController;
    [SerializeField] private GameModel _gameModel;

    public void HandleLeftClick(Vector3 mousePosition)
    {
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            if (hit.collider.TryGetComponent(out Unit unit))
                _gameController.HandleUnitClick(unit);
            else if (hit.collider.TryGetComponent(out Cell cell))
                _gameController.HandleCellClick(cell);
        }
        //_gameController.UnhighlightAvailableMoves();
    }

    public void HandleEndTurnButtonClicked()
    {
        // Проверяем, выбран ли игрок
        var selectedUnit = _gameController.Selector.SelectedUnit;
        if (selectedUnit != null)
        {
            Debug.Log("You must unselect the current unit before ending the turn.");
            return;
        }

        

        // Проверяем, был ли игрок перемещен в этом ходе
        var movedUnits = _gameController.Grid.AllUnits.Where(u => u.Status == UnitStatus.Moved).ToList();
        if (movedUnits.Count == 0)
        {
            Debug.Log("You can't end the turn until all units have moved.");
            return;
        }

        // Передаем ход следующему игроку
        _gameModel.EndTurn();
    }


}
