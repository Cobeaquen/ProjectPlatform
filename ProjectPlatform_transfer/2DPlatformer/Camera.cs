using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ProjectPlatformer
{
    public class Camera
    {
        public Vector2 position;
        public Matrix view;

        private Vector2 previousPos = Vector2.Zero;
        private Vector2 Target = Vector2.Zero;

        public float minPositionX;
        public float minPositionY;
        public float maxPositionX;
        public float maxPositionY;

        public Camera(Vector2 startPosition, float minX, float maxX, float minY, float maxY)
        {
            position = startPosition;
            Target = position;

            minPositionX = minX;
            maxPositionX = maxX;
            minPositionY = minY;
            maxPositionY = maxY;
        }

        public void MoveTowards(Vector2 target)
        {
            if (position.X < minPositionX)
            {
                position.X = minPositionX;
            }
            else
            {
                position = Vector2.Lerp(position, target, 0.1f);
            }
            HitMapWall();
            SetDisplay(position);
        }
        public void HitMapWall()
        {
            bool hit = false;
            if (position.X < minPositionX)
            {
                hit = true;
                position.X = minPositionX;
            }
            else if (position.X > maxPositionX)
            {
                hit = true;
                position.X = maxPositionX;
            }
            if (position.Y < minPositionY)
            {
                hit = true;
                position.Y = minPositionY;
            }
            else if (position.Y > maxPositionY)
            {
                hit = true;
                position.Y = maxPositionY;
            }
            if (hit && PlatformerGame.settings.EditorMode)
            {
                Target = position;
            }

        }
        public Vector2 PixelToWorldCoords(Vector2 pos)
        {
            return pos - new Vector2(view.Translation.X, view.Translation.Y);
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
        public void MoveAroundFreely()
        {
            KeyboardState state = Keyboard.GetState();

            if (state.IsKeyDown(Keys.A) || state.IsKeyDown(Keys.D) || state.IsKeyDown(Keys.W) || state.IsKeyDown(Keys.S))
            {
                if (state.IsKeyDown(Keys.D))
                {
                    position.X += 15;
                }
                else if (state.IsKeyDown(Keys.A))
                {
                    position.X -= 15;
                }
                if (state.IsKeyDown(Keys.W))
                {
                    position.Y -= 15;
                }
                else if (state.IsKeyDown(Keys.S))
                {
                    position.Y += 15;
                }

                HitMapWall();

                SetDisplay(position);
            }
        }
        public void SetDisplay(Vector2 position)
        {
            view = Matrix.CreateTranslation(new Vector3(PlatformerGame.screenCenter - position, 0f)); // try putting in loadContent method for optimization
        }
    }
}
