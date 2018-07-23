using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using System.Collections.Generic;
using _2DPlatformer.Blocks;
using _2DPlatformer.Grid;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using _2DPlatformer.Networking;

namespace _2DPlatformer
{
    public class PlatformerGame : Game
    {
        #region networking
        public static bool multiplayer = true;
        public static int port = 9009;
        public static string ip = "195.67.217.18";
        public static TcpClient client;
        public static TcpListener server;
        public static int playerIndex;
        public PlatformerNetworking platformerNetworking;
        public PlatformerNetworking latestPlatformerNetworking;
        List<Cell> blockCellsChanged = new List<Cell>();

        long timer = 0;
        float updateSpeed = 5;

        public static bool connectionDataRecieved = false;
        public bool lastStreamRecieved = true;

        Byte[] dataReciever = new byte[1500]; // look for resizing later
        Byte[] dataSender;

        NetworkStream stream;
        #endregion

        #region Grid
        Cell prevPlayerCell;
        Cell[] surCells;
        Cell[] collCell;
        #endregion

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Matrix view;
        MouseState oldstate;
        Vector2 cameraPosition;

        public Player player;
        CollisionType[] colType;

        //player collision sections
        int feetHeight = Cell.cellHeight * 2;
        int rightHeight = Cell.cellHeight;
        int rightWidth = Cell.cellWidth;
        int leftHeight = Cell.cellHeight;
        int leftwidth = Cell.cellHeight;

        //

        Vector2[] headPositions;
        Box head;

        bool isfalling;

        Vector2 velocity;
        Vector2 screenCenter;
        
        public PlatformerGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            SetApplicationSettings();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        #region Multiplayer
        private void LoadMultiplayer() // make a function for every type to send
        {
            NetworkManager.connectedClient = new List<PlatformerNetworking>(); // change
            client = NetworkManager.ConnectToServer(ip, port);
            if (client.Connected)
            {
                stream = client.GetStream();
            }
            platformerNetworking = new PlatformerNetworking(player, Cell.blockCells);
            //latestPlatformerNetworking;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        void RunTimer()
        {
            timer++;
        }
        void ResetTimer()
        {
            timer = 0;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        private void SendDataToServer()
        {
            if (latestPlatformerNetworking == null)
                latestPlatformerNetworking = new PlatformerNetworking();

            PlatformerNetworking dataChanges = platformerNetworking.ChangedVariables(latestPlatformerNetworking, blockCellsChanged);
            if (dataChanges == null) // fix bitte
                return;

            MemoryStream dataStream = Serialization.Serialize(dataChanges);
            byte[] bytes = Serialization.GetBytesFromStream(dataStream);
            byte[] send = new byte[bytes.Length + 1];
            Array.Copy(bytes, 0, send, 1, bytes.Length);
            send[0] = 0; // this is the metadata: sending type PlatformerNetworking

            try
            {
                stream.Flush();
                stream.Write(send, 0, send.Length);
                lastStreamRecieved = false;
            }
            catch
            {
                lastStreamRecieved = true;
            }

            bytes = new byte[bytes.Length];

            blockCellsChanged.Clear();

            //latestPlatformerNetworking = platformerNetworking;
            //Player newPlayer = Serialization.DeSerialize<Player>(playerStream);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        private void SetNetworkingVariables(byte[] bytes)
        {
            PlatformerNetworking newPlatformerNetwork = Serialization.DeSerialize<PlatformerNetworking>(bytes);
            if (NetworkManager.connectedClient.Count < 1)
            {
                NetworkManager.connectedClient.Add(newPlatformerNetwork);
            }
            NetworkManager.connectedClient[0] = newPlatformerNetwork; // need to change this for when there are more than two people on the server

            if (newPlatformerNetwork.blockCells != null)
            {
                //Cell.blockCells.AddRange(newPlatformerNetwork.blockCells);
                for (int i = 0; i < newPlatformerNetwork.blockCells.Count; i++)
                {
                    newPlatformerNetwork.blockCells[i].block.Sprite = Pixel(Cell.cellWidth, Cell.cellHeight);
                    Block.PlaceBlock(newPlatformerNetwork.blockCells[i], new Block(newPlatformerNetwork.blockCells[i].block.Sprite, Cell.cellWidth, Cell.cellHeight));
                    //newPlatformerNetwork.blockCells[i].block.Sprite = Pixel(Cell.cellWidth, Cell.cellHeight); // make a static funtion that converts an enum of a blocktype to a block
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        private void SendByte(byte send)
        {
            byte[] _send = new byte[] { 1 };
            stream.Write(_send, 0, 1);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        private void RecieveData(byte[] data)
        {
            byte[] bytes = new byte[data.Length - 1];
            Array.Copy(data, 1, bytes, 0, bytes.Length);

            if (!data.All(b => b == 0))
            {
                switch (data[0])
                {
                    case 0: // this is the data of the player
                        SetNetworkingVariables(bytes);
                        SendByte(1);
                        break;
                    case 1: // this tells us that the last data stream was recieved (properly)
                        lastStreamRecieved = true;
                        Console.WriteLine("the servers data was recieved!!");
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Console.WriteLine("all sent bytes are null");
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        private void UpdateMultiplayer()
        {
            platformerNetworking.player = player;
            platformerNetworking.blockCells = Cell.blockCells; // can possibly be optimized by adding to this while adding to the regular list.
            if (timer > updateSpeed) //|| lastStreamRecieved) // try and add some kind of timer as a backup for if the data was lost. ------------------------------------------------------------------- TAKE A LOOK PLEEEES.
            {
                ResetTimer();
                SendDataToServer();
            }
            else
            {
                RunTimer();
            }
            if (stream.DataAvailable) // get the data that has been sent by the server
            {
                int r = ReadIncomingData(dataReciever);
                if (r > 0)
                {
                    RecieveData(dataReciever);
                }
                dataReciever = new byte[dataReciever.Length];
            }

            /*if (!client.Connected || !connectionDataRecieved)
            {
                if (!client.Connected)
                {
                    try
                    {
                        client.Connect(ip, port);
                    }
                    catch (SocketException)
                    {

                    }
                }
                else if (!connectionDataRecieved)
                {
                    int i = stream.Read(dataReciever, 0, dataReciever.Length);
                    if (i != 0)
                    {
                        playerIndex = BitConverter.ToInt32(dataReciever, 0);
                        Console.WriteLine("playerindex: " + playerIndex);
                        connectionDataRecieved = true;
                    }
                }
                return;
            }*/

            //playerStream.CopyTo(stream);

            /*
            byte[] send01 = BitConverter.GetBytes(sendFloats[0]);
            byte[] send02 = BitConverter.GetBytes(sendFloats[1]);

            byte[] senderData = NetworkManager.CombineData(send01, send02);
            
            dataSender = new byte[senderData.Length + 1];
            Array.Copy(senderData, 0, dataSender, 1, senderData.Length);
            dataSender[0] = 0;

            NetworkManager.SendData(stream, dataSender, true);
            */
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        private int ReadIncomingData(byte[] reader)
        {
            if (stream.DataAvailable)
            {
                int i = stream.Read(reader, 0, reader.Length);
                return i;
            }
            return 0;
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void LoadContent()
        {
            Cell.blockCells.Clear();

            if (multiplayer)
                LoadMultiplayer();

            isfalling = true;
            screenCenter = new Vector2(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);

            player = new Player();

            //playerSprite = Content.Load<Texture2D>("player");
            player.Sprite = Content.Load<Texture2D>("trumperin0"); //Pixel(200, 200);
            player.position = new Position(screenCenter + new Vector2(1000, 1000));
            player.Width = 3;
            player.HalfWidth = player.Width / 2f;
            player.Height = 5;
            player.HalfWidth = player.Height / 2f;
            player.Origin = new Vector2((player.Width * Cell.cellWidth) / 2, (player.Height * Cell.cellHeight) / 2);
            player.Speed = 10f;

            spriteBatch = new SpriteBatch(GraphicsDevice);

            Cell.CreateGrid();

            player.playerCell = Cell.GetCell(player.position.ToVector2());
            surCells = Cell.GetAreaOfCells(player.playerCell, 4, 6);

            //byte[] newbytes = playerStream.GetBuffer();

            //int lo = BitConverter.ToInt32(newbytes, 0);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void UnloadContent()
        {
            
        }

        bool stopped = false;
        bool isMoving = false;
        bool movingRight = false, movingLeft = false;

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

            KeyboardState state = Keyboard.GetState();
            MouseState mState = Mouse.GetState();

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                velocity.Y = -20f; //Vector2.Lerp(playerPosition, new Vector2(playerPosition.X, lastYPosition - 100f), 0.07f);
            }

            if (state.IsKeyDown(Keys.A))
            {
                velocity.X = -player.Speed;
            }
            else if (state.IsKeyDown(Keys.D))
            {
                velocity.X = player.Speed;
            }
            else
            {
                velocity.X = 0;
            }

            Vector2 pos1 = Cell.SnapToGrid(player.position.ToVector2());
            player.playerCell = Cell.GetCell(pos1);

            surCells = Cell.GetAreaOfCells(player.playerCell, player.Width + 4, player.Height + 4);

            movingRight = false;
            movingLeft = false;

            isMoving = false;

            int contacts = Colliding(new Vector2(player.position.X, player.position.Y), player.Width * Cell.cellWidth, player.Height * Cell.cellHeight, surCells, ref colType, ref collCell);
            if (contacts > 0)
            {
                if (Colliding(new Vector2(player.position.X, player.position.Y+ (player.Height / 2f) * Cell.cellHeight - (feetHeight/2f)), player.Width * Cell.cellWidth, feetHeight, surCells, ref colType, ref collCell) > 0)
                {
                    Console.WriteLine("FEEEEET");
                    isfalling = false;

                    int heighestPos = int.MinValue;

                    for (int i = 0; i < collCell.Length; i++)
                    {
                        if (collCell[i].y > heighestPos)
                        {
                            heighestPos = collCell[i].y;
                        }
                    }

                    //if (!stopped)
                    //{
                        stopped = true;
                        Vector2 position = new Vector2(player.position.X, heighestPos - (player.Height/2f)*Cell.cellHeight - Cell.cellHeight/2);
                        //if (velocity.Y <= 0)
                        //{
                        player.position.Y = position.Y;
                        //}
                        //playerPosition.Y = position.Y;
                    //}
                }
                Console.WriteLine("contacts: {0}", contacts);
                /*for (int i = 0; i < contacts; i++)
                {
                    switch (colType[i])
                    {
                        case CollisionType.head:
                            //isMoving = true;
                            velocity.Y = 0;
                            break;
                        case CollisionType.right:
                            if (playerCell != prevPlayerCell)
                            {
                                isMoving = true;
                                movingRight = false;
                                movingLeft = true;
                                playerPosition.X = collCell[i].x - Cell.cellWidth;
                            }
                            break;
                        case CollisionType.left:
                            if (!isMoving)
                            {
                                isMoving = true;
                                movingRight = true;
                                movingLeft = false;
                                playerPosition.X = collCell[i].x + Cell.cellWidth;
                            }
                            break;
                        case CollisionType.feet:
                            //isMoving = true;
                            if (!stopped)
                            {
                                stopped = true;
                                isfalling = false;
                                playerPosition.Y = collCell[i].y - Cell.cellHeight * 2 + (Cell.cellHeight / 2);
                            }
                            break;
                    }
                }*/
            }
            else
            {
                stopped = false;
                isMoving = false;
                movingRight = false;
                movingLeft = false;
                isfalling = true;
            }

            if (!isfalling)
            {
                if (velocity.Y > 0)
                    velocity.Y = 0;
            }
            if (movingRight)
            {
                if (velocity.X > 0)
                    velocity.X = 0;
            }
            if (movingLeft)
            {
                if (velocity.X < 0)
                    velocity.X = 0;
            }
            prevPlayerCell = player.playerCell;

            if (mState.LeftButton == ButtonState.Pressed && oldstate.LeftButton == ButtonState.Released)
            {
                PlaceBlock(multiplayer);
            }
            oldstate = mState;

            if (isfalling)
            {
                velocity.Y += 9.82f * 0.075f;
            }

            player.position = new Position(player.position.ToVector2() + velocity);
            cameraPosition = Vector2.Lerp(cameraPosition,  player.position.ToVector2(), 0.1f);

            view = Matrix.CreateTranslation(new Vector3(screenCenter - cameraPosition, 0f));

            if (multiplayer)
            {
                UpdateMultiplayer();
            }

            base.Update(gameTime);
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        private void PlaceBlock(bool isMultiplayer)
        {
            Vector2 position = Cell.SnapToGrid(Mouse.GetState().Position.ToVector2() - new Vector2(view.Translation.X, view.Translation.Y));
            Cell blockCell = Cell.GetCell(position);
            Block.PlaceBlock(blockCell, new Block(Pixel(Cell.cellWidth, Cell.cellHeight), Cell.cellWidth, Cell.cellHeight));
            if (isMultiplayer)
            {
                blockCellsChanged.Add(blockCell);
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, view);

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
            if (NetworkManager.connectedClient.Count > 0)
            {
                foreach (PlatformerNetworking p in NetworkManager.connectedClient)
                {
                    spriteBatch.Draw(Pixel(player.Width * Cell.cellWidth, player.Height * Cell.cellHeight), p.player.position.ToVector2(), null, Color.Purple, 0f, player.Origin, Vector2.One, SpriteEffects.None, 0f);
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------------------------------

        #region Texture
        Texture2D Pixel(int w, int h)
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

        #region Collision
        int Colliding (Vector2 sectionOrigin, int width, int height, Cell[] nearbyCells, ref CollisionType[] collisionTypes, ref Cell[] edgeCells) //make this a function and feed in values to detect collisions for every object in the game. make collision-sections of the player with this function.. START SIMPLE!!!!!
        {
            Vector2 rect1 = new Vector2(sectionOrigin.X - (width/2f), sectionOrigin.Y - (height/2f));
            List<Cell> edge = new List<Cell>();
            List<CollisionType> colTypes = new List<CollisionType>();

            int contacts = 0;

            for (int i = 0; i < nearbyCells.Length; i++)
            {
                if (nearbyCells[i].block != null)
                {
                    Vector2 rect2 = new Vector2(nearbyCells[i].x - (Cell.cellWidth/2f), nearbyCells[i].y - (Cell.cellHeight/2f));
                    if (rect1.X < rect2.X + Cell.cellWidth &&
                    rect1.X + width > rect2.X &&
                    rect1.Y < rect2.Y + Cell.cellHeight &&
                    height + rect1.Y > rect2.Y)
                    {
                        contacts++;
                        colTypes.Add(nearbyCells[i].colType);
                        edge.Add(nearbyCells[i]);
                    }
                }
            }
            collisionTypes = colTypes.Where(c => c != CollisionType.none).ToArray(); //remove?
            edgeCells = edge.Where(e => e != null).ToArray();
            return contacts;
            /*

            Vector2 target = playerPosition;
                //also check if we are colliding with the bottom and on the side for the feet
                List<Cell> edge = new List<Cell>();
                int contacts = 0;
                List<CollisionType> colTypes = new List<CollisionType>();
                for (int i = 0; i < cells.Length; i++)
                {
                    if (cells[i].block != null)
                    {
                        contacts++;
                        colTypes.Add(cells[i].colType);
                        edge.Add(cells[i]);
                    }
                }
                edgeCells = edge.Where(e => e != null).ToArray();
                collisionTypes = colTypes.Where(c => c != CollisionType.none).ToArray();

                return contacts;
                */
        }
        void Colliding(Vector2 sectionTopLeft, int width, int height, Color clr, bool debug = true)
        {
            int halwid = width / 2;
            int halheight = height / 2;

            int cellR = (int)sectionTopLeft.X + halwid;
            int cellL = (int)sectionTopLeft.X - halwid;
            int cellU = (int)sectionTopLeft.Y - halheight;
            int cellD = (int)sectionTopLeft.Y + halheight;

            DebugLine(new Vector2(cellL, cellU), new Vector2(cellR, cellU), clr, 5);
            DebugLine(new Vector2(cellR, cellU), new Vector2(cellR, cellD), clr, 5);
            DebugLine(new Vector2(cellR, cellD), new Vector2(cellL, cellD), clr, 5);
            DebugLine(new Vector2(cellL, cellD), new Vector2(cellL, cellU), clr, 5);
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
