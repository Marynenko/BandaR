using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UnitMenu : MonoBehaviour
{
    [SerializeField] private InputPlayer InputPlayer;
    private Unit _currentUnit;
    
    public GameObject MainContainerPlayer; // Меню игрока
    public GameObject MainContainerEnemy; // Меню врага

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
        if(unit.Stats.Type == UnitType.Player && isMoving)
        {
            // Открыть меню игрока и закрыть меню врага
            MainContainerPlayer.SetActive(true);
            MainContainerEnemy.SetActive(false);
        }
        else
        {
            // Открыть меню врага и закрыть меню игрока
            MainContainerEnemy.SetActive(true);
            MainContainerPlayer.SetActive(false);
            // input.IsEnemyClicked = true;
        }
    }

    public void HideMenu()
    {
        InputPlayer.IsMenuActive = false;
        gameObject.SetActive(false);
    }

    public void HandleMove()
    {
        // Можно в будуем сделать что бы просто IsMOving = true;
        // input.IsMovementClicked = true;
        InputPlayer.IsTileClickable = true;
        InputPlayer.GameController.HandleUnitClick(_currentUnit);
    }

    private void HandleAttack()
    {
        InputPlayer.IsAttackClickable = true;
    }

    private void HandleInfo()
    {
        InputPlayer.IsInfoClickable = true;
    }

    private void HandleEndTurn()
    {
        InputPlayer.GameModel.HandleEndTurnButtonClicked(_currentUnit);
    }
}