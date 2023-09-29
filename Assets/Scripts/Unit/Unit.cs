using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    #region Variables

    // Fields
    [SerializeField] private UnitStats _stats;

    // Constants
    private const float MAX_DISTANCE = 4f;
    private const float HEIGHT_TO_PUT_UNIT_ON_TILE = 0.68f;

    // Private fields
    private Tile _occupiedTile;

    // Public properties
    public UnitStats Stats => _stats;
    public UnitType Type => _stats.Type;
    public int MovementPoints => Stats.MovementPoints;
    public int MovementRange => Stats.MovementRange;
    public Tile OccupiedTile => _occupiedTile; // Можно сослать на _occupiedTile
    public UnitStatus Status { get; set; } = UnitStatus.Available; // Initialize to Waiting

    // public fields
    public Vector2Int SpawnCellVector2Int;
    // public List<Tile> AvailableMoves;
    public HashSet<Tile> AvailableMoves;
    public bool UnitIsMoving = false;

    #endregion

    #region Initialization

    public virtual Unit GetUnitType() => this;

    public void InitializeUnit(Tile[,] tiles)
    {
        var startTile = CompareSpawnPosToTile(tiles);
        // Установка позиции юнита на центр ячейки с учетом высоты модели
        transform.position = startTile.transform.position + Vector3.up * HEIGHT_TO_PUT_UNIT_ON_TILE;
        // transform.position = spawnCell.position + Vector3.up * HEIGHT_TO_PUT_UNIT_ON_TILE;
        // Установка текущей ячейки для юнита
        _occupiedTile = startTile;

        _occupiedTile.State = Type == UnitType.Player ? TileState.OccupiedByPlayer : TileState.OccupiedByEnemy;
        _occupiedTile.UnitOn = true;
        Status = UnitStatus.Unavailable;
    }

    private Tile CompareSpawnPosToTile(Tile[,] tiles)
    {
        return tiles.Cast<Tile>().FirstOrDefault(tile => tile.Coordinates.x == SpawnCellVector2Int.x
                                                         && tile.Coordinates.y == SpawnCellVector2Int.y);
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
        // Вычисляем позицию для перемещения с учетом высоты юнита
        Vector3 newPosition = new(targetTile.transform.position.x, transform.position.y,
            targetTile.transform.position.z);

        const float movementSpeed = 5.5f;
        //Запускаем анимацию перемещения
        transform.DOMove(newPosition, Mathf.Sqrt(distanceSq) / MAX_DISTANCE * movementSpeed)
            .SetEase(Ease.Linear)
            .OnComplete(() => { transform.position = newPosition; });
        
        _occupiedTile = targetTile;
        _stats.MovementPoints -= 1;

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

    #endregion
}