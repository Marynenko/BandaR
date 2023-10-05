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
        if (activePlayer.Target == null && activePlayer.Type != UnitType.Enemy)
            activePlayer.Target = activePlayer.OccupiedTile;
        if (activePlayer.Type == UnitType.Player && activePlayer.Target != null)
        {
            if (activePlayer.transform.position ==
                activePlayer.Target.transform.position + Vector3.up * HEIGHT_TO_PUT_UNIT_ON_TILE)
            {
                MoveOn();
                FinishMove();
                return true;
            }
        }
        else if (activePlayer.Type == UnitType.Enemy && activePlayer.Target != null)
        {
            if (activePlayer.transform.position ==
                activePlayer.Target.transform.position + Vector3.up * HEIGHT_TO_PUT_UNIT_ON_TILE)
            {
                MoveOn();
                FinishMove();
                return true;
            }
        }
        // if (activePlayer.OccupiedTile == GridUI.Instance.TurnManager.AI.Target)
            

        return false;
    }

    private void MoveOn()
    {
        UIManager.Instance.MenuAction.HideMenu();
        activePlayer.Stats.MovementPoints = 0;
        selector.UnselectUnit(activePlayer);
        activePlayer.OccupiedTile.Available = false;
        activePlayer.OccupiedTile.State = activePlayer.Type == UnitType.Player
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