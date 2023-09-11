using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AI : MonoBehaviour
{
    [SerializeField] private Button _endTurnButton;
    private GameController _gameController;
    private GameModel _gameModel;

    private void OnEnable()
    {
        _gameController = gameObject.GetComponentInParent<GameController>();
        _gameModel = gameObject.GetComponentInParent<GameModel>();
    }

    public void UpdateUi<T>(T activePlayer, Button button) where T : Unit
    {
        var units = _gameController.Grid.Generator.AllUnits;
        var currentPlayer = activePlayer;

        if (currentPlayer == null)
        {
            // ���� ActivePlayer == null, �� ������ ������ �����������
            button.interactable = false;
            return;
        }

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

        button.interactable = canEndTurn;

        // ����������: ���������, ������ �� ���� � ������ �� ��� ���
        if (currentPlayer.Status != UnitStatus.Moved)
        {
            canEndTurn = false;
        }
    }


    public void Move(Unit unit)
    {
        //_gameModel.ActivePlayer = unit;
        var ameController = _gameController;
        var grid = _gameController.Grid;
        var interactor = _gameController.Interactor;
        var localSelector = _gameController.Selector;

        var availableMoves = localSelector.GetAvailableMoves(unit.OccupiedTile, unit.MovementPoints);
        if (availableMoves.Count == 0)
        {
            unit.Status = UnitStatus.Moved;
            return;
        }

        // ������� ���� ���������� ������
        var enemies = grid.Generator.AllUnits.Where(u => u.Type != unit.Type).ToArray();

        // �������� ���������� �����
        var targetEnemy = enemies.OrderBy(e => interactor.PathConstructor.GetDistance(unit.OccupiedTile, e.OccupiedTile)).FirstOrDefault();

        // ���� ����� �����, �������� � ����
        if (targetEnemy != null)
        {
            var targetTile = targetEnemy.OccupiedTile;
            interactor.PathConstructor.FindPathToTarget(unit.OccupiedTile, targetTile, out List<Tile> path, _gameController.Grid);
            _gameController.MoveUnitAlongPath(unit, path);
        }
        else
        {
            // ���� ������ ���, �������� � ��������� ��������� ������
            var randomIndex = Random.Range(0, availableMoves.Count);
            var targetTile = availableMoves[randomIndex];
            interactor.PathConstructor.FindPathToTarget(unit.OccupiedTile, targetTile, out List<Tile> path, _gameController.Grid);
            _gameController.MoveUnitAlongPath(unit, path);
        }

        // ��������� ��������� �����
        interactor.UpdateUnit(unit);
        localSelector.UnselectUnit(unit); // ����� ������ ��������

        // ��������� ����������� ����� ����� �����������
        unit.OccupiedTile.DeselectTile();
        unit.Status = UnitStatus.Moved;
        unit.Stats.MovementPoints = unit.Stats.MovementRange; // �������, ������ ��������

        _gameModel.EndTurn();
    }
}
