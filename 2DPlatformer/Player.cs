using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using _2DPlatformer.Grid;

namespace _2DPlatformer
{
    [Serializable]
    public class Player
    {
        public Position position;
        [NonSerialized]public Cell playerCell;
        [NonSerialized]public Vector2 Origin;
        [NonSerialized]public Texture2D Sprite;
        public int Width, Height;
        [NonSerialized]public float HalfWidth, HalfHeight;
        public float Speed;

        public Player()
        {
            position = new Position(0, 0);
        }
    }
}
