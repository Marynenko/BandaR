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
    private bool _attackIsFinished;

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
        ui.TurnManager.ShowPortrait(ActivePlayer, true);
        ui.GridUI.ClearColorTiles(Grid.Tiles);
        ui.CameraManager.IsActive = true;
        ui.AttackManager.AttackIndicators.Launch(ActivePlayer.Stats.Energy,
            ActivePlayer.Stats.StateFatigue);
        ui.AttackManager.Attacks.InitializeAttacks(ActivePlayer.AttacksPrefab);
    }

    public bool HandleEndTurnButtonClicked(Unit unit)
    {
        ActivePlayer = unit;
        HandlePlayerNullTarget();

        return ActivePlayer.Stats.Type switch
        {
            UnitType.Player when ActivePlayer.Target != null && MatchPositionsPlayerAndDestination() => GoOn(unit),
            UnitType.Ally when ActivePlayer.Target != null && MatchPositionsPlayerAndDestination() => GoOn(unit),
            UnitType.Enemy when ActivePlayer.Target != null && MatchPositionsPlayerAndDestination() => GoOn(unit),
            _ => false
        };
    }

    private bool GoOn(Unit unit)
    {
        if (unit.Stats.Type != UnitType.Player)
        {
            var enemies = GetEnemyFromNeighbours(unit);
            if (enemies.Count == 0)
            {
                ResetPlayerState();
                return true;
            }

            var enemy = LocateBestEnemyToHit(enemies);

            if (_attackIsFinished)
            {
                ResetPlayerState();
                _attackIsFinished = false;
                return true;
            }

            if (!_isCoroutineOn)
            {
                StartCoroutine(AIAttack(enemy));
            }

            return false;
        }
        
        ResetPlayerState();
        return true;
    }

    private void ResetPlayerState()
    {
        MoveOn();
        FinishMove();
        ResetInputPlayer();
    }

    private void ResetInputPlayer()
    {
        InputPlayer.ClickedUnit = null;
        InputPlayer.IsTileClickable = true;
        InputPlayer.IsUnitClickable = true;
    }

    private IEnumerator AIAttack(Unit unit)
    {
        _isCoroutineOn = true;
        UIManager.Instance.AttackManager.HandleAttackButtonClicked(ActivePlayer);
        yield return new WaitForSeconds(1.5f);
        _isCoroutineOn = false;

        UIManager.Instance.AttackManager.LaunchAttack(ActivePlayer, unit);
        _attackIsFinished = true;
    }

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

    private void MoveOn()
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
    }

    private void FinishMove()
    {
        // Проверяем, был ли игрок перемещен в этом ходе
        if (ActivePlayer.Status == UnitStatus.Moved)
        {
            // Передаем ход следующему игроку
            UIManager.Instance.GridUI.HighlightTiles(ActivePlayer.OccupiedTile.Neighbors, TileState.Standard);
            UIManager.Instance.TurnManager.SetCurrentPlayer(ref ActivePlayer);
        }
    }
}