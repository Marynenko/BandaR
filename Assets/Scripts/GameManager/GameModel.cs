using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameModel : MonoBehaviour, IGameModel
{
    [SerializeField] private GameController _controller;
    [SerializeField] private GridInteractor _interactor;

    private Unit _activePlayer;
    private Unit _opponentPlayer;


    public void EndTurn(Unit activePlayer, Unit opponentPlayer)
    {
        _activePlayer = activePlayer;
        _opponentPlayer = opponentPlayer;

        // Снимаем выделение с текущего юнита и доступность ячеек
        _interactor.UnselectUnit(_activePlayer);

        //_activePlayer.DisableCells();

        //// Снимаем выделение с выбранного юнита
        //_interactor.UnselectUnit(_controller.);

        //// Снимаем выделение доступных ходов
        //ClearHighlightedTiles();

        //// Передаем ход противнику
        //foreach (Player player in players)
        //{
        //    if (player.isCurrentPlayer)
        //    {
        //        player.isCurrentPlayer = false;
        //        break;
        //    }
        //}
        //foreach (Player player in players)
        //{
        //    if (!player.isCurrentPlayer)
        //    {
        //        player.isCurrentPlayer = true;
        //        break;
        //    }
        //}
    }
}
