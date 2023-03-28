using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public enum UnitType
{
    Player,
    Enemy
}

public enum UnitStatus
{
    Selected,
    Unselected,
    Moved,
    Moving
}

public class Unit : MonoBehaviour
{
    #region Variables
    private const float POSITION_Y = .8f;
    private const float MAX_DISTANCE = 3f;
    private const float MOVE_ANIMATION_DURATION = 1f;

    public delegate void UnitActionEventHandler(UnitActionType actionType, Unit unit, Cell cell);
    public static event UnitActionEventHandler OnUnitAction;

    public Cell CurrentCell;
    public UnitType Type;
    public UnitStatus Status = UnitStatus.Unselected;

    public int ID { get; private set; }
    public int Health { get; private set; } = 100;
    public int AttackDamage { get; private set; } = 10;
    public float AttackRange { get; private set; } = 1.5f;
    public int MovementPoints { get; private set; } = 1;
    public int MovementRange { get; private set; } = 3;

    #endregion

    #region Public Methods    

    [ContextMenu("Initialize Unit")]
    public void InitializeUnit()
    {
        var ray = new Ray(transform.position, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, MAX_DISTANCE))
            if (hit.collider.GetComponent<Cell>())
            {
                transform.position = new Vector3(hit.transform.position.x, POSITION_Y, hit.transform.position.z);
                CurrentCell = hit.collider.GetComponent<Cell>();
                CurrentCell.UnitOn = StatusUnitOn.Yes;
                Status = UnitStatus.Unselected;
            }
    }

    public bool CanMoveToCell(Cell cell, Unit unit, List<Cell> AvailableMoves)
    {
        if (unit.CurrentCell == cell) return false;
        if (Vector3.Distance(unit.transform.position, cell.transform.position) > MAX_DISTANCE) return false;
        if (unit.Status == UnitStatus.Moved) return false;
        if (!AvailableMoves.Contains(cell)) return false;
        if (cell.UnitOn != StatusUnitOn.No) return false; // Либо нет либо да
        if (unit.MovementPoints < cell.MovementCost) return false;
        return true;
    }


    public void MoveToCell(Cell targetCell, GridInteractor grid)
    {
        if (CurrentCell == targetCell) return;
        if (Vector3.Distance(transform.position, targetCell.transform.position) > MAX_DISTANCE) return;

        if (Status != UnitStatus.Moved)
        {
            Status = UnitStatus.Moved;
            //grid.RemoveUnit(this);
            //grid.AddUnit(this);
            CurrentCell = targetCell;

            if (OnUnitAction != null)
            {
                OnUnitAction(UnitActionType.Move, this, targetCell);
            }

            // Вычисляем позицию для перемещения с учетом высоты юнита
            Vector3 newPosition = new Vector3(targetCell.transform.position.x, transform.position.y, targetCell.transform.position.z);

            // Запускаем анимацию перемещения
            var pos = transform.position;
            var curPos = CurrentCell.transform.position;
            transform.DOMove(newPosition, Vector3.Distance(transform.position, newPosition) / MAX_DISTANCE)
                     .SetEase(Ease.Linear)
                     .OnComplete(() => transform.position = newPosition);
        }
    }


    public bool CanAttack(Unit targetUnit)
    {
        if (targetUnit == null || targetUnit.Type != UnitType.Enemy)
        {
            return false;
        }

        var distance = Vector3.Distance(transform.position, targetUnit.transform.position);
        return distance <= AttackRange;
    }


    public void Attack(Unit target)
    {
        if (Vector3.Distance(transform.position, target.transform.position) <= AttackRange)
        {
            target.TakeDamage(AttackDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        if (Health <= 0)
        {
            Health = 0;
            Die();
            Destroy(gameObject);
        }
    }

    public void Die()
    {
        //_gridInteractor.RemoveUnit(this);
    }

    public void UpdateVisuals()
    {
        //healthBar.fillAmount = (float)Health / MaxHealth;
    }
    #endregion
}
