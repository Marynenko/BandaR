using DG.Tweening;
using System;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    #region Variables

    // Fields
    [SerializeField] private UnitStats _stats;

    // Constants
    private const float MAX_DISTANCE = 4f;
    private const float HEIGHT_TO_PUT_UNIT_ON_TILE = 0.8f;

    // Private fields
    private Tile _occupiedTile;

    // Public properties
    public UnitStats Stats => _stats;
    public UnitType Type => _stats.Type;
    public int MovementPoints => Stats.MovementPoints;
    public int MovementRange => Stats.MovementRange;
    public Tile OccupiedTile => _occupiedTile; // Можно сослать на _occupiedTile
    public UnitStatus Status { get; set; } = UnitStatus.Available; // Initialize to Waiting

    // Events
    public event Action<Unit> OnMoved; // Add event for unit movement
    public event Action<Unit> OnDeath; // Add event for unit death
    #endregion

    #region Initialization
    public virtual Unit GetUnitType() => this;

    public void InitializeUnit(Tile startTile)
    {
        // Установка позиции юнита на центр ячейки с учетом высоты модели
        transform.position = startTile.transform.position + Vector3.up * HEIGHT_TO_PUT_UNIT_ON_TILE;
        // Установка текущей ячейки для юнита
        _occupiedTile = startTile;

        _occupiedTile.State = Type == UnitType.Player ? TileState.OccupiedByPlayer : TileState.OccupiedByEnemy;
        _occupiedTile.UnitOn = true;
        Status = UnitStatus.Unavailable;

    }
    #endregion

    #region Action Movement
    public bool CanMoveToTile(Tile targetTile, out float distanceSq)
    {
        distanceSq = (OccupiedTile.transform.position - targetTile.transform.position).sqrMagnitude;
        return OccupiedTile != targetTile &&
               distanceSq <= MAX_DISTANCE &&
               Status != UnitStatus.Moved &&
               Stats.MovementPoints > 1 &&
               !targetTile.UnitOn &&
               Stats.MovementPoints >= targetTile.MovementCost;
    }

    public void MoveToTile(Tile targetTile, float distanceSq)
    {
        _occupiedTile = targetTile;
        _stats.MovementPoints -= 1;
        //OnUnitAction?.Invoke(ActionType.Move, this, targetTile);

        // Вычисляем позицию для перемещения с учетом высоты юнита
        Vector3 newPosition = new(targetTile.transform.position.x, transform.position.y, targetTile.transform.position.z);

        //Запускаем анимацию перемещения
        transform.DOMove(newPosition, Mathf.Sqrt(distanceSq) / MAX_DISTANCE) // Use Mathf.Sqrt for distance
                 .SetEase(  Ease.Linear)
                 .OnComplete(() =>
                 {
                     transform.position = newPosition;
                     OnMoved?.Invoke(this); // Raise the OnMoved event after moving
                 });

        //Raise the event after moving
        OnUnitMoved(this);
    }

    #endregion

    #region Action ATTACK

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

    private void TakeDamage(int damage)
    {
        _stats.Health = -damage; // используем метод SetHealth() для изменения здоровья
        if (_stats.Health <= 0)
        {
            _stats.Health = 0;
            Die();
        }
    }

    public bool IsAlive() => _stats.Health > 0;

    private Action Die()
    {
        // Доработать
        Destroy(gameObject);
        return delegate { };

    }

    #endregion

    #region Part of unusable code

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

    // TODO эта часть скрипта связана с меню UI v. 1.4.7.7.X Когда будет работать код, тогда можно сюда лезть
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

    protected virtual void OnOnDeath(Unit obj)
    {
        OnDeath?.Invoke(obj);
    }
    #endregion
}
