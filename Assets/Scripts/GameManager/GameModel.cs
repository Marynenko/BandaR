using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameModel : MonoBehaviour
{
    [SerializeField] private Grid Grid;
    [SerializeField] private InputPlayer InputPlayer;
    [SerializeField] private Selector Selector;
    [HideInInspector] public Unit ActivePlayer;

    private const float HeightToPutUnitOnTile = 0.68f;
    private bool _isCoroutineOn;
    public bool _attackIsFinished;

    private Unit _enemy;

    private Queue<Unit> _units = new();

    private void Start()
    {
        Grid.StartCreating();
        StartGame();
    }

    private void StartGame()
    {
        var ui = UIManager.Instance;
        _units = ui.TurnManager.PlayersGet;
        ActivePlayer = _units.Peek(); // Назначаем первого игрока активным
        ActivePlayer.Status = UnitStatus.Available;
        ui.TurnManager.HighlightPortrait(ActivePlayer, true);
        ui.GridUI.ClearColorTiles(Grid.Tiles);
        ui.CameraManager.IsActive = true;
        ui.AttackManager.MovementIndicators.Launch(ActivePlayer.Stats.Energy,
            ActivePlayer.Stats.StateFatigue);
        ui.AttackManager.Attacks.InitializeAttacks(ActivePlayer.AttacksPrefab);
    }

    #region Finish Move

    public bool HandleEndTurnButtonClicked(Unit unit)
    {
        ActivePlayer = unit;
        HandlePlayerNullTarget();

        if (MatchPositionsPlayerAndDestination())
        {
            if (ActivePlayer.Stats.Type == UnitType.Player)
            {
                FinishMove();
                return true;
            }

            var isAttackSuccessful = IsAttackSuccessful(ActivePlayer);

            if (isAttackSuccessful)
                return true;
        }

        return false;
    }

    private bool IsAttackSuccessful(Unit unit)
    {
        if (unit.Stats.Type == UnitType.Player)
        {
            FinishMove();
            return true;
        }

        var enemies = GetEnemyFromNeighbours(unit);
        if (enemies.Count == 0)
        {
            FinishMove();
            return true;
        }

        _enemy = LocateBestEnemyToHit(enemies);

        if (_attackIsFinished)
        {
            FinishMove();
            _attackIsFinished = false;
            return true;
        }

        if (!_attackIsFinished)
        {
            ActivateAttackButton();
            Invoke(nameof(LaunchAttack), 1.5f);
        }

        return false;
    }

    private void ActivateAttackButton()
    {
        UIManager.Instance.AttackManager.HandleAttackButtonClicked(ActivePlayer);
    }

    private void LaunchAttack()
    {
        if (ActivePlayer.Stats.CountAttacks == 0) return;
        
        _attackIsFinished = true;
        UIManager.Instance.AttackManager.LaunchAttack(ActivePlayer, _enemy);
    }

    private void FinishMove()
    {
        PassTurn();
        ResetInputPlayer();
    }

    private void PassTurn()
    {
        UIManager.Instance.MenuAction.HideMenu();
        ActivePlayer.Stats.MovementPoints = 0;
        Selector.UnselectUnit(ActivePlayer);
        ActivePlayer.OccupiedTile.Available = false;


        ActivePlayer.OccupiedTile.State = ActivePlayer.Stats.Type switch
        {
            UnitType.Player => TileState.OccupiedByPlayer,
            UnitType.Ally => TileState.OccupiedByAlly,
            UnitType.Enemy => TileState.OccupiedByEnemy,
            _ => ActivePlayer.OccupiedTile.State
        };

        ActivePlayer.Status = UnitStatus.Moved;

        // Проверяем, был ли игрок перемещен в этом ходе
        if (ActivePlayer.Status == UnitStatus.Moved)
        {
            // Передаем ход следующему игроку
            UIManager.Instance.GridUI.HighlightTiles(ActivePlayer.OccupiedTile.Neighbors, TileState.Standard);
            UIManager.Instance.TurnManager.SetCurrentPlayer(ref ActivePlayer);
        }

        if (ActivePlayer.Stats.Type != UnitType.Player)
            UIManager.Instance.TurnManager.AI.InitializeAI(ActivePlayer);
    }

    private void ResetInputPlayer()
    {
        InputPlayer.ClickedUnit = null;
        InputPlayer.IsTileClickable = true;
        InputPlayer.IsUnitClickable = true;
    }

    #endregion

    private Unit LocateBestEnemyToHit(List<Unit> enemies)
    {
        var playerWithLeastHp = enemies.OrderBy(player => player.Stats.Health).FirstOrDefault();
        return playerWithLeastHp;
    }

    public List<Unit> GetEnemyFromNeighbours(Unit unit)
    {
        var neighbours = Selector.PathConstructor.GetNeighbours(unit.OccupiedTile);
        var type = unit.Stats.Type;
        if (type == UnitType.Enemy)
            return (from neighbour in neighbours
                where !neighbour.Available
                where neighbour.State is TileState.OccupiedByPlayer or TileState.OccupiedByAlly
                select GetUnitFromNeighbour(neighbour)).ToList();
        return (from neighbour in neighbours
            where !neighbour.Available
            where neighbour.State is TileState.OccupiedByEnemy
            select GetUnitFromNeighbour(neighbour)).ToList();
    }

    private Unit GetUnitFromNeighbour(Tile neighbour)
    {
        return Grid.AllUnits.FirstOrDefault(unit => unit.OccupiedTile == neighbour);
    }

    private void HandlePlayerNullTarget()
    {
        if (ActivePlayer.Target == null)
            ActivePlayer.Target = ActivePlayer.OccupiedTile;
        // if (ActivePlayer.Target == null && ActivePlayer.Stats.Type != UnitType.Enemy)
        //     ActivePlayer.Target = ActivePlayer.OccupiedTile;
    }

    private bool MatchPositionsPlayerAndDestination() =>
        ActivePlayer.transform.position ==
        ActivePlayer.Target.transform.position + Vector3.up * HeightToPutUnitOnTile;
}