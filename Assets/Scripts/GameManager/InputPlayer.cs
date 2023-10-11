using UnityEngine;
using UnityEngine.EventSystems;

public class InputPlayer : MonoBehaviour
{
    public GameController GameController;
    public GameModel GameModel;

    public Unit ClickedUnit;
    private Tile _clickedTile;
    private Camera _camera;

    public bool IsMenuActive = false;
    public bool IsInfoClickable = false;
    public bool IsAttackClickable = false;
    public bool IsMovementClickable = false;
    public bool IsUnitClickable = true;
    public bool IsEnemyClickable = false;
    public bool IsAllyClickable = false;
    public bool IsTileClickable = false;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Debug.Log("Input Player Update ESCAPE");
            if (IsMenuActive)
            {
                UIManager.Instance.MenuAction.HideMenu();
            }

            IsTileClickable = true;
            IsUnitClickable = true;
            ClickedUnit = null;

        }
        // Call 2
        if (ClickedUnit == null) return;
        if (_clickedTile != null && ClickedUnit.UnitIsMoving)
        {
            GameController.HandleTileClick(_clickedTile);
        }
    }

    private bool IsMouseOverUI() => EventSystem.current.IsPointerOverGameObject();
    
    public void HandleLeftClick(Vector3 mousePosition)
    {
        if (_camera == null) return;
        
        var ray = _camera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            
            if (hit.collider.TryGetComponent(out Unit unit))
            {
                if (!IsUnitClickable) return;
                if (ClickedUnit != null)
                    if (unit != ClickedUnit) return;
                if (GetCurrentMovingUnit() != unit)
                {
                    UIManager.Instance.MenuAction.ShowMenu(unit, false);
                    return;
                }
                ClickedUnit = unit;
                IsUnitClickable = false;
                UIManager.Instance.MenuAction.ShowMenu(unit, true);
            }
            else if (hit.collider.TryGetComponent(out Tile tile) && ClickedUnit != null)
            {
                if (!IsTileClickable) return;
                _clickedTile = tile;
                CompareAvailableMovesToTile();
                IsTileClickable = false;
            }
        }

        if (ClickedUnit != null)
        {
            if (ClickedUnit.Stats.MovementPoints > 1)
            {
                IsUnitClickable = true;
            }
        }
    }

    private Unit GetCurrentMovingUnit()
    {
        var players = GridUI.Instance.TurnManager.PlayersGet;
        foreach (var player in players)
        {
            if (player.Status == UnitStatus.Available)
                return player;
        }

        return null;
    }


    private void CompareAvailableMovesToTile()
    {
        if (ClickedUnit == null) return;
        if (!ClickedUnit.AvailableMoves.Contains(_clickedTile))
            return;
        ClickedUnit.UnitIsMoving = true;
    }
}