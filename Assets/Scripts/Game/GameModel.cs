﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleGame
{
    public struct GameModel
    {
        public Field Field { get; set; }
        public List<Player> Players;

        public GameModel(int playersCount)
        {
            Players = new List<Player>();
            for (int i = 0; i < playersCount; i++)
            {
                var player = new Player();
                player.Team = i;
                player.Territory = new List<Tile>();
                Players.Add(player);
            }

            Field = new Field(25, 25);

            Field.SetPlayers(Players);
        }

        public void StartNewTurn()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].TurnPhase = TurnPhase.ExpandPhase;
            }
        }

        public void PlayerExpand(Player player, Tile origin, Tile target)
        {
            if (!origin.Neigbours.Contains(target))
            {
                throw new Exception("Can't attack not neigbour tile");
            }

            if (target.Power >= origin.Power - 1)
            {
                target.Power -= (origin.Power - 1);
                origin.Power = 1;
                return;
            }

            if (target.Controller.Player != null)
            {
                target.Controller.Player.Territory.Remove(target);
            }

            target.ChangeController(player);
            player.Territory.Add(target);
            target.Power = origin.Power - target.Power - 1;
            origin.Power = 1;

            if (player.ActiveTiles.Count == 0)
            {
                PlayerFinishedExpand(player);
            }
        }

        public void PlayerFinishedExpand(Player p)
        {
            p.TurnPhase = TurnPhase.FinishedExpandPhase;
            if (Players.Any(p => p.TurnPhase != TurnPhase.FinishedExpandPhase))
            {
                StartEconomicPhase();
            }
        }

        private void StartEconomicPhase()
        {
            for (int i = 0; i < Players.Count; i++)
            {
                Players[i].Power = Players[i].Territory.Count;
                Players[i].TurnPhase = TurnPhase.EconomicsPhase;
            }
        }

        public void PlayerGrowTile(Player p, Tile t)
        {
            if (!p.Territory.Contains(t))
            {
                throw new Exception("Player can't grow that tile");
            }
            if (p.Power == 0)
            {
                throw new Exception("Player has no power left");
            }
            if (t.Power >= 10)
            {
                throw new Exception("Player can't grow that tile");
            }

            p.Power--;
            t.Power++;

            if (p.Power == 0)
            {
                PlayerFinishedTurn(p);
            }
        }

        private void PlayerFinishedTurn(Player p)
        {
            p.Power = 0;
            p.TurnPhase = TurnPhase.FinishedTurn;
            if (Players.Any(p => p.TurnPhase != TurnPhase.FinishedTurn))
            {
                FinishTurn();
            }

        }

        private void FinishTurn()
        {
            var elminated = Players.Where(p => p.Territory.Count == 0).ToList();
            foreach (var player in elminated)
            {
                Players.Remove(player);
                // Player playter defeated;
            }
            if(Players.Count == 1)
            {
                GameFinished(Players[0]);
            }
        }

        private void GameFinished(Player player)
        {
            // Player player won
            throw new NotImplementedException();
        }
    }
}
