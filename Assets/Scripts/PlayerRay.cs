using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRay : MonoBehaviour
{
    [SerializeField] private GridInteractor _gridInteractor;

    public event Action<Unit> OnPlayerSelected;
    public event Action<Unit> OnEnemySelected;
    public event Action<Unit> OnUnitDeselected;
    public event Action<Unit, Cell> OnUnitMoved;

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

                if (unit != null)
                {
                    if (unit.Status == UnitStatus.Unselected)
                    {
                        if (unit.Type == UnitType.Player)
                        {
                            // выбираем Player и клетку, на которой он находится
                            _gridInteractor.SelectUnit(unit);
                            var currentCell = unit.CurrentCell;
                            _gridInteractor.SelectCell(currentCell, unit.Type);
                            currentCell.UnitOn = UnitOnStatus.Yes;
                            unit.Status = UnitStatus.Selected;
                            OnPlayerSelected?.Invoke(unit);
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
                            OnEnemySelected?.Invoke(unit);
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
                            OnUnitDeselected?.Invoke(unit);
                        }
                    }
                }

                else if (cell != null)
                {
                    var selectedUnit = _gridInteractor.SelectedUnit;

                    if (selectedUnit != null)
                    {
                        if (cell != selectedUnit.CurrentCell)
                        {
                            if (_gridInteractor.CanMoveToCell(selectedUnit, cell))
                            {
                                _gridInteractor.MoveUnitToCell(selectedUnit, cell);
                                OnUnitMoved?.Invoke(selectedUnit, cell);
                            }
                            else
                            {
                                _gridInteractor.UnselectUnit(selectedUnit);
                                selectedUnit.CurrentCell.ChangeColor(selectedUnit.CurrentCell.CellStandardColor);
                                selectedUnit.CurrentCell.UnitOn = UnitOnStatus.No;
                                OnUnitDeselected?.Invoke(selectedUnit);
                                if (selectedUnit.Type == UnitType.Enemy)
                                {
                                    OnEnemySelected?.Invoke(selectedUnit);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
