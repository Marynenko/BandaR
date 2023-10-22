using UnityEngine;
using UnityEngine.UI;

public class UnitMenu : MonoBehaviour
{
    [SerializeField] private InputPlayer InputPlayer;

    private Unit _currentUnit;

    public GameObject MainContainerPlayer; // ���� ������
    public GameObject MainContainerEnemy; // ���� �����

    public Button MoveButton;
    public Button AttackButton;
    public Button InfoPlayerButton;
    public Button InfoButton;
    public Button EndTurnButton;

    private void Start()
    {
        MoveButton.onClick.AddListener(HandleMove);
        AttackButton.onClick.AddListener(HandleAttack);
        InfoPlayerButton.onClick.AddListener(HandleInfo);
        InfoButton.onClick.AddListener(HandleInfo);
        EndTurnButton.onClick.AddListener(HandleEndTurn);
    }

    public void ShowMenu(Unit unit, bool isMoving)
    {
        _currentUnit = unit;

        InputPlayer.IsMenuActive = true;
        gameObject.SetActive(true);
        
        CheckUnitType(unit, isMoving);   
    }

    private void CheckUnitType(Unit unit, bool isMoving = false)
    {
        if (unit.Stats.Type == UnitType.Player && isMoving)
        {
            // ������� ���� ������ � ������� ���� �����
            MainContainerPlayer.SetActive(true);
            
            var enemies = InputPlayer.GameModel.GetEnemyFromNeighbours(unit);
            if (_currentUnit.Stats.CountAttacks == 0 || enemies.Count == 0)
                AttackButton.interactable = false;
            else AttackButton.interactable = true;
            // AttackButton.interactable = _currentUnit.Stats.CountAttacks != 0;
            MainContainerEnemy.SetActive(false);
        }
        else
        {
            // ������� ���� ����� � ������� ���� ������
            MainContainerEnemy.SetActive(true);
            MainContainerPlayer.SetActive(false);
        }
    }

    public void HideMenu()
    {
        InputPlayer.IsMenuActive = false;
        gameObject.SetActive(false);
    }

    public void HandleMove()
    {
        // ����� � ������ ������� ��� �� ������ IsMOving = true;
        // input.IsMovementClicked = true;
        InputPlayer.IsTileClickable = true;
        InputPlayer.GameController.HandleUnitClick(_currentUnit);
    }

    private void HandleAttack()
    {
        HideMenu();
        InputPlayer.IsTileClickable = false;
        InputPlayer.IsAttackActive = true;
        InputPlayer.IsUnitClickable = true;
        UIManager.Instance.AttackManager.HandleAttackButtonClicked(_currentUnit);
    }

    private void HandleInfo()
    {
    }

    private void HandleEndTurn()
    {
        InputPlayer.GameModel.HandleEndTurnButtonClicked(_currentUnit);
        UpdateUnitUI();
    }

    private void UpdateUnitUI()
    {
        var updateIndicators = UIManager.Instance.AttackManager.AttackIndicators;
        updateIndicators.Launch(updateIndicators.EnergyMax, _currentUnit.Stats.StateFatigue);
    }
}