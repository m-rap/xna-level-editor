﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Jitter;

namespace XleModel
{
    abstract public class GridObject : BaseObject
    {
        protected Grid grid;
        protected Vector2 gridPosition, prevGridPosition;

        public override Vector3 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                //if (grid.GridOutOfBounds(new Vector2(value.X, value.Z)))
                //    return;
                base.Position = value;
                position.Y = grid.Terrain.Vertices[(int)(position.X + position.Z * grid.Terrain.Width)].Position.Y + 0.1f;
                gridPosition.X = (float)Math.Floor(position.X / grid.Size);
                gridPosition.Y = (float)Math.Floor(position.Z / grid.Size);
            }
        }

        public virtual Vector2 GridPosition
        {
            get { return gridPosition; }
            set
            {
                if (grid.GridOutOfBounds(value))
                    return;
                gridPosition = value;
                Vector3 temp = position;
                temp.X = gridPosition.X * grid.Size;
                temp.Z = gridPosition.Y * grid.Size;
                base.Position = temp;
                position.Y = grid.Terrain.Vertices[(int)(position.X + position.Z * grid.Terrain.Width)].Position.Y + 0.1f;
            }
        }

        public GridObject(Game game, World physicsWorld, Grid grid)
            : base(game, physicsWorld)
        {
            this.grid = grid;
        }

        public virtual void CheckOrientation()
        {
        }

        public virtual void UpdateObserver()
        {
        }
    }


}
