using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace _2DPlatformer
{
    public class Box
    {
        public float width, height;
        public Vector2 position;
        public BoxType boxType;

        public Box(float _width, float _height, Vector2 _position, BoxType _boxtype)
        {
            width = _width;
            height = _height;
            boxType = _boxtype;
            position = _position;
        }

        public void LoadContent()
        {

        }

        public void Update ()
        {

        }
    }

    public enum BoxType
    {
        Head,
        Right,
        Left,
        Feet
    };
}
