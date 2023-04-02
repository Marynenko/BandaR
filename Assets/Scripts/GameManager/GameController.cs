using System;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour, IGameController
{
    [SerializeField] private Grid _grid;
    [SerializeField] private GridInteractor _interactor;
    [SerializeField] private GridGenerator _generator;

    public void HandleUnitClick(Unit unit)
    {
        var selectedUnit = _interactor.SelectedUnit;

        if (selectedUnit == null)
            if (unit.Type == UnitType.Player)
                _interactor.SelectUnit(unit);

        else if (selectedUnit.Equals(unit))
            return;
        else if (unit.Type == UnitType.Player)
            _interactor.SelectUnit(unit);
        else if (unit.Type == UnitType.Enemy && selectedUnit.Type == UnitType.Player)
            HandleUnitAttack(selectedUnit, unit);
        else
            _interactor.HandleUnitDeselection(selectedUnit, unit);
    }

    private void HandleUnitAttack(Unit selectedUnit, Unit targetUnit)
    {
        if (selectedUnit.Status != UnitStatus.Selected)
        {
            return;
        }

        if (targetUnit.Type == UnitType.Enemy && selectedUnit.CanAttack(targetUnit))
        {
            selectedUnit.Attack(targetUnit);

            if (targetUnit.Health <= 0)
            {
                _grid.RemoveUnit(targetUnit);
            }
            else
            {
                _interactor.UpdateUnit(targetUnit);
            }

            selectedUnit.CurrentCell.UnselectCell();
            _interactor.UnselectUnit(selectedUnit);


            // Update available moves after attack
            var availableMoves = _interactor.GetAvailableMoves(selectedUnit.CurrentCell, 1);
            _interactor.HighlightAvailableMoves(availableMoves, selectedUnit.CurrentCell.ColorMovementCell);
        }
    }

    public void HandleCellClick(Cell cell)
    {
        var selectedUnit = _interactor.SelectedUnit;

        if (selectedUnit == null || selectedUnit.Type != UnitType.Player || selectedUnit.Status != UnitStatus.Selected)
        {
            return;
        }

        if (cell == selectedUnit.CurrentCell)
        {
            return;
        }

        selectedUnit.CurrentCell.UnselectCell();
        _interactor.UnselectUnit(selectedUnit);

        var availableMoves = _interactor.AvailableMoves;

        if (!availableMoves.Contains(cell))
        {
            return;
        }

        var path = _interactor.FindPathToTarget(selectedUnit.CurrentCell, cell);
        if (path.Count == 0)
        {
            return;
        }

        _interactor.MoveUnitAlongPath(selectedUnit, path);

        _interactor.SelectUnit(selectedUnit);
        selectedUnit.CurrentCell.SelectCell();

        // проверяем соседство юнитов после каждого перемещения
        //if (AreUnitsAdjacent(selectedUnit, _interactor.AllUnits))
        //{
        //    // начинаем бой или выполняем нужные действия
        //}
    }




    public bool AreUnitsAdjacent(Unit unit1, Unit unit2)
    {
        var distance = Vector3.Distance(unit1.transform.position, unit2.transform.position);
        return distance <= 1f; // или другое значение, в зависимости от размеров клетки и модели юнитов
    }
}