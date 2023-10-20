using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UnitMenu Menu;
    public CameraManager CameraManager;
    // public MovementManager MovementManager;
    public AttackManager AttackManager;

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
    
    public static float GetDistance(Tile tileFrom, Tile tileTo)
    {
        if (tileFrom == tileTo)
            return 0;
        var dx = tileFrom.Coordinates.x - tileTo.Coordinates.x;
        var dy = tileFrom.Coordinates.y - tileTo.Coordinates.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}