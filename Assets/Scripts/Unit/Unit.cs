using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

public enum UnitStatus
{
    //Selected,
    //Unselected,
    Moved,
    Available,
    Unavailable,
    AIMove
}

public enum UnitType
{
    Player,
    Enemy
}

public enum ActionType
{
    Select,
    Move,
    Attack,
    SpecialAbility
}

public class Unit : MonoBehaviour, IUnit
{
    #region Variables
    [SerializeField] private UnitStats _stats;
    private const float MAX_DISTANCE = 3f;
    
    public UnitStatus Status;

    public Grid Grid { get; private set; }
    public Tile CurrentCell { get; private set; }
    public UnitStats Stats { get { return _stats; } }
    public int ID { get { return Stats.ID; } }
    public UnitType Type { get { return Stats.Type; } }
    public float ViewRange { get { return Stats.ViewRange; } }
    public float AttackRange { get { return Stats.AttackRange; } }
    public int Health { get { return Stats.Health; } }
    public int AttackDamage { get { return Stats.AttackDamage; } }
    public int MovementPoints { get { return Stats.MovementPoints; } 
        private set { MovementPoints = value; } }
    public int MovementRange { get { return Stats.MovementRange; } }

    #endregion

    #region Public Methods    
    public virtual Unit GetUnitType() => this;

    public void InitializeUnit(Grid grid, Tile cell)
    {
        Grid = grid;
        
        // Установка позиции юнита на центр ячейки с учетом высоты модели
        Vector3 unitPosition = cell.transform.position;
        unitPosition.y += 0.8f;
        transform.position = unitPosition;
        
        // Установка текущей ячейки для юнита
        CurrentCell = cell;
        var state = Type == UnitType.Player ? State.OccupiedByPlayer : State.OccupiedByEnemy;

        Status = UnitStatus.Unavailable;

        CurrentCell.CurrentState = state;
        CurrentCell.UnitOn = true;
    }

    public bool CanMoveToCell(Tile cell)
    {
        if (CurrentCell == cell) return false;
        if (Vector3.Distance(CurrentCell.transform.position, cell.transform.position) > MAX_DISTANCE) return false;
        if (Status == UnitStatus.Moved) return false;
        if (MovementPoints <= 1) return false;
        if (cell.UnitOn == true) return false;
        if (Stats.MovementPoints < cell.MovementCost) return false;
        return true;
    }

    public void MoveToCell(Tile targetCell)
    {
        //Status = UnitStatus.Moved; // Сюда же так же можно поставить Unselected
        CurrentCell = targetCell;
        Stats.MovementPoints -= 1;
        //OnUnitAction?.Invoke(ActionType.Move, this, targetCell);

        // Вычисляем позицию для перемещения с учетом высоты юнита
        Vector3 newPosition = new(targetCell.transform.position.x, transform.position.y, targetCell.transform.position.z);

        // Запускаем анимацию перемещения
        var pos = transform.position;
        transform.DOMove(newPosition, Vector3.Distance(transform.position, newPosition) / MAX_DISTANCE)
                 .SetEase(Ease.Linear)
                 .OnComplete(() => pos = newPosition);
    }


    public bool CanAttack(Unit targetUnit)
    {
        var unit = targetUnit.GetUnitType();
        if (targetUnit == null || (unit as Unit).Type != UnitType.Enemy)
            return false;

        var distance = Vector3.Distance(transform.position, (targetUnit as Unit).transform.position);
        return distance <= AttackRange;
    }

    public void Attack(Unit target)
    {
        var unit = target.GetUnitType();
        if (Vector3.Distance(transform.position, (unit as Unit).transform.position) <= AttackRange)
            target.TakeDamage(AttackDamage);
    }

    public void TakeDamage(int damage)
    {
        Stats.Health = Stats.Health - damage; // используем метод SetHealth() для изменения здоровья
        if (Stats.Health <= 0)
        {
            Stats.Health = 0;
            Die(this);
            Destroy(gameObject);
        }
    }

    public Action Die(Unit unit)
    {
        // Доработать        
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

    public bool IsAlive()
    {
        return Health > 0;
    }

    public void SetAvailability()
    {
        MovementPoints = MovementRange;
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
