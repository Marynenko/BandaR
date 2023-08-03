using System.Collections;
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

    public void UpdateUI<T>(T _activePlayer, Button button) where T : Unit
    {
        var currentPlayer = _activePlayer;

        if (currentPlayer == null)
        {
            // ���� ActivePlayer == null, �� ������ ������ �����������
            button.interactable = false;
            return;
        }

        var canEndTurn = currentPlayer.Status == UnitStatus.Moved;

        if (!canEndTurn)
        {
            var movedUnits = _gameController.Grid.AllUnits.Where(u => u.Status == UnitStatus.Moved && u == currentPlayer).ToList();
            if (movedUnits.Count == 0)
            {
                canEndTurn = true;
            }
        }

        button.interactable = canEndTurn;

        // ����������: ���������, ������ �� ���� � ������ �� ��� ���
        var isUnitSelected = currentPlayer != null;
        if (isUnitSelected && !(currentPlayer.Status == UnitStatus.Moved))
        {
            canEndTurn = false;
        }
    }


    public void Move(Unit unit)
    {
        //_gameModel.ActivePlayer = unit;
        var localController = _gameController;
        var localGrid = localController.Grid;
        var localInteractor = localController.Interactor;
        var localSelector = localController.Selector;

        var availableMoves = localSelector.GetAvailableMoves(unit.CurrentCell, unit.MovementPoints);
        if (availableMoves.Count == 0)
        {
            unit.Status = UnitStatus.Moved;
            return;
        }

        // ������� ���� ���������� ������
        var enemies = localGrid.AllUnits.Where(u => u.Type != unit.Type).ToArray();

        // �������� ���������� �����
        var targetEnemy = enemies.OrderBy(e => localInteractor.PathConstructor.GetDistance(unit.CurrentCell, e.CurrentCell)).FirstOrDefault();

        // ���� ����� �����, �������� � ����
        if (targetEnemy != null)
        {
            var targetCell = targetEnemy.CurrentCell;
            localInteractor.PathConstructor.FindPathToTarget(unit.CurrentCell, targetCell, out List<Tile> Path, _gameController.Grid);
            _gameController.MoveUnitAlongPath(unit, Path);
        }
        else
        {
            // ���� ������ ���, �������� � ��������� ��������� ������
            var randomIndex = Random.Range(0, availableMoves.Count);
            var targetCell = availableMoves[randomIndex];
            localInteractor.PathConstructor.FindPathToTarget(unit.CurrentCell, targetCell, out List<Tile> Path, _gameController.Grid);
            _gameController.MoveUnitAlongPath(unit, Path);
        }

        // ��������� ��������� �����
        localInteractor.UpdateUnit(unit);
        localSelector.UnselectUnit(unit); // ����� ������ ��������

        // ��������� ����������� ����� ����� �����������
        unit.CurrentCell.UnselectCell();
        unit.Status = UnitStatus.Moved;
        unit.Stats.MovementPoints = unit.Stats.MovementRange; // �������, ������ ��������

        _gameModel.EndTurn();
    }
}
