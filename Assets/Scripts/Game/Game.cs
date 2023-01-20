using SimpleGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Game : MonoBehaviour
{
    [SerializeField]
    TileView TilePrefab;

    public List<TileView> tileViews;

    private GameModel gameModel;

    public Player CurrentPlayer { get; private set; }
    public List<PlayerView> PlayerViews { get; set; }

    public TileView CurrentTile { get; set; }

    //private void Start()
    //{
    //    gameModel = new GameModel(2);
    //    Init(gameModel.Field);
    //}

    public void StartTheGame(GameModel gameModel, int PlayerTeam)
    {
        this.gameModel = gameModel;
        Init(gameModel.Field);

        CurrentPlayer = gameModel.Players.FirstOrDefault(p => p.Team == PlayerTeam);
    }
    private void Init(Field field)
    {
        for (int i = 0; i < field.Tiles.GetLength(0); i++)
        {

            for (int j = 0; j < field.Tiles.GetLength(1); j++)
            {
                var tile = Instantiate(TilePrefab, new Vector3(i, j, 0), Quaternion.identity);
                tileViews.Add(tile);
                tile.GameView = this;
            }
        }
    }

    public void OnTileClick(TileView tileView)
    {

        if (CurrentPlayer.TurnPhase == TurnPhase.FinishedTurn || CurrentPlayer.TurnPhase == TurnPhase.FinishedExpandPhase)
        {
            return;
        }

        var tile = tileView.Tile;

        if (CurrentPlayer.TurnPhase == TurnPhase.ExpandPhase)
        {
            ResetAllTileViews();

            if (CurrentPlayer.Territory.Contains(tile))
            {

                if (tile.Power > 1 && tile.Neigbours.Any(t => !CurrentPlayer.Territory.Contains(t)))
                {
                    tileView.SetSelectedImage();
                    CurrentTile = tileView;
                }
            }
            else
            {
                if (CurrentTile == null) return;
                gameModel.PlayerExpand(CurrentPlayer, CurrentTile.Tile, tile);
                ResetAllTileViews();
            }
        }

    }

    private void ResetAllTileViews()
    {
        foreach (var tileView in tileViews)
        {
            tileView.state = TileView.State.Default;
            tileView.UpdateImage();
        }
    }

}
