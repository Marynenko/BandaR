using UnityEditor;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UnitMenu _menuAction;

    public UnitMenu MenuAction { get => _menuAction; set => _menuAction = value; }

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
}
