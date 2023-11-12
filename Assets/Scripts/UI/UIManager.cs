using System;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UnitMenu Menu;
    public CameraManager CameraManager;
    public AttackManager AttackManager;
    public TurnManager TurnManager;
    public MovementManager MovementManager;
    public GridUI GridUI;
    public PathConstructor PathConstructor; // todo переместить это в GameManager или GameController

    public UnitMenu MenuAction
    {
        get => Menu;
        set => Menu = value;
    }

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
    
    public static float Heuristic(Tile currentTile, Tile endTile)
    {
        var dx = Math.Abs(currentTile.Coordinates.x - endTile.Coordinates.x);
        var dy = Math.Abs(currentTile.Coordinates.y - endTile.Coordinates.y);

        var cost = currentTile.MovementCost;

        if (dx > dy)
            return 1.001f * dx + dy + cost;
        return dx + 1.001f * dy + cost;
    }
}