using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using System.Collections.Generic;
using ProjectPlatformer.Blocks;
using ProjectPlatformer.Grid;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using ProjectPlatformer.Networking;
using ProjectPlatformer.Character;

namespace ProjectPlatformer
{
    public class PlatformerGame : Game
    {
        #region Singleton
        public static PlatformerGame instance;
        #endregion

        #region networking
        public static bool multiplayer = true;
        public NetworkManager net;
        #endregion

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Player player;

        public static Vector2 screenCenter;
        
        public PlatformerGame()
        {
            instance = this;
            Cell.CreateGrid();
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            SetApplicationSettings();
            net = new NetworkManager();
            player = new Player();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void LoadContent()
        {
            Cell.blockCells.Clear();

            screenCenter = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);

            player.LoadContent(Content);
            //playerSprite = Content.Load<Texture2D>("player");

            spriteBatch = new SpriteBatch(GraphicsDevice);

            if (multiplayer)
                net.LoadMultiplayer(player);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void UnloadContent()
        {
            
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void Update(GameTime gameTime)
        {
            //first check for collision on the entire player

            /*
            if (Colliding(new Vector2(playerPosition.X, playerPosition.Y - 25), playerWidth, playerHeight - 50, false, false, Color.White))
            {
                Console.WriteLine("Hit me head");
                //isfalling = false;
                //velocity = Vector2.Zero;
            }
            */
            //jumping

            player.Update();

            if (multiplayer)
            {
                net.UpdateMultiplayer();
            }

            base.Update(gameTime);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, player.camera.view);

            spriteBatch.Draw(Pixel(player.Width * Cell.cellWidth, player.Height * Cell.cellHeight), player.position.ToVector2(), null, Color.Green, 0f, player.Origin, Vector2.One, SpriteEffects.None, 0f);

            /*for (int i = 0; i < surCells.Length; i++)
            {
                Colliding(new Vector2(surCells[i].x, surCells[i].y), Cell.cellWidth, Cell.cellHeight, Color.Red);
            }*/

            //Colliding(new Vector2(player.position.X, player.position.Y + (player.Height/2f) * Cell.cellHeight - (feetHeight/2f)), player.Width * Cell.cellWidth, feetHeight, Color.Yellow);
            //Colliding(new Vector2(player.position.X, player.position.Y), player.Width * Cell.cellWidth, player.Height * Cell.cellHeight, Color.Black);

            foreach (Cell c in Cell.blockCells)
            {
                if (c.block != null)
                {
                    spriteBatch.Draw(c.block.Sprite, c.ToVector2(), null, Color.ForestGreen, 0f, new Vector2(Cell.cellWidth / 2, Cell.cellHeight / 2), 1f, SpriteEffects.None, 0f);
                }
            }

            spriteBatch.Draw(Pixel(10, 10), new Vector2(player.position.X, player.position.Y + (player.Height / 2f) * Cell.cellHeight - Cell.cellHeight), null, Color.Blue, 0f, new Vector2(5, 5), 1f, SpriteEffects.None, 0f);

            if (multiplayer) // multiplayer
                DrawMultiplayer();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        private void DrawMultiplayer()
        {
            if (net.connectedClient.Count > 0)
            {
                foreach (PlatformerNetworking p in net.connectedClient)
                {
                    spriteBatch.Draw(Pixel(player.Width * Cell.cellWidth, player.Height * Cell.cellHeight), p.player.position.ToVector2(), null, Color.Purple, 0f, player.Origin, Vector2.One, SpriteEffects.None, 0f);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        #region Texture
        public Texture2D Pixel(int w, int h)
        {
            Texture2D tx = new Texture2D(GraphicsDevice, w, h);
            Color[] c = new Color[w * h];

            for (int i = 0; i < c.Length; i++)
            {
                c[i] = Color.White;
            }

            tx.SetData(c);
            return tx;
        }
        #endregion

        #region Debug
        public void DebugLine(Vector2 begin, Vector2 end, Color color, int width = 1)
        {
            Rectangle r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length() + width, width);
            Vector2 v = Vector2.Normalize(begin - end);
            float angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
            if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
            spriteBatch.Draw(Pixel(1, 1), r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }
        #endregion
        #region Settings
        void SetApplicationSettings()
        {
            IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = 1920;
            graphics.PreferredBackBufferHeight = 1080;

            Window.IsBorderless = true;
            Window.Title = "2D-Platformer - Testing version";
        }
        #endregion
    }
    #region enums
    public enum CollisionType
    {
        none,
        head,
        right,
        left,
        feet
    };
    #endregion
}
