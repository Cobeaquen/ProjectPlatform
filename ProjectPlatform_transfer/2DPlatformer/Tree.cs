using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using ProtoBuf;
using ProjectPlatformer.Grid;
using ProjectPlatformer.Blocks;

namespace ProjectPlatformer
{
    [ProtoContract]
    public class Tree // make this derive from another class which can be used for all of these types
    {
        public Texture2D Sprite { get; set; }
        [ProtoMember(1)]
        public TreeType treeType { get; set; }
        [ProtoMember(2)]
        SpriteEffects flip { get; set; }
        [ProtoMember(3)]
        public Cell[] cells { get; set; }
        [ProtoMember(4)]
        public int height { get; set; }
        [ProtoMember(5)]
        public bool Flip { get; set; }

        public Vector2 pivot { get; set; }

        public Tree(int height, Cell bottomCell, bool flip)
        {
            this.height = height;
            Flip = flip;

            cells = new Cell[height];
            for (int y = 0; y < height; y++)
            {
                cells[y] = Cell.GetCell(bottomCell, 0, y);
            }

            int tree = PlatformerGame.rand.Next(0, 2);

            switch (tree)
            {
                case 0:
                    treeType = (TreeType)0;
                    Sprite = PlatformerGame.Instance.Load<Texture2D>(treeType.ToString());
                    break;
                case 1:
                    treeType = (TreeType)1;
                    Sprite = PlatformerGame.Instance.Load<Texture2D>(treeType.ToString());
                    break;
                default:
                    break;
            }

            if (Flip)
            {
                this.flip = SpriteEffects.FlipHorizontally;
            }
            else
            {
                this.flip = SpriteEffects.None;
            }

            pivot = new Vector2(Sprite.Width / 2, Sprite.Height);
        }
        public Tree()
        {

        }

        public void Place()
        {
            if (Sprite == null)
            {
                SetSprite();
            }
        }

        private void SetSprite()
        {
            Sprite = PlatformerGame.Instance.Load<Texture2D>(treeType.ToString());

            if (Flip)
            {
                flip = SpriteEffects.FlipHorizontally;
            }
            else
            {
                flip = SpriteEffects.None;
            }

            pivot = new Vector2(Sprite.Width / 2, Sprite.Height);
        }

        public void Draw(SpriteBatch batch, GameTime gameTime)
        {
            batch.Draw(Sprite, cells[0].ToVector2(), null, Color.White, 0f, pivot, height, flip, 0f);
        }

        [ProtoContract]
        public enum TreeType
        {
            FulGran,
            FulGran2
        }
    }
}
