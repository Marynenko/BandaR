﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputPlayer : MonoBehaviour
{
    public GameController GameController;
    public GameModel GameModel;

    public Unit ClickedUnit;
    private Tile _clickedTile;
    private Tile _startTile;
    private Camera _camera;

    public bool IsMenuActive = false;
    public bool IsAttackActive = false;
    public bool IsUnitClickable = true;
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
                UIManager.Instance.MenuAction.HideMenu();
            if (ClickedUnit != null)
            {
                if (ClickedUnit.AvailableMoves != null)
                    GridUI.Instance.HighlightTiles(ClickedUnit.AvailableMoves, TileState.Standard);
                if (ClickedUnit.OccupiedTile.Neighbors != null)
                    GridUI.Instance.HighlightTiles(ClickedUnit.OccupiedTile.Neighbors, TileState.Standard);
            }

            IsTileClickable = true;
            IsUnitClickable = true;
            IsAttackActive = false;
            _startTile = null;
            ClickedUnit = null;
        }

        // Call 2
        if (ClickedUnit == null) return;
        if (_clickedTile != null && ClickedUnit.UnitIsMoving)
            GameController.HandleTileClick(_clickedTile);
    }

    public void HandleLeftClick(Vector3 mousePosition)
    {
        if (_camera == null) return;

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
                if (!IsUnitClickable) return;
                if (ClickedUnit != null)
                    if (unit != ClickedUnit)
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
                if (!IsTileClickable) return;
                _clickedTile = tile;
                CompareAvailableMovesToTile();
                IsTileClickable = false;
            }
        }

        if (ClickedUnit != null)
            if (ClickedUnit.Stats.MovementPoints > 1)
                IsUnitClickable = true;
    }

    

    private Unit GetCurrentMovingUnit()
    {
        var players = GridUI.Instance.TurnManager.PlayersGet;
        return players.FirstOrDefault(player => player.Status == UnitStatus.Available);
    }


    private void CompareAvailableMovesToTile()
    {
        if (ClickedUnit == null) return;
        if (!ClickedUnit.AvailableMoves.Contains(_clickedTile))
            return;
        ClickedUnit.UnitIsMoving = true;
    }
}