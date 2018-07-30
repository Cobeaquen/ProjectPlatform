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
using ProtoBuf;

namespace ProjectPlatformer.Character
{
    public class Player
    {
        public Camera camera;

        public Position position;
        public Cell playerCell;
        public Vector2 Origin;
        public Texture2D Sprite;
        public int Width, Height;
        public float HalfWidth, HalfHeight;
        public float Speed;

        bool isfalling;
        Vector2 velocity;

        private MouseState oldstate;

        #region Grid
        Cell prevPlayerCell;
        Cell[] surCells;
        Cell[] collCell;
        #endregion

        CollisionType[] colType;

        //player collision sections
        int feetHeight = Cell.cellHeight * 2;
        int rightHeight = Cell.cellHeight;
        int rightWidth = Cell.cellWidth;
        int leftHeight = Cell.cellHeight;
        int leftwidth = Cell.cellHeight;

        bool stopped = false;
        bool isMoving = false;
        bool movingRight = false, movingLeft = false;

        public Player(bool localPlayer)
        {
            if (localPlayer)
            {
                camera = new Camera();
                isfalling = true;
                position = new Position(PlatformerGame.screenCenter + new Vector2(1000, 1000));
                Width = 3;
                HalfWidth = Width / 2f;
                Height = 5;
                HalfWidth = Height / 2f;
                Origin = new Vector2((Width * Cell.cellWidth) / 2, (Height * Cell.cellHeight) / 2);
                Speed = 10f;
            }
        }

        public void LoadContent(ContentManager content)
        {
            Sprite = content.Load<Texture2D>("trumperin0"); //Pixel(200, 200);

            playerCell = Cell.GetCell(position.ToVector2());
            surCells = Cell.GetAreaOfCells(playerCell, 4, 6);
        }

        public void Update()
        {
            KeyboardState state = Keyboard.GetState();
            MouseState mState = Mouse.GetState();

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
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
            prevPlayerCell = playerCell;

            if (mState.LeftButton == ButtonState.Pressed && oldstate.LeftButton == ButtonState.Released)
            {
                PlaceBlock();
            }
            oldstate = mState;

            if (isfalling)
            {
                velocity.Y += 9.82f * 0.075f;
            }

            Vector2 newPos = position.ToVector2() + velocity;

            Move(newPos);
        }

        private void Move(Vector2 newPos)
        {
            position = new Position(newPos);
            camera.MoveTowards(newPos);
        }

        private void PlaceBlock()
        {
            Vector2 position = Cell.SnapToGrid(Mouse.GetState().Position.ToVector2() - new Vector2(camera.view.Translation.X, camera.view.Translation.Y));
            Cell blockCell = Cell.GetCell(position);
            Block.PlaceBlock(blockCell, new Block(PlatformerGame.instance.Pixel(Cell.cellWidth, Cell.cellHeight), Cell.cellWidth, Cell.cellHeight)); // remember to change texture

            if (PlatformerGame.multiplayer)
            {
                Block.PlaceBlock(blockCell, new Block(PlatformerGame.instance.Pixel(Cell.cellWidth, Cell.cellHeight), Cell.cellWidth, Cell.cellHeight));
            }
        }
    }
}
