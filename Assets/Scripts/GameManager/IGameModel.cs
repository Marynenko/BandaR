using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameModel
{
    void StartGame();
    void StartTurn(Unit activePlayer);
    void EndTurn(Unit activePlayer, Unit _nextPlayer);

}
