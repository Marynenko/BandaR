using System.Collections.Generic;
using UnityEngine;

public class GameModel : MonoBehaviour
{
    [SerializeField] private Grid Grid;
    [SerializeField] private InputPlayer InputPlayer;
    [SerializeField] private Selector Selector;
    [HideInInspector] public Unit ActivePlayer;

    private const float HeightToPutUnitOnTile = 0.68f;

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
        GridUI.Instance.TurnManager.HighlightPlayer(ActivePlayer, true);
        Grid.ClearColorTiles();
    }

    public bool HandleEndTurnButtonClicked(Unit unit)
    {
        bool GoOn()
        {
            MoveOn();
            FinishMove();
            InputPlayer.ClickedUnit = null;
            InputPlayer.IsTileClickable = true;
            InputPlayer.IsUnitClickable = true;
            return true;
        }

        ActivePlayer = unit;
        HandlePlayerNullTarget();

        return ActivePlayer.Stats.Type switch
        {
            UnitType.Player when ActivePlayer.Target != null && MatchPositionsPlayerAndDestination() => GoOn(),
            UnitType.Enemy when ActivePlayer.Target != null && MatchPositionsPlayerAndDestination() => GoOn(),
            _ => false
        };
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
        ActivePlayer.OccupiedTile.State = ActivePlayer.Stats.Type == UnitType.Player
            ? TileState.OccupiedByPlayer
            : TileState.OccupiedByEnemy;

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