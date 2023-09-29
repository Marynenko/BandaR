using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class InputPlayer : MonoBehaviour
{
    public GameController GameController;
    public GameModel GameModel;
    
    private Unit _clickedUnit;
    private Tile _clickedTile;

    public void HandleLeftClick(Vector3 mousePosition)
    {
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out var hit) && !UIManager.Instance.MenuAction.isActiveAndEnabled)
        {
            if (hit.collider.TryGetComponent(out Unit unit))
            {
                _clickedUnit = unit;
                UIManager.Instance.MenuAction.ShowMenu(unit);
            }
            else if (hit.collider.TryGetComponent(out Tile tile) && _clickedUnit != null)
            {
                _clickedTile = tile;
                _clickedUnit.UnitIsMoving = true;
            }
        }
    }
    
    private void Update()
    {
        if (_clickedTile != null && _clickedUnit.UnitIsMoving)
        {
            GameController.HandleTileClick(_clickedTile);
        }
    }

    public void HandleEndTurnButtonClicked()
    {
        UIManager.Instance.MenuAction.HideMenu();
        var Status = GameController.Selector.SelectedUnit.Status;
        GameController.Selector.UnselectUnit(GameController.Selector.SelectedUnit);

        if (GameController.Selector.SelectedUnit != null)
        {
            Debug.Log("You must unselect the current unit before ending the turn.");
            return;
        }

        // Проверяем, был ли игрок перемещен в этом ходе
        if (Status == UnitStatus.Moved)
        {
            // Передаем ход следующему игроку
            GameModel.EndTurn();    
        }
    }
}