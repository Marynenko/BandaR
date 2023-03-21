using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRay : MonoBehaviour
{
    [SerializeField] private GridInteractor _gridInteractor;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject.CompareTag("Unit"))
                {
                    var unit = hit.collider.gameObject.GetComponent<Unit>();
                    _gridInteractor.ChangeUnitStats(unit, unit.Status);
                }
                foreach (var unit in _gridInteractor.Units)
                {
                    if (unit.CurrentCell.Position == hit.collider.GetComponent<Cell>().Position) // unit active
                    {
                        if (unit.Status == UnitStatus.Unselected)
                            _gridInteractor.ChangeUnitStats(unit, UnitStatus.Unselected);
                        else
                        _gridInteractor.ChangeUnitStats(unit, UnitStatus.Selected);
                    }
                    // Проверка - выбор пустой клетки, при Selected = true;
                    else if (unit.Status == UnitStatus.Selected)
                        _gridInteractor.ChangeUnitStats(unit, UnitStatus.Selected);
                    else
                        _gridInteractor.ChangeUnitStats(unit, UnitStatus.Unselected);
                }
            }
        }
    }
}
