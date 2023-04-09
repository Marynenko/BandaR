using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameModel
{
    void EndTurn(Unit activePlayer, Unit opponentPlayer);
}
