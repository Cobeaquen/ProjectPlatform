using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using System.Collections.Generic;
using ProjectPlatformer.Blocks;
using ProjectPlatformer.Grid;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using ProjectPlatformer.Networking;
using ProjectPlatformer.Character;
using ProjectPlatformer.Time;
using System.Threading;

namespace ProjectPlatformer
{
    public class PlatformerGame : Game
    {
        #region Singleton
        public static PlatformerGame Instance { get; set; }
        #endregion

        #region networking
        public static bool multiplayer { get; set; }
        public NetworkClient net { get; set; }
        #endregion

        public Map map;

        public static Random rand;

        public static Settings settings { get; set; }
        public static string cwd { get; set; }
        public static string OptionsPath { get; set; }

        GraphicsDeviceManager graphics { get; set; }
        SpriteBatch spriteBatch { get; set; }

        public Player player { get; set; }

        public static Vector2 screenCenter { get; set; }

        private bool drawDebugging { get; set; } = false;

        public PlatformerGame()
        {
            Instance = this;
            rand = new Random();
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            SetApplicationSettings();
            net = new NetworkClient();
            player = new Player(true);
            map = new Map();

            if (settings.EditorMode)
            {
                settings.multiplayer = false;
                multiplayer = false;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void LoadContent()
        {
            screenCenter = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);

            // load or create new world
            map.LoadMap("new_world.world"); // will create map everytime
            //map.CreateMap(420);

            Cell.blockCells.Clear();

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
            map.SaveWorld("new_world.world");
            if (multiplayer)
            {
                net.Exit();
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void Update(GameTime gameTime)
        {
            if (Cell.CellsOnScreen == null)
            {
                Cell.UpdateCellsOnScreen(player.camera.position, settings.resolutionWidth, settings.resolutionHeight);
            }

            player.Update();

            if (settings.EditorMode)
            {
                player.camera.MoveAroundFreely();
            }

            if (player.camera.HasMoved(Cell.cellWidth))
            {
                Cell.UpdateCellsOnScreen(player.camera.position, settings.resolutionWidth, settings.resolutionHeight);
            }

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

            /*foreach (Cell c in Cell.blockCells) // get every cell on the screen instead
            {
                if (c.block != null)
                {
                    
                }
            }*/

            DrawWorld();

            if (multiplayer) // multiplayer
                net.Draw(spriteBatch);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawWorld()
        {
            //Cell[] cellsOnScreen = Cell.GetCellsOnScreen(player.camera.position, settings.resolutionWidth, settings.resolutionHeight);

            if (Cell.CellsOnScreen != null)
            {
                foreach (Cell cell in Cell.CellsOnScreen)//GetCellsOnScreen(player.camera.position, settings.resolutionWidth, settings.resolutionHeight))
                {
                    if (cell != null)
                        if (cell.block != null)
                        {
                            cell.DrawBlock(spriteBatch);
                        }
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
            OptionsPath = cwd + "\\Options\\";
            string settingsFilePath = OptionsPath + "Settings.platform";

            if (!File.Exists(settingsFilePath))
            {
                settings = new Settings()
                {
                    isBorderless = false,
                    multiplayer = false,
                    resolutionWidth = 1500,
                    resolutionHeight = 1200,
                    vsync = false,
                    EditorMode = false
                };
                Serialization.SerializeJson(settingsFilePath, settings);
            }
            else
            {
                settings = Serialization.DeserializeJson<Settings>(settingsFilePath);
            }
            Console.WriteLine(cwd);

            IsMouseVisible = true;
            Window.AllowUserResizing = true;
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
