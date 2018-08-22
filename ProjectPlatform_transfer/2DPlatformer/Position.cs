using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using ProtoBuf;

namespace ProjectPlatformer
{
    [ProtoContract]
    public class Position
    {
        [ProtoMember(1)]
        public float X;// { get; set; }
        [ProtoMember(2)]
        public float Y;// { get; set; }

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
