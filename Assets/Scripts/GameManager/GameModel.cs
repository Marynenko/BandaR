using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameModel : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private InputPlayer input;
    [SerializeField] private Selector selector;
    [HideInInspector] public Unit activePlayer;

    private const float HEIGHT_TO_PUT_UNIT_ON_TILE = 0.68f;

    private Queue<Unit> _units = new();

    private void Start()
    {
        grid.StartCreating();
        StartGame();
    }

    private void Update()
    {
        if (activePlayer == null)
            return;
        if (activePlayer.UnitIsMoving || activePlayer.Status == UnitStatus.AIMove)
            return;
        if (Input.GetMouseButtonDown(0))
        {
            var mousePosition = Input.mousePosition;
            input.HandleLeftClick(mousePosition);
        }
    }

    private void StartGame()
    {
        _units = GridUI.Instance.TurnManager.Players;
        activePlayer = _units.Peek(); // Назначаем первого игрока активным
        activePlayer.Status = UnitStatus.Available;
        GridUI.Instance.TurnManager.HighlightPlayer(activePlayer, true);
        grid.SetAvailableTiles();
    }

    public bool HandleEndTurnButtonClicked(Unit unit)
    {
        activePlayer = unit;
        HandlePlayerNullTarget();

        switch (activePlayer.Stats.Type)
        {
            case UnitType.Player when activePlayer.Target != null && MatchPositionsPlayerAndDestination():
                MoveOn();
                FinishMove();
                return true;
            case UnitType.Enemy when activePlayer.Target != null && MatchPositionsPlayerAndDestination():
                MoveOn();
                FinishMove();
                return true;
            default:
                return false;
        }
    }

    private void HandlePlayerNullTarget()
    {
        if (activePlayer.Target == null && activePlayer.Stats.Type != UnitType.Enemy)
            activePlayer.Target = activePlayer.OccupiedTile;
    }

    private bool MatchPositionsPlayerAndDestination( ) =>
        activePlayer.transform.position ==
        activePlayer.Target.transform.position + Vector3.up * HEIGHT_TO_PUT_UNIT_ON_TILE;

    private void MoveOn()
    {
        UIManager.Instance.MenuAction.HideMenu();
        activePlayer.Stats.MovementPoints = 0;
        selector.UnselectUnit(activePlayer);
        activePlayer.OccupiedTile.Available = false;
        activePlayer.OccupiedTile.State = activePlayer.Stats.Type == UnitType.Player
            ? TileState.OccupiedByPlayer
            : TileState.OccupiedByEnemy;

        activePlayer.Status = UnitStatus.Moved;
    }

    private void FinishMove()
    {
        // Проверяем, был ли игрок перемещен в этом ходе
        if (activePlayer.Status == UnitStatus.Moved)
        {
            // Передаем ход следующему игроку
            GridUI.Instance.TurnManager.SetCurrentPlayer(ref activePlayer);
            // GridUI.Instance.TurnManager.EndTurn(ref activePlayer);    
        }
    }
}