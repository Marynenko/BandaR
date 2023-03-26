using System.Collections.Generic;
using UnityEngine;

public class Player : Unit
{
    public float viewRange;
    public float attackRange;

    private List<Unit> _playersInRange;
}
