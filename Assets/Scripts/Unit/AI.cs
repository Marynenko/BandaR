using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AI : MonoBehaviour
{
    private GameController _gameController;
    private GameModel _gameModel;
    private Grid _grid;
    
    private void OnEnable()
    {
        _gameController = gameObject.GetComponentInParent<GameController>();
        _gameModel = gameObject.GetComponentInParent<GameModel>();
        _grid = _gameController.Grid;
    }

    public void StartMove(Unit unit)
    {
        StartCoroutine(Move(unit));
    }

    public void UpdateUi(Unit activePlayer)
    {
        var units = _grid.AllUnits;
        var currentPlayer = activePlayer;

        // if (currentPlayer == null)
        // {
        //     // ���� ActivePlayer == null, �� ������ ������ �����������
        //     // button.interactable = false;
        //     return;
        // }

        var canEndTurn = currentPlayer.Status == UnitStatus.Moved;

        if (!canEndTurn)
        {
            var movedUnits = units.Where
                (u => u.Status == UnitStatus.Moved && u == currentPlayer).
                ToList();

            if (movedUnits.Count == 0)
            {
                canEndTurn = true;
            }
        }

        // button.interactable = canEndTurn;
    }


    public void Move2(Unit unit)
    {
        var availableMoves = _gameController.Selector.PathConstructor.GetAvailableMoves(unit.OccupiedTile, unit.MovementPoints);
        if (availableMoves.Count == 0)
        {
            unit.Status = UnitStatus.Moved;
            return;
        }

        // ������� ���� ���������� ������
        var enemies = _grid.AllUnits.Where(u => u.Type != unit.Type).ToArray();

        // �������� ���������� �����
        var targetEnemy = enemies.OrderBy(e => 
            _gameController.Selector.PathConstructor.GetDistance(unit.OccupiedTile, e.OccupiedTile)).FirstOrDefault();

        // ���� ����� �����, �������� � ����
        if (targetEnemy != null)
        {
            var targetTile = targetEnemy.OccupiedTile;
            _gameController.Selector.PathConstructor.FindPathToTarget(unit.OccupiedTile, targetTile, out List<Tile> path);
            _gameController.MoveUnitAlongPath(unit, path);
        }
        else
        {
            // ���� ������ ���, �������� � ��������� ��������� ������
            var randomIndex = Random.Range(0, availableMoves.Count);
            var targetTile = availableMoves[randomIndex];
            _gameController.Selector.PathConstructor.FindPathToTarget(unit.OccupiedTile, targetTile, out var path);
            _gameController.MoveUnitAlongPath(unit, path);
        }

        // ��������� ��������� �����
        // _grid.Selector.UpdateUnit(unit); // TODO ����������� �� ������
        _grid.Selector.UnselectUnit(unit); // ����� ������ ��������

        // ��������� ����������� ����� ����� �����������
        // unit.OccupiedTile.UnselectTile();
        // unit.Status = UnitStatus.Moved;
        unit.Stats.MovementPoints = unit.Stats.MovementRange; // �������, ������ ��������

        _gameModel.EndTurn();
    }

    public IEnumerator Move(Unit unit)
    { 
        // ����� �����
        _gameController.Selector.SelectUnit(unit);
        yield return new WaitForSeconds(1.5f);
        
        // ������� ���� ���������� ������
        var enemies = _grid.AllUnits.Where(u => u.Type != unit.Type).ToArray();
        
        // �������� ���������� �����
        var targetEnemy = enemies.OrderBy(e => 
            _gameController.Selector.PathConstructor.GetDistance(unit.OccupiedTile, e.OccupiedTile)).FirstOrDefault();

        // ���� ����� �����, �������� � ����
        if (targetEnemy != null)
        {
            var targetTile = targetEnemy.OccupiedTile;
            _gameController.Selector.PathConstructor.FindPathToTarget(unit.OccupiedTile, targetTile, out List<Tile> path);
            _gameController.MoveUnitAlongPath(unit, path);
        }
        else
        {
            _gameModel.EndTurn();
        }

        _gameController.Selector.UnselectUnit(unit);
        _gameModel.EndTurn();
    }
}
