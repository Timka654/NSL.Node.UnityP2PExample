using SimpleGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mono.Cecil;

public class Game : MonoBehaviour
{
    [SerializeField]
    NodeNetwork nodeNetwork;
    [SerializeField]
    TileView TilePrefab;

    public List<TileView> tileViews;

    private GameModel gameModel;
    NodeTransportClient client;
    public Player CurrentPlayer { get; private set; }
    public List<PlayerView> PlayerViews;

    public TileView tile { get; set; }

    private void Start()
    {
        gameModel = new GameModel(2);
        StartTheGame(gameModel, 0);
        //client = nodeNetwork.transportClient;
    }

    public void StartTheGame(GameModel gameModel, int PlayerTeam)
    {
        this.gameModel = gameModel;
        for (int i = 0; i < PlayerViews.Count; i++)
        {
            var pV = PlayerViews[i];
            pV.PlayerModel = gameModel.Players[i];
        }
        Init(gameModel.Field);

        CurrentPlayer = gameModel.Players.FirstOrDefault(p => p.Team == PlayerTeam);
        gameModel.StartNewTurn();
    }
    private void Init(Field field)
    {
        var startX = -3f;
        var startY = -3f;
        for (int i = 0; i < field.Tiles.GetLength(0); i++)
        {
            for (int j = 0; j < field.Tiles.GetLength(1); j++)
            {
                var tile = Instantiate(TilePrefab, new Vector3(startX + i * 0.6f, startX + j * 0.6f, 0), Quaternion.identity);
                tileViews.Add(tile);
                tile.GameView = this;
                tile.Init(gameModel.Field.Tiles[i, j], this);
            }
        }
    }

    public void OnTileClick(TileView tileView)
    {

        if (CurrentPlayer.TurnPhase == TurnPhase.FinishedTurn || CurrentPlayer.TurnPhase == TurnPhase.FinishedExpandPhase)
        {
            return;
        }

        var tile = tileView.TileModel;

        if (CurrentPlayer.TurnPhase == TurnPhase.ExpandPhase)
        {
            ResetAllTileViews();

            if (CurrentPlayer.Territory.Contains(tile))
            {

                if (tile.Power > 1 && tile.Neigbours.Any(t => !CurrentPlayer.Territory.Contains(t)))
                {
                    tileView.SetSelectedImage();
                    this.tile = tileView;
                }
            }
            else
            {
                if (this.tile == null) return;
                gameModel.PlayerExpand(CurrentPlayer, this.tile.TileModel, tile);
                client.SendPlayerExpand(CurrentPlayer.Team,
                    (this.tile.TileModel.Coordinates.Y, this.tile.TileModel.Coordinates.X),
                    (tile.Coordinates.Y, tile.Coordinates.X));
                ResetAllTileViews();
            }
        }
        else if (CurrentPlayer.TurnPhase == TurnPhase.EconomicsPhase)
        {
            if (CurrentPlayer.Territory.Contains(tile))
            {

                if (tile.Power < 10)
                {
                    gameModel.PlayerGrowTile(CurrentPlayer, tile);
                    client.SendPlayerGrowTile(CurrentPlayer.Team, (tile.Coordinates.Y, tile.Coordinates.X));
                }
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
