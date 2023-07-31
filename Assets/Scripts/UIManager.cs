using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _menuAction;

    public GameObject MenuAction { get => _menuAction; set => _menuAction = value; }

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

    public void OpenMenuAction()
    {
        MenuAction.SetActive(true);
    }

    public void CloseMenuAction()
    {
        MenuAction.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
