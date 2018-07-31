using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using ProjectPlatformer.Grid;

namespace ProjectPlatformer
{
    public static class Collision
    {
        #region Collision
        public static int Colliding(Vector2 sectionOrigin, int width, int height, Cell[] nearbyCells, ref CollisionType[] collisionTypes, ref Cell[] edgeCells) //make this a function and feed in values to detect collisions for every object in the game. make collision-sections of the player with this function.. START SIMPLE!!!!!
        {
            Vector2 rect1 = new Vector2(sectionOrigin.X - (width / 2f), sectionOrigin.Y - (height / 2f));
            List<Cell> edge = new List<Cell>();
            List<CollisionType> colTypes = new List<CollisionType>();

            int contacts = 0;

            for (int i = 0; i < nearbyCells.Length; i++)
            {
                if (nearbyCells[i].block != null)
                {
                    Vector2 rect2 = new Vector2(nearbyCells[i].x - (Cell.cellWidth / 2f), nearbyCells[i].y - (Cell.cellHeight / 2f));
                    if (rect1.X < rect2.X + Cell.cellWidth &&
                    rect1.X + width > rect2.X &&
                    rect1.Y < rect2.Y + Cell.cellHeight &&
                    height + rect1.Y > rect2.Y)
                    {
                        contacts++;
                        //colTypes.Add(nearbyCells[i].colType);
                        edge.Add(nearbyCells[i]);
                    }
                }
            }
            //collisionTypes = colTypes.Where(c => c != CollisionType.none).ToArray(); //remove?
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
        public static void DebugColliding(Vector2 sectionTopLeft, int width, int height, Color clr)
        {
            int halwid = width / 2;
            int halheight = height / 2;

            int cellR = (int)sectionTopLeft.X + halwid;
            int cellL = (int)sectionTopLeft.X - halwid;
            int cellU = (int)sectionTopLeft.Y - halheight;
            int cellD = (int)sectionTopLeft.Y + halheight;

            PlatformerGame.Instance.DebugLine(new Vector2(cellL, cellU), new Vector2(cellR, cellU), clr, 5);
            PlatformerGame.Instance.DebugLine(new Vector2(cellR, cellU), new Vector2(cellR, cellD), clr, 5);
            PlatformerGame.Instance.DebugLine(new Vector2(cellR, cellD), new Vector2(cellL, cellD), clr, 5);
            PlatformerGame.Instance.DebugLine(new Vector2(cellL, cellD), new Vector2(cellL, cellU), clr, 5);
        }
        #endregion
    }
}
