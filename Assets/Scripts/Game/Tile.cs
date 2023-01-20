﻿using System.Security.Cryptography.X509Certificates;
using System;
using System.Collections.Generic;

namespace SimpleGame
{
    public class Tile
    {
        public (Player Player, int Team) Controller { get; private set; }
        public int Power { get; set; }
        public (int Y, int X) Coordinates { get; private set; }
        public List<Tile> Neigbours { get; private set; }


        public Action<(int, int)> OnCoordsChanged;
        public Action<(Player Player, int Team)> OnControllerChanged;
        
        
        public void SetCoordinates(int Y, int X)
        {
            Coordinates = (Y, X);
            OnCoordsChanged?.Invoke(Coordinates);
        }

        public void ChangeController(Player controller)
        {
            Controller = (controller, controller.Team);
            OnControllerChanged?.Invoke(Controller);
        }


        public void SetNeighbours(Field field)
        {
            if (Coordinates.X > 0 && Coordinates.Y > 0)
            {
                Neigbours.Add(field.Tiles[Coordinates.X - 1, Coordinates.Y - 1]);
            }
            if (Coordinates.X < field.Tiles.GetLength(0) && Coordinates.Y > 0)
            {
                Neigbours.Add(field.Tiles[Coordinates.X + 1, Coordinates.Y - 1]);
            }
            if (Coordinates.X < field.Tiles.GetLength(0) && Coordinates.Y < field.Tiles.GetLength(1))
            {
                Neigbours.Add(field.Tiles[Coordinates.X + 1, Coordinates.Y + 1]);
            }
            if (Coordinates.X < field.Tiles.GetLength(0) && Coordinates.Y < field.Tiles.GetLength(1))
            {
                Neigbours.Add(field.Tiles[Coordinates.X - 1, Coordinates.Y + 1]);
            }
        }
    }
    public enum State
    {
        Selected,
        Default
    }
}