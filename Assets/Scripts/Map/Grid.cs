using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{   
    [SerializeField] protected GridInteractor GGridInteractor;
    public List<Unit> Units;

    private void Start()
    {
        // Получить все Unit в игре и добавить их в список Units
        Units = FindObjectsOfType<Unit>().ToList();
        foreach (Unit unit in Units)
        {
            unit.InitializeUnit();
        }
    }

}
