using System;
using UnityEngine;
using UnityEngine.UI;

public class UnitMenu : MonoBehaviour
{
    [SerializeField] private InputPlayer _input;
    public GameObject _blockPanel;
    public GameObject MainContainerPlayer; // ���� ������
    public GameObject MainContainerEnemy; // ���� �����

    public Button moveButton;
    public Button attackButton;
    public Button infoButton;
    public Button endTurnButton;
    
    private Unit _currentUnit;
    private static UIManager _instance;

    public static UIManager Instance
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
        _blockPanel.SetActive(true);
    }
    
    private void CheckUnitType(Unit unit)
    {
        if(unit.Type == UnitType.Player)
        {
            // ������� ���� ������ � ������� ���� �����
            MainContainerPlayer.SetActive(true);
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
        gameObject.SetActive(false);
        _blockPanel.SetActive(false);
    }

    private void HandleMove()
    {
        _input.GameController.HandleUnitClick(_currentUnit);
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
        _input.GameModel.HandleEndTurnButtonClicked();
    }
}