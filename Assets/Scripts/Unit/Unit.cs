using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class Unit : SoundsManager
{
    #region Variables

    // Fields
    [SerializeField] private UnitStats _stats;

    // Constants
    private const float MAX_DISTANCE = 4f;
    private const float HEIGHT_TO_PUT_UNIT_ON_TILE = 0.68f;

    // Private fields

    // Public properties
    public UnitStats Stats => _stats;
    public UnitStatus Status { get; set; }
    public Tile OccupiedTile { get; private set; }

    // public fields
    public Tile Target;
    public UISign Sign;
    public Image Portrait;

    public Vector2Int SpawnCellVector2Int;
    public HashSet<Tile> AvailableMoves;
    public bool UnitIsMoving;

    #endregion

    #region Initialization

    public abstract void TrackAllEnemies();

    public void InitializeUnit(Tile[,] tiles, UIPortraitManager uiGroup)
    {
        var startTile = CompareSpawnPosToTile(tiles);
        // Установка позиции юнита на центр ячейки с учетом высоты модели
        transform.position = startTile.transform.position + Vector3.up * HEIGHT_TO_PUT_UNIT_ON_TILE;
        // Установка текущей ячейки для юнита
        OccupiedTile = startTile;

        Portrait = uiGroup.GetPlayerPortrait(this);

        GridUI.Instance.TurnManager.ShowPortrait(this); // off
        
        OccupiedTile.State = Stats.Type switch
        {
            UnitType.Player => TileState.OccupiedByPlayer,
            UnitType.Ally => TileState.OccupiedByAlly,
            _ => TileState.OccupiedByEnemy
        };
        
        OccupiedTile.Available = false;
        Status = UnitStatus.Unavailable;
    }

    private Tile CompareSpawnPosToTile(Tile[,] tiles) =>
        tiles.Cast<Tile>().FirstOrDefault(tile => tile.Coordinates.x == SpawnCellVector2Int.x
                                                  && tile.Coordinates.y == SpawnCellVector2Int.y);

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

        const float movementSpeed = 2.5f;
        //Запускаем анимацию перемещения

        // var sound = GetComponentInParent<SoundsManager>();
        // PlaySound(Sounds[0]);            
        transform.DOMove(newPosition, Mathf.Sqrt(distanceSq) / MAX_DISTANCE * movementSpeed)
            .SetEase(Ease.Linear)
            .OnComplete(() => { transform.position = newPosition; });

        OccupiedTile = targetTile;
        _stats.MovementPoints -= 1;
    }

    public bool CanMoveMore() => Stats.MovementPoints > 1;

    #endregion

    #region Action ATTACK

    private bool CanAttack(Unit targetUnit) =>
        targetUnit != null &&
        targetUnit.Stats.Type == UnitType.Enemy &&
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
}