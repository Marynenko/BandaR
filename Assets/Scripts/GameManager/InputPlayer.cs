using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class InputPlayer : MonoBehaviour
{
    public GameController GameController;
    public GameModel GameModel;

    public void HandleLeftClick(Vector3 mousePosition)
    {
        var ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out var hit) && !UIManager.Instance.MenuAction.isActiveAndEnabled)
        {
            if (hit.collider.TryGetComponent(out Unit unit))
            {
                UIManager.Instance.MenuAction.ShowMenu(unit);
            }
            else if (hit.collider.TryGetComponent(out Tile tile))
                GameController.HandleTileClick(tile);
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