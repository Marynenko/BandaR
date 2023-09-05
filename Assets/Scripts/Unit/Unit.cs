using DG.Tweening;
using System;
using UnityEngine;

public enum ActionType
{
    Select,
    Move,
    Attack,
    SpecialAbility
}

public class Unit : MonoBehaviour
{
    #region Variables
    [SerializeField] private UnitStats _stats;
    private const float MAX_DISTANCE = 3f;

    public UnitStatus Status;
    public Grid Grid { get; private set; }
    public Tile CurrentCell { get; private set; }
    public UnitStats Stats { get { return _stats; } }
    public UnitType Type { get { return _stats.Type; } }

    public int MovementPoints
    {
        get { return Stats.MovementPoints; }
        private set { MovementPoints = value; }
    }
    public int MovementRange { get { return Stats.MovementRange; } }
    #endregion

    #region Public Methods    
    public virtual Unit GetUnitType() => this;

    public void InitializeUnit(Grid grid, Tile cell)
    {
        Grid = grid;
        // Установка позиции юнита на центр ячейки с учетом высоты модели
        transform.position = cell.transform.position + Vector3.up * 0.8f;
        // Установка текущей ячейки для юнита
        CurrentCell = cell;

        CurrentCell.CurrentState = Type == UnitType.Player ? State.OccupiedByPlayer : State.OccupiedByEnemy;
        CurrentCell.UnitOn = true;
        Status = UnitStatus.Unavailable;
    }

    public bool CanMoveToCell(Tile cell) =>
        CurrentCell != cell &&
        Vector3.Distance(CurrentCell.transform.position, cell.transform.position) <= MAX_DISTANCE &&
        Status != UnitStatus.Moved &&
        _stats.MovementPoints > 1 &&
        !cell.UnitOn &&
        _stats.MovementPoints >= cell.MovementCost;

    public void MoveToCell(Tile targetCell)
    {
        CurrentCell = targetCell;
        _stats.MovementPoints -= 1;
        //OnUnitAction?.Invoke(ActionType.Move, this, targetCell);

        // Вычисляем позицию для перемещения с учетом высоты юнита
        Vector3 newPosition = new(targetCell.transform.position.x, transform.position.y, targetCell.transform.position.z);

        // Запускаем анимацию перемещения       
        transform.DOMove(newPosition, Vector3.Distance(transform.position, newPosition) / MAX_DISTANCE)
                 .SetEase(Ease.Linear)
                 .OnComplete(() => transform.position = newPosition);
    }


    public bool CanAttack(Unit targetUnit) =>
        targetUnit != null &&
        targetUnit.Type == UnitType.Enemy &&
        Vector3.Distance(transform.position, targetUnit.transform.position) <= _stats.AttackRange;

    public void Attack(Unit target)
    {
        if (CanAttack(target))
        {
            target.TakeDamage(_stats.AttackDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        _stats.Health =- damage; // используем метод SetHealth() для изменения здоровья
        if (_stats.Health <= 0)
        {
            _stats.Health = 0;
            Die(this);
        }
    }

    public bool IsAlive() => _stats.Health > 0;

    public Action Die(Unit unit)
    {
        // Доработать
        Destroy(gameObject);
        return delegate { };
    }

    public void UpdateVisuals()
    {
        //healthBar.fillAmount = (float)Health / MaxHealth;
    }

    public void OnUnitMoved(Unit movedUnit)
    {
        // Действия, которые должны произойти при перемещении другого юнита на соседнюю клетку
    }

    public void SetAvailability()
    {
        _stats.MovementPoints = _stats.MovementRange;
        Status = UnitStatus.Moved;
    }


    public bool IsActionAvailableForUnit(Unit unit, ActionType actionType)
    {
        switch (actionType)
        {
            case ActionType.Move:
                // Check if the unit can move in the current situation
                break;
            case ActionType.Attack:
                // Check if the unit can attack in the current situation
                break;
            case ActionType.SpecialAbility:
                // Check if the unit can use its special ability in the current situation
                break;
            default:
                // Handle invalid action type
                break;
        }

        return false;
    }

    #endregion
}
