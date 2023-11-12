using System.Linq;
using UnityEngine;

public class InputPlayer : MonoBehaviour
{
    public GameController GameController;
    public GameModel GameModel;

    public Unit ClickedUnit;

    public bool IsMenuActive;
    public bool IsAttackActive;
    public bool IsUnitClickable = true;
    public bool IsTileClickable;

    private Tile _clickedTile;
    private Tile _startTile;
    private Camera _camera;


    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (EscHandler()) return;
        }

        // Call 2
        if (ClickedUnit == null) return;
        if (_clickedTile != null && ClickedUnit.UnitIsMoving)
            GameController.HandlePlayerTileClick(_clickedTile);
    }

    public bool EscHandler()
    {
        var ui = UIManager.Instance.AttackManager;

        // Debug.Log("Input Player Update ESCAPE");
        if (IsMenuActive)
            UIManager.Instance.MenuAction.HideMenu();

        AttackMenuChecker(ui);
        if (ui.AttackMenu.isActiveAndEnabled) return true;

        if (ClickedUnit != null)
        {
            if (ClickedUnit.AvailableMoves != null)
                UIManager.Instance.GridUI.HighlightTiles(ClickedUnit.AvailableMoves, TileState.Standard);
            if (ClickedUnit.OccupiedTile.Neighbors != null)
                UIManager.Instance.GridUI.HighlightTiles(ClickedUnit.OccupiedTile.Neighbors, TileState.Standard);
        }

        IsTileClickable = true;
        IsUnitClickable = true;
        IsAttackActive = false;
        _startTile = null;
        ClickedUnit = null;
        return false;
    }

    private void AttackMenuChecker(AttackManager ui)
    {
        if (IsAttackActive)
        {
            if (!ui.Attacks.PanelAskSure.activeSelf)
            {
                ui.MovementIndicators.Energy = ui.MovementIndicators.EnergyTransparent;
                ui.MovementIndicators.Ustalost = ui.MovementIndicators.UstalostTransparent;
                ui.MovementIndicators.ModifyEnergy(0);
                ui.MovementIndicators.ModifyUstalost(0);
                ui.AttackMenu.gameObject.SetActive(false);
            }

            if (ui.Attacks.PanelAskSure.gameObject.activeSelf)
                ui.Attacks.PanelAskSure.SetActive(false);
        }
    }

    public void HandleLeftClick(Vector3 mousePosition)
    {
        if (_camera == null) 
            return;

        var ray = _camera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            if (hit.collider.TryGetComponent(out Unit unit))
            {
                if (IsAttackActive)
                {
                    UIManager.Instance.CameraManager.IsActive = false;
                    UIManager.Instance.AttackManager.LaunchAttack(ClickedUnit, unit);
                    return;
                }

                if (!IsUnitClickable) 
                    return;
                
                if (ClickedUnit != null && unit != ClickedUnit) 
                    return;
                
                if (GetCurrentMovingUnit() == null) 
                    return;
                
                if (GetCurrentMovingUnit() != unit)
                {
                    UIManager.Instance.MenuAction.ShowMenu(unit, false);
                    return;
                }

                ClickedUnit = unit;
                IsUnitClickable = false;
                _startTile = ClickedUnit.OccupiedTile;
                UIManager.Instance.MenuAction.ShowMenu(unit, true);
            }
            else if (hit.collider.TryGetComponent(out Tile tile) && ClickedUnit != null)
            {
                if (!IsTileClickable)
                    return;
                
                if (!TileIsNotAvailable(tile)) 
                    return;
                    
                IsTileClickable = false;
            }
        }

        if (ClickedUnit != null && ClickedUnit.Stats.MovementPoints > 1)
            IsUnitClickable = true;
    }

    private Unit GetCurrentMovingUnit()
    {
        var players = UIManager.Instance.TurnManager.Players;
        return players.FirstOrDefault(player => player.Status == UnitStatus.Available);
    }

    private bool TileIsNotAvailable(Tile clickedTile)
    {
        if (ClickedUnit == null) 
            return false;
        
        if (!ClickedUnit.AvailableMoves.Contains(clickedTile))
            return false;
        
        _clickedTile = clickedTile;
        ClickedUnit.UnitIsMoving = true;
        return true;
    }
}