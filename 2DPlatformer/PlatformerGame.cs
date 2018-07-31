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
using ProjectPlatformer.Time;

namespace ProjectPlatformer
{
    public class PlatformerGame : Game
    {
        #region Singleton
        public static PlatformerGame Instance;
        #endregion

        #region networking
        public static bool multiplayer;
        public NetworkClient net;
        #endregion

        public Settings settings;
        public static string cwd;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Player player;

        public static Vector2 screenCenter;

        private bool drawDebugging = false;
        
        public PlatformerGame()
        {
            Instance = this;
            Cell.CreateGrid();
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            SetApplicationSettings();
            net = new NetworkClient();
            player = new Player(true);
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

        public T Load<T>(string assetName)
        {
            return Content.Load<T>(assetName);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void UnloadContent()
        {
            if (multiplayer)
            {
                net.Exit();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void Update(GameTime gameTime)
        {
            player.Update();

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

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

            spriteBatch.Draw(player.Sprite, player.position.ToVector2(), null, Color.White, 0f, player.Origin, Vector2.One, SpriteEffects.None, 0f); // draw player

            if (drawDebugging)
            {
                DrawDebug();
            }

            foreach (Cell c in Cell.blockCells)
            {
                if (c.block != null)
                {
                    spriteBatch.Draw(c.block.Sprite, c.ToVector2(), null, Color.White, 0f, new Vector2(Cell.cellWidth / 2, Cell.cellHeight / 2), 1f, SpriteEffects.None, 0f);
                }
            }

            if (multiplayer) // multiplayer
                DrawMultiplayer();

            spriteBatch.End();

            base.Draw(gameTime);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        private void DrawMultiplayer()
        {
            if (net.connectedClients.Count > 0)
            {
                try
                {
                    foreach (NetworkPlayer p in net.connectedClients)
                    {
                        spriteBatch.Draw(player.Sprite, new Vector2(p.xPos, p.yPos), null, Color.Purple, 0f, player.Origin, Vector2.One, SpriteEffects.None, 0f); // optimize
                    }
                }
                catch (Exception)
                {

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
        private void DrawDebug()
        {
            for (int i = 0; i < player.surCells.Length; i++)
            {
                Collision.DebugColliding(new Vector2(player.surCells[i].x, player.surCells[i].y), Cell.cellWidth, Cell.cellHeight, Color.Red);
            }

            Collision.DebugColliding(new Vector2(player.position.X, player.position.Y + (player.Height / 2f) * Cell.cellHeight - (player.feetHeight / 2f)), player.Width * Cell.cellWidth, player.feetHeight, Color.Yellow);
            Collision.DebugColliding(new Vector2(player.position.X, player.position.Y), player.Width * Cell.cellWidth, player.Height * Cell.cellHeight, Color.Black);
        }

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
            cwd = Directory.GetCurrentDirectory();
            string optionsPath = cwd + "\\Options\\";
            string settingsFilePath = optionsPath + "Settings.projectplatform";
            Console.WriteLine();
            if (!File.Exists(settingsFilePath))
            {
                settings = new Settings()
                {
                    isBorderless = false,
                    multiplayer = false,
                    resolutionWidth = 1500,
                    resolutionHeight = 1200,
                    vsync = false
                };
                Serialization.SerializeJson(settingsFilePath, settings);
            }
            else
            {
                settings = Serialization.DeserializeJson<Settings>(optionsPath + "Settings.projectplatform");
            }
            Console.WriteLine(cwd);

            IsMouseVisible = true;
            graphics.PreferredBackBufferWidth = settings.resolutionWidth;
            graphics.PreferredBackBufferHeight = settings.resolutionHeight;

            Window.IsBorderless = settings.isBorderless;
            multiplayer = settings.multiplayer;
            graphics.SynchronizeWithVerticalRetrace = settings.vsync;
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
