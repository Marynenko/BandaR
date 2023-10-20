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
        _units = GridUI.Instance.TurnManager.PlayersGet;
        ActivePlayer = _units.Peek(); // Назначаем первого игрока активным
        ActivePlayer.Status = UnitStatus.Available;
        GridUI.Instance.TurnManager.ShowPortrait(ActivePlayer, true);
        GridUI.Instance.ClearColorTiles(Grid.Tiles);
        UIManager.Instance.CameraManager.IsActive = true;
        UIManager.Instance.AttackManager.AttackIndicators.Launch(ActivePlayer.Stats.Energy,
            ActivePlayer.Stats.StateFatigue);
        UIManager.Instance.AttackManager.Attacks.AttacksPrefab = ActivePlayer.AttacksPrefab;
        
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
        void LocalMoveOn()
        {
            MoveOn();
            FinishMove();
            InputPlayer.ClickedUnit = null;
            InputPlayer.IsTileClickable = true;
            InputPlayer.IsUnitClickable = true;
        }

        if (unit.Stats.Type != UnitType.Player)
        {
            if (_attackIsFinished)
            {
                LocalMoveOn();
                _attackIsFinished = false;
                return true;
            }

            if (!_isCoroutineOn)
            {
                StartCoroutine(AIAttack(unit));
            }

            if (_isCoroutineOn)
                return false;
        }
        else
        {
            LocalMoveOn();
            return true;
        }

        return true;
    }

    private IEnumerator AIAttack(Unit unit)
    {
        _isCoroutineOn = true;
        UIManager.Instance.AttackManager.HandleAttackButtonClicked(ActivePlayer);
        yield return new WaitForSeconds(1.5f);
        _isCoroutineOn = false;
        var enemies = GetEnemyFromNeighbours(unit.Stats.Type);
        var enemy = LocateBestEnemyToHit(enemies);
        UIManager.Instance.AttackManager.LaunchAttack(ActivePlayer, enemy);
        _attackIsFinished = true;
    }

    private Unit LocateBestEnemyToHit(List<Unit> enemies)
    {
        var playerWithLeastHp = enemies.OrderBy(player => player.Stats.Health).FirstOrDefault();
        return playerWithLeastHp;
    }

    private List<Unit> GetEnemyFromNeighbours(UnitType type)
    {
        var neighbours = Selector.PathConstructor.GetNeighbours(ActivePlayer.OccupiedTile);

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
        if (ActivePlayer.Target == null && ActivePlayer.Stats.Type != UnitType.Enemy)
            ActivePlayer.Target = ActivePlayer.OccupiedTile;
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
            GridUI.Instance.TurnManager.SetCurrentPlayer(ref ActivePlayer);
            // GridUI.Instance.TurnManager.EndTurn(ref activePlayer);    
        }
    }
}