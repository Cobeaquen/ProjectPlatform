using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using ProjectPlatformer.Grid;
using ProtoBuf;

namespace ProjectPlatformer.Networking
{
    [ProtoContract]
    public class NetworkPlayer : Entity // only draw the player if it is close
    {
        [ProtoMember(1)]public float xPos;
        [ProtoMember(2)]public float yPos;
        [ProtoMember(3)]public int playerIndex;
        public Cell cell;

        public NetworkPlayer()
        {
            xPos = 0;
            yPos = 0;
            playerIndex = 0;
            cell = new Cell(0, 0);
            position = new Microsoft.Xna.Framework.Vector2(xPos, yPos);
            flip = SpriteEffects.FlipHorizontally;
            //Sprite = PlatformerGame.Instance.Load<Texture2D>("trumperin0"); // fix
        }

        public override void Draw(SpriteBatch batch, Microsoft.Xna.Framework.Vector2 position)
        {
            if (Sprite == null)
            {
                Sprite = PlatformerGame.Instance.Load<Texture2D>("trumperin0");
                pivot = new Microsoft.Xna.Framework.Vector2(Sprite.Width / 2f, Sprite.Height / 2f);
            }
            batch.Draw(Sprite, position, null, Microsoft.Xna.Framework.Color.White, 0f, pivot, 1f, flip, 0f);
            base.Draw(batch, position);
        }

        public NetworkPlayer Clone()
        {
            return new NetworkPlayer()
            {
                xPos = xPos,
                yPos = yPos,
                playerIndex = playerIndex,
                cell = cell,
                //position = position,
                flip = flip,
                Sprite = Sprite
            };
        }
        /*public Entity ToEntity()
        {
            return new Entity()
            {
                flip = flip ? Microsoft.Xna.Framework.Graphics.SpriteEffects.None : Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally,
                position = new Microsoft.Xna.Framework.Vector2(xPos, yPos)
                //set sprite here
            };
        }*/
    }
}
