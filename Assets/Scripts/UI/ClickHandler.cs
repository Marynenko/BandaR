using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickHandler : MonoBehaviour, IPointerClickHandler
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
}
 