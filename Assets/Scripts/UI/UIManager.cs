using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UnitMenu Menu;

    public UnitMenu MenuAction { get => Menu; set => Menu = value; }

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