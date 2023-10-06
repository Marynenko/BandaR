using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UnitMenu : MonoBehaviour
{
    [SerializeField] private InputPlayer input;
    public GameObject blockPanel;
    public GameObject mainContainerPlayer; // Меню игрока
    public GameObject mainContainerEnemy; // Меню врага

    public Button moveButton;
    public Button attackButton;
    public Button infoButton;
    public Button endTurnButton;
    
    private Unit _currentUnit;
    private static UIManager _instance;

    public static UIManager instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<UIManager>();
            return _instance;
        }
    }

    private void Start()
    {
        HideMenu();

        moveButton.onClick.AddListener(HandleMove);
        attackButton.onClick.AddListener(HandleAttack);
        infoButton.onClick.AddListener(HandleInfo);
        endTurnButton.onClick.AddListener(HandleEndTurn);
    }

    public void ShowMenu(Unit unit)
    {
        _currentUnit = unit;
        
        
        gameObject.SetActive(true);
        CheckUnitType(unit);
        blockPanel.SetActive(true);
    }
    
    private void CheckUnitType(Unit unit)
    {
        if(unit.Stats.Type == UnitType.Player)
        {
            // Открыть меню игрока и закрыть меню врага
            mainContainerPlayer.SetActive(true);
            mainContainerEnemy.SetActive(false);
        }
        else
        {
            // Открыть меню врага и закрыть меню игрока
            mainContainerEnemy.SetActive(true);
            mainContainerPlayer.SetActive(false);
        }
    }

    public void HideMenu()
    {
        gameObject.SetActive(false);
        blockPanel.SetActive(false);
    }

    private void HandleMove()
    {
        input.GameController.HandleUnitClick(_currentUnit);
    }

    private void HandleAttack()
    {
        Debug.Log("Attack action");
    }

    private void HandleInfo()
    {
        Debug.Log("Info action");
    }

    private void HandleEndTurn()
    {
        input.GameModel.HandleEndTurnButtonClicked(_currentUnit);
    }
}