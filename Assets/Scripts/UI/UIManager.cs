using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UnitMenu menuAction;
    public  UIGroupPortraits uiGroupPortraits;

    public UnitMenu MenuAction { get => menuAction; set => menuAction = value; }

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

    private void OnEnable()
    {
        Instance.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        Instance.gameObject.SetActive(false);
    }
}