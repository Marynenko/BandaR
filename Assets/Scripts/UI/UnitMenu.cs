using UnityEngine;
using UnityEngine.UI;

public class UnitMenu : MonoBehaviour
{
    [SerializeField] private InputPlayer input;
    public GameObject mainContainerPlayer; // ���� ������
    public GameObject mainContainerEnemy; // ���� �����

    public Button moveButton;
    public Button attackButton;
    public Button infoButton;
    public Button endTurnButton;
    
    private Unit _currentUnit;

    private void Start()
    {
        moveButton.onClick.AddListener(HandleMove);
        attackButton.onClick.AddListener(HandleAttack);
        infoButton.onClick.AddListener(HandleInfo);
        endTurnButton.onClick.AddListener(HandleEndTurn);
    }

    public void ShowMenu(Unit unit, bool isMoving)
    {
        _currentUnit = unit;

        // input.IsMenuActive = true;
        input.IsMenuActive = true;
        gameObject.SetActive(true);
        CheckUnitType(unit, isMoving);
        
    }
    
    private void CheckUnitType(Unit unit, bool isMoving = false)
    {
        if(unit.Stats.Type == UnitType.Player && isMoving)
        {
            // ������� ���� ������ � ������� ���� �����
            mainContainerPlayer.SetActive(true);
            mainContainerEnemy.SetActive(false);
        }
        else
        {
            // ������� ���� ����� � ������� ���� ������
            mainContainerEnemy.SetActive(true);
            mainContainerPlayer.SetActive(false);
            // input.IsEnemyClicked = true;
        }
    }

    public void HideMenu()
    {
        input.IsMenuActive = false;
        gameObject.SetActive(false);
    }

    public void HandleMove()
    {
        // ����� � ������ ������� ��� �� ������ IsMOving = true;
        // input.IsMovementClicked = true;
        input.IsTileClickable = true;
        input.GameController.HandleUnitClick(_currentUnit);
    }

    private void HandleAttack()
    {
        input.IsAttackClickable = true;
    }

    private void HandleInfo()
    {
        input.IsInfoClickable = true;
    }

    private void HandleEndTurn()
    {
        input.GameModel.HandleEndTurnButtonClicked(_currentUnit);
    }
}