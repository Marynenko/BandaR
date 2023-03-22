using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRay : MonoBehaviour
{
    [SerializeField] private GridInteractor _gridInteractor;

    public event Action<Unit, UnitType> OnUnitSelected;
    public event Action<UnitActionType, Unit, Cell> OnUnitAction;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                var unit = hit.collider.GetComponent<Unit>();
                var cell = hit.collider.GetComponent<Cell>();
                Debug.Log(Input.mousePosition + " " + hit.collider.name);

                if (unit != null) // Если Unit существует
                {
                    if (unit.Status == UnitStatus.Unselected) // Если Unit не выбран
                    {
                        if (unit.Type == UnitType.Player) // Если тип = Игрок
                        {
                            // выбираем Player и клетку, на которой он находится
                            _gridInteractor.SelectUnit(unit);
                            var currentCell = unit.CurrentCell;
                            _gridInteractor.SelectCell(currentCell, unit.Type);
                            currentCell.UnitOn = UnitOnStatus.Yes;
                            unit.Status = UnitStatus.Selected;
                            OnUnitSelected?.Invoke(unit, unit.Type);
                        }
                        else if (unit.Type == UnitType.Enemy)
                        {
                            // выбираем Enemy и подсвечиваем клетку, на которой он находится
                            _gridInteractor.SelectUnit(unit);
                            var currentCell = unit.CurrentCell;
                            currentCell.ChangeColor(currentCell.CellEnemyOnColor);
                            _gridInteractor.SelectCell(currentCell, unit.Type);
                            currentCell.UnitOn = UnitOnStatus.Yes;
                            unit.Status = UnitStatus.Selected;
                            OnUnitSelected?.Invoke(unit, unit.Type);
                        }
                    }
                    else if (unit.Status == UnitStatus.Selected)
                    {
                        if (unit.Type == UnitType.Player)
                        {
                            // отменяем выбор Player
                            _gridInteractor.UnselectUnit(unit);
                            unit.Status = UnitStatus.Unselected;
                            unit.CurrentCell.ChangeColor(unit.CurrentCell.CellStandardColor);
                            unit.CurrentCell.UnitOn = UnitOnStatus.No;
                            OnUnitSelected?.Invoke(unit, unit.Type);
                        }
                    }
                }

                else if (cell != null)
                {
                    var selectedUnit = _gridInteractor.SelectedUnit;

                    if (cell != selectedUnit.CurrentCell)
                    {
                        // получаем список возможных ходов для текущей клетки и типа юнита
                        List<Cell> availableMoves = _gridInteractor.GetAvailableMoves(selectedUnit.CurrentCell, selectedUnit.Type, selectedUnit.MaxMoves);

                        if (availableMoves.Contains(cell))
                        {   
                            // перемещаем юнита на выбранную клетку
                            _gridInteractor.MoveUnit(selectedUnit, cell);
                            selectedUnit.Status = UnitStatus.Unselected;
                            selectedUnit.CurrentCell.UnitOn = UnitOnStatus.No;
                            _gridInteractor.UnselectUnit(selectedUnit);
                            OnUnitAction?.Invoke(UnitActionType.Move, selectedUnit, cell);
                        }
                        else
                        {
                            // сбрасываем выбор юнита
                            _gridInteractor.UnselectUnit(selectedUnit);
                            selectedUnit.CurrentCell.ChangeColor(selectedUnit.CurrentCell.CellStandardColor);
                            selectedUnit.CurrentCell.UnitOn = UnitOnStatus.No;
                            OnUnitSelected?.Invoke(selectedUnit, selectedUnit.Type);
                        }
                    }
                }
            }
        }
    }
}

public enum UnitActionType
{
    Move
}
