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
    
    // Fields
    [SerializeField] private UnitStats _stats;

    // Constants
    private const float MAX_DISTANCE = 3f;

    // Private fields
    private Tile _occupiedTile;

    // Public properties
    public UnitStats Stats { get { return _stats; } }
    public UnitType Type { get { return _stats.Type; } }
    public int MovementPoints
    {
        get { return Stats.MovementPoints; }
        private set { MovementPoints = value; }
    }
    public int MovementRange { get { return Stats.MovementRange; } }    
    public Grid Grid { get; private set; }
    public Tile OccupiedTile { get { return _occupiedTile; } } // Можно сослать на _occupiedTile

    // Other variables
    public UnitStatus Status;

    #endregion

    #region Initialization
    public virtual Unit GetUnitType() => this;

    public void InitializeUnit(Grid grid, Tile startTile)
    {
        Grid = grid;
        // Установка позиции юнита на центр ячейки с учетом высоты модели
        transform.position = startTile.transform.position + Vector3.up * 0.8f;
        // Установка текущей ячейки для юнита
        _occupiedTile = startTile;

        _occupiedTile.CurrentState = Type == UnitType.Player ? State.OccupiedByPlayer : State.OccupiedByEnemy;
        _occupiedTile.UnitOn = true;
        Status = UnitStatus.Unavailable;
    }
    #endregion

    #region Action Checks
    public bool CanMoveToTile(Tile tile) =>
        OccupiedTile != tile &&
        Vector3.Distance(OccupiedTile.transform.position, tile.transform.position) <= MAX_DISTANCE &&
        Status != UnitStatus.Moved &&
        _stats.MovementPoints > 1 &&
        !tile.UnitOn &&
        _stats.MovementPoints >= tile.MovementCost;

    public void MoveToTile(Tile targetTile)
    {
        _occupiedTile = targetTile;
        _stats.MovementPoints -= 1;
        //OnUnitAction?.Invoke(ActionType.Move, this, targetTile);

        // Вычисляем позицию для перемещения с учетом высоты юнита
        Vector3 newPosition = new(targetTile.transform.position.x, transform.position.y, targetTile.transform.position.z);

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
