using UnityEngine;
using UnityEngine.UI;

public class UnitMenu : MonoBehaviour
{
    [SerializeField] private InputManager inputManager;

    private Unit _currentUnit;

    public GameObject MainContainerPlayer; // ���� ������
    public GameObject MainContainerEnemy; // ���� �����

    public Button MoveButton;
    public Button RotateButton;
    public Button AttackButton;
    public Button InfoPlayerButton;
    public Button InfoButton;
    public Button EndTurnButton;

    private void Start()
    {
        MoveButton.onClick.AddListener(HandleButtonMove);
        RotateButton.onClick.AddListener(HandleButtonRotate);
        AttackButton.onClick.AddListener(HandleButtonAttack);
        InfoPlayerButton.onClick.AddListener(HandleButtonInfo);
        InfoButton.onClick.AddListener(HandleButtonInfo);
        EndTurnButton.onClick.AddListener(HandleButtonEndTurn);
    }

    public void ShowMenu(Unit unit, bool isMoving)
    {
        _currentUnit = unit;
        inputManager.IsMenuActive = true;
        gameObject.SetActive(true);
        
        CheckUnitType(unit, isMoving);   
    }

    private void CheckUnitType(Unit unit, bool isMoving = false)
    {
        if (unit.Stats.Type == UnitType.Player && isMoving)
        {
            // ������� ���� ������ � ������� ���� �����
            var enemies = UIManager.Instance.PathConstructor.GetEnemyFromAdjacentTiles(unit);
            AttackButton.interactable = _currentUnit.Stats.CountAttacks != 0 && enemies.Count > 0;
            MainContainerEnemy.SetActive(false);
            MainContainerPlayer.SetActive(true);
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
        inputManager.IsMenuActive = false;
        gameObject.SetActive(false);
    }

    private void HandleButtonMove()
    {
        inputManager.IsTileClickable = true;
        inputManager.IsUnitClickable = false;
        UIManager.Instance.GameController.HandleUnitClick(_currentUnit);
    }
    
    private void HandleButtonRotate()
    {
        inputManager.IsTileClickable = true;
        inputManager.IsUnitClickable = true;
        inputManager.HasToRotate = true;

        if (_currentUnit != null)
        {
            var tilesToHighlight = UIManager.Instance.PathConstructor.GetAdjacentTiles(_currentUnit.OccupiedTile);
            UIManager.Instance.GridUI.HighlightTiles(tilesToHighlight, TileState.Rotation);    
        }
        
        HideMenu();
    }

    private void HandleButtonAttack()
    {
        HideMenu();
        inputManager.IsTileClickable = false;
        inputManager.IsAttackMenuActive = true;
        inputManager.IsUnitClickable = true;
        UIManager.Instance.AttackManager.uiAttackMenu.gameObject.SetActive(true); // For Attack Menu open instantly
        // UIManager.Instance.AttackManager.HandleAttackButtonClicked(_currentUnit); // For attack menu open after chose enemy
    }

    private void HandleButtonInfo()
    {
    }

    private void HandleButtonEndTurn()
    {
        UIManager.Instance.GameManager.HandleEndTurnButtonClicked(_currentUnit);
        UpdateUnitUI();
    }

    private void UpdateUnitUI()
    {
        var updateIndicators = UIManager.Instance.AttackManager.MovementIndicators;
        updateIndicators.Launch(updateIndicators.EnergyMax, _currentUnit.Stats.StateFatigue);
    }
}