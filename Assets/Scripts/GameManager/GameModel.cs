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

    // private void Update()
    // {
    //     // Call 1
    //     if (ActivePlayer == null) return;
    //     if (ActivePlayer.UnitIsMoving || ActivePlayer.Status == UnitStatus.AIMove)
    //         return;
    //     // if (UnitMenu.Instance.MenuAction.isActiveAndEnabled)
    //     //     return;
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         var mousePosition = Input.mousePosition;
    //         InputPlayer.HandleLeftClick(mousePosition);
    //     }
    // }

    private void StartGame()
    {
        _units = GridUI.Instance.TurnManager.PlayersGet;
        ActivePlayer = _units.Peek(); // Назначаем первого игрока активным
        ActivePlayer.Status = UnitStatus.Available;
        GridUI.Instance.TurnManager.HighlightPlayer(ActivePlayer, true);
        Grid.SetAvailableTiles();
    }

    public bool HandleEndTurnButtonClicked(Unit unit)
    {
        ActivePlayer = unit;
        HandlePlayerNullTarget();

        switch (ActivePlayer.Stats.Type)
        {
            case UnitType.Player when ActivePlayer.Target != null && MatchPositionsPlayerAndDestination():
                MoveOn();
                FinishMove();
                InputPlayer.ClickedUnit = null;
                return true;
            case UnitType.Enemy when ActivePlayer.Target != null && MatchPositionsPlayerAndDestination():
                MoveOn();
                FinishMove();
                InputPlayer.ClickedUnit = null;
                return true;
            default:
                return false;
        }
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