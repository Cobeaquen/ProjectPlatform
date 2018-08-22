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
using ProjectPlatformer.Items.Weapons.Swords;
using Penumbra;

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

        #region Lighting
        public PenumbraComponent penumbra;

        public Light sun = new PointLight
        {
            //ConeDecay = 100,
            Intensity = 2,
            Position = new Vector2(0, 0),
            ShadowType = ShadowType.Occluded,
            CastsShadows = false,
            Scale = new Vector2(2000f),
            Radius = 2000f,
            Color = Color.White,
            //Rotation = MathHelper.PiOver2
        };

        float halfwidth = Cell.cellWidth / 2f;
        float halfheight = Cell.cellHeight / 2f;
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

            graphics = new GraphicsDeviceManager(this)
            {
                PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8
            };

            #region Lighting
            penumbra = new PenumbraComponent(Instance)
            {
                AmbientColor = Color.DarkGray,
                Debug = false
            };

            penumbra.Lights.Clear();
            penumbra.Hulls.Clear();
            #endregion

            rand = new Random();
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

        public void AddLightBlocker(Cell cell)
        {
            Hull newHull = new Hull(Block.cornerPoints)
            {
                Position = Vector2.Transform(cell.ToVector2(), player.camera.view)
            };
            cell.hull = newHull;

            penumbra.Hulls.Add(cell.hull);
        }
        public void AddLight(Light light)
        {
            penumbra.Lights.Add(light);
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            screenCenter = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);

            // load or create new world
            map.LoadMap("new_world.world"); // will create map everytime
            //map.CreateMap(420);

            Cell.blockCells.Clear();

            player.LoadContent(Content);

            //playerSprite = Content.Load<Texture2D>("player");

            if (multiplayer)
                net.LoadMultiplayer(player);

            penumbra.Initialize();
            //penumbra.Transform = player.camera.view;
            //Components.Add(penumbra);
            penumbra.Lights.Add(sun);
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
            //player.light.Position = Vector2.Zero;

            if (Cell.CellsOnScreen == null)
            {
                Cell.UpdateCellsOnScreen(player.camera.position, settings.resolutionWidth, settings.resolutionHeight);
            }

            player.Update();

            #region Lighting
            sun.Position = Vector2.Transform(player.position.ToVector2(), player.camera.view);
            #endregion

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
            penumbra.BeginDraw();

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, player.camera.view);

            player.Draw(spriteBatch); // draw player

            if (drawDebugging)
            {
                DrawDebug();
            }

            if (multiplayer)
            {
                /*foreach (var p in net.connectedClients)
                {
                    net.DrawPlayer(spriteBatch, );
                }*/
            }

            DrawWorld(gameTime);

            spriteBatch.End();

            penumbra.Draw(gameTime);

            //spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, null, player.camera.view);
            //player.Draw(spriteBatch); // draw player
            //spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawWorld(GameTime gameTime)
        {
            if (Cell.CellsOnScreen != null) // add trees n shet to this
            {
                foreach (Cell cell in Cell.CellsOnScreen)
                {
                    if (cell != null)
                    {
                        if (cell.block != null)
                        {
                            if (cell.hull == null)
                            {
                                AddLightBlocker(cell); // could clear the list before adding new ones, that will save memory and cost on the cpu.. thats not too esy
                            }
                            /*if (cell.hull != null)
                                cell.UpdateLighting();*/
                            cell.UpdateLighting();
                            cell.DrawBlock(spriteBatch);
                        }
                        if (cell.playerEntities.Count > 0)
                        {
                            for (int i = 0; i < cell.playerEntities.Count; i++)
                            {
                                net.DrawPlayer(spriteBatch, cell.playerEntities[i]);
                            }
                        }
                    }
                }
                foreach (var tree in Map.world.trees)
                {
                    tree.Draw(spriteBatch, gameTime);
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
                settings = new Settings() // default settings
                {
                    isBorderless = false,
                    multiplayer = false,
                    resolutionWidth = 1500,
                    resolutionHeight = 1000,
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
    
    #endregion
}