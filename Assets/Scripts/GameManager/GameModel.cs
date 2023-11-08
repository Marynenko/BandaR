using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class GameModel : MonoBehaviour
{
    [SerializeField] private Grid Grid;
    [SerializeField] private InputPlayer InputPlayer;
    [SerializeField] private Selector Selector;

    [HideInInspector] public Unit ActivePlayer;
    public bool IsAttackFinished;
    public bool IsAttackStarted;


    private const float HeightToPutUnitOnTile = 0.68f;
    private bool _isCoroutineOn;


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
        _units = ui.TurnManager.Players;
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

        if (ActivePlayer.Stats.Type == UnitType.Player)
        {
            FinishMove();
            return true;
        }

        HandlePlayerNullTarget();


        if (MatchPositionsPlayerAndDestination(ActivePlayer))
        {
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

        switch (IsAttackFinished)
        {
            case true:
                FinishMove();
                IsAttackStarted = false;
                return true;
            case false when unit.Stats.CountAttacks != 0:
                IsAttackStarted = true;
                ActivateAttackButton();
                Invoke(nameof(LaunchAttack), 1.5f);
                break;
        }

        return false;
    }

    private void ActivateAttackButton()
    {
        UIManager.Instance.AttackManager.HandleAttackButtonClicked(ActivePlayer);
    }

    private void LaunchAttack()
    {
        if (IsAttackFinished || !IsAttackStarted) return;
        IsAttackFinished = true;
        UIManager.Instance.AttackManager.LaunchAttack(ActivePlayer, _enemy);
        IsAttackStarted = false;
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
        List<Unit> enemyUnits = new List<Unit>();

        foreach (var neighbour in neighbours)
        {
            if (!neighbour.Available)
            {
                if (type == UnitType.Enemy &&
                    neighbour.State is TileState.OccupiedByPlayer or TileState.OccupiedByAlly)
                {
                    enemyUnits.Add(GetUnitFromNeighbour(neighbour));
                }
                else if (type != UnitType.Enemy && neighbour.State == TileState.OccupiedByEnemy)
                {
                    enemyUnits.Add(GetUnitFromNeighbour(neighbour));
                }
            }
        }

        return enemyUnits;
    }


    private Unit GetUnitFromNeighbour(Tile neighbour)
    {
        return Grid.AllUnits.FirstOrDefault(unit => unit.OccupiedTile == neighbour);
    }

    private void HandlePlayerNullTarget()
    {
        if (ActivePlayer.Target == null)
            ActivePlayer.Target = ActivePlayer.OccupiedTile;
    }

    public bool MatchPositionsPlayerAndDestination(Unit unit)
    {
        // ActivePlayer = unit;
        // HandlePlayerNullTarget();
        return unit.transform.position ==
               unit.Target.transform.position + Vector3.up * HeightToPutUnitOnTile;
    }
}