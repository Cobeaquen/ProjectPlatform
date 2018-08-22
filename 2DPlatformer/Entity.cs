using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ProjectPlatformer
{
    public class Entity
    {
        public Vector2 position;
        public Vector2 pivot;
        public Texture2D Sprite;
        public SpriteEffects flip;

        public virtual void Draw(SpriteBatch batch, Vector2 position)
        {

        }
    }
}