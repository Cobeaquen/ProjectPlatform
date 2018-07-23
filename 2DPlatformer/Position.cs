using System;
using Microsoft.Xna.Framework;

namespace _2DPlatformer
{
    [Serializable]
    public class Position
    {
        public float X;
        public float Y;

        public Position(float x, float y)
        {
            X = x;
            Y = y;
        }
        public Position(Vector2 pos)
        {
            X = pos.X;
            Y = pos.Y;
        }
        public Vector2 ToVector2()
        {
            return new Vector2(X, Y);
        }
    }
}
