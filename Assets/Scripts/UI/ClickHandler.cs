using UnityEngine;
using UnityEngine.EventSystems;

public class ClickHandler : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    private InputPlayer _inputPlayer;

    private void Awake()
    {
        _inputPlayer = FindObjectOfType<InputPlayer>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicked!");
        if (!UIManager.Instance.MenuAction.isActiveAndEnabled)
            _inputPlayer.HandleLeftClick(eventData.pressPosition);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Debug.Log("Навёл!");
        // UIManager.Instance.AttackManager.AttackIndicators.ModifyEnergyOnTime();
    }
}