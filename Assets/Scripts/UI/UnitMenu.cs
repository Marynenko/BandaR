using System;
using UnityEngine;
using UnityEngine.UI;

public class UnitMenu : MonoBehaviour
{
    [SerializeField] private InputPlayer _input;
    [SerializeField] private GameObject _blockPanel;


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
        _blockPanel.SetActive(true);
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
