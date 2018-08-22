using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using ProjectPlatformer.Grid;
using ProjectPlatformer.Blocks;
using ProjectPlatformer.Items.Weapons;
using ProtoBuf;
using Penumbra;

namespace ProjectPlatformer.Character
{
    public class Player
    {
        public Light light = new PointLight
        {
            //ConeDecay = 1,
            Position = new Vector2(0, 0),
            ShadowType = ShadowType.Occluded,
            Scale = new Vector2(1000f),
            Radius = 100f,
            Color = Color.White
            //Rotation = (float)Math.PI / 2f
        };

        public Weapon heldItem; // change to item class later, make the weapon class derive from that class

        public Camera camera { get; set; }

        public Position position { get; set; }
        public Cell playerCell { get; set; }
        public Vector2 Origin { get; set; }
        public Texture2D Sprite { get; set; }
        public int Width, Height;
        public float HalfWidth, HalfHeight;
        public float Speed { get; set; }
        public SpriteEffects flip { get; set; }

        private bool isfalling;
        private Vector2 velocity;
        Position previousPos = new Position(0, 0);

        private MouseState oldstate;

        #region Grid
        public Cell[] surCells { get; set; }
        Cell[] collCell;
        #endregion

        CollisionType[] colType;

        //player collision sections
        public int feetHeight = Cell.cellHeight * 2;
        public int rightHeight = Cell.cellHeight;
        public int rightWidth = Cell.cellWidth;
        public int leftHeight = Cell.cellHeight;
        public int leftwidth = Cell.cellHeight;

        bool stopped = false;
        bool isMoving = false;
        bool movingRight = false, movingLeft = false;

        public Player(bool localPlayer) // remove parameter?
        {
            if (localPlayer)
            {
                isfalling = true;
                position = new Position(PlatformerGame.screenCenter + new Vector2(1000, 1000));
                float halfWidth = PlatformerGame.settings.resolutionWidth / 2f;
                float halfHeight = PlatformerGame.settings.resolutionHeight / 2f;
                camera = new Camera(position.ToVector2(), halfWidth + Cell.cellWidth/2f, Map.mapWidth * Cell.cellWidth - PlatformerGame.settings.resolutionWidth - Cell.cellWidth/2f, halfHeight + Cell.cellHeight / 2f, Map.mapHeight * Cell.cellHeight - halfHeight - Cell.cellHeight/2f);

                Width = 3;
                HalfWidth = Width / 2f;
                Height = 5;
                HalfWidth = Height / 2f;
                Origin = new Vector2((Width * Cell.cellWidth) / 2, (Height * Cell.cellHeight) / 2);
                Speed = 10f;
            }
        }
        public Player()
        {

        }

        public void LoadContent(ContentManager content)
        {
            heldItem = Weapon.GetRandomWeapon(this);

            Sprite = PlatformerGame.Instance.Load<Texture2D>("trumperin0"); //Pixel(200, 200);

            playerCell = Cell.GetCell(position.ToVector2());
            surCells = Cell.GetAreaOfCells(playerCell, 4, 6);

            camera.SetDisplay(position.ToVector2());
        }

        public void Update()
        {
            MouseState mState = Mouse.GetState();

            if (!PlatformerGame.settings.EditorMode)
            {
                Movement();
                light.Position = Vector2.Transform(light.Position, camera.view);
            }

            if (mState.LeftButton == ButtonState.Pressed)
            {
                if (!heldItem.isUsing)
                    heldItem.isUsing = true;
            }
            if (heldItem.isUsing)
            {
                heldItem.Use();
            }

            if (mState.LeftButton == ButtonState.Pressed && oldstate.LeftButton == ButtonState.Released)
            {
                PlaceBlock();
            }
            oldstate = mState;
        }

        public void Movement()
        {
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.Space))
            {
                velocity.Y = -20f; //Vector2.Lerp(playerPosition, new Vector2(playerPosition.X, lastYPosition - 100f), 0.07f);
            }

            if (state.IsKeyDown(Keys.A))
            {
                velocity.X = -Speed;
            }
            else if (state.IsKeyDown(Keys.D))
            {
                velocity.X = Speed;
            }
            else
            {
                velocity.X = 0;
            }

            Vector2 pos1 = Cell.SnapToGrid(position.ToVector2());
            playerCell = Cell.GetCell(pos1);

            surCells = Cell.GetAreaOfCells(playerCell, Width + 4, Height + 4);

            movingRight = false;
            movingLeft = false;

            isMoving = false;

            int contacts = Collision.Colliding(new Vector2(position.X, position.Y), Width * Cell.cellWidth, Height * Cell.cellHeight, surCells, ref colType, ref collCell);
            if (contacts > 0)
            {
                if (Collision.Colliding(new Vector2(position.X, position.Y + (Height / 2f) * Cell.cellHeight - (feetHeight / 2f)), Width * Cell.cellWidth, feetHeight, surCells, ref colType, ref collCell) > 0)
                {
                    //Console.WriteLine("FEEEEET");
                    isfalling = false;

                    int heighestPos = int.MinValue;

                    for (int i = 0; i < collCell.Length; i++)
                    {
                        if (collCell[i].y > heighestPos)
                        {
                            heighestPos = collCell[i].y;
                        }
                    }

                    Vector2 newPosition = new Vector2(position.X, heighestPos - (Height / 2f) * Cell.cellHeight - Cell.cellHeight / 2);
                    position.Y = newPosition.Y;
                }
                //Console.WriteLine("contacts: {0}", contacts);
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

            if (isfalling)
            {
                velocity.Y += 9.82f * 0.075f;
            }

            if (velocity.X > 0.001f)
                flip = SpriteEffects.FlipHorizontally;
            else if (velocity.X < -0.001f)
                flip = SpriteEffects.None;

            Vector2 newPos = position.ToVector2() + velocity;

            Move(newPos);
        }

        private void Move(Vector2 newPos)
        {
            position = new Position(newPos);

            camera.MoveTowards(newPos);

            camera.HitMapWall();
        }

        public bool HasMoved(float distance)
        {
            if (Math.Abs(previousPos.X - position.X) > distance || Math.Abs(previousPos.Y - position.Y) > distance)
            {
                previousPos = position;
                return true;
            }
            return false;
        }

        public void Draw(SpriteBatch batch)
        {
            batch.Draw(Sprite, position.ToVector2(), null, Color.White, 0f, Origin, 1f, flip, 0f);
            heldItem.Draw(batch);
        }

        private void PlaceBlock()
        {
            Vector2 position = Cell.SnapToGrid(camera.PixelToWorldCoords(Mouse.GetState().Position.ToVector2()));
            Cell blockCell = Cell.GetCell(position);
            if (blockCell.block == null)
            {
                Block.PlaceBlock(blockCell, new Block(Block.BlockType.Dirt));
                Map.world.AddBlock(blockCell);

                PlatformerGame.Instance.AddLightBlocker(blockCell);

                if (PlatformerGame.multiplayer)
                {
                    PlatformerGame.Instance.net.ChangeCell(blockCell, Networking.CellChangeType.PlaceBlock);
                }
            }
            else
            {
                Console.WriteLine("You just tried to place a block, where there already is a block");
            }
        }
    }
}
