using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace ProjectPlatformer
{
    public class Camera
    {
        public Vector2 position;
        public Matrix view;

        public Camera()
        {

        }

        public void MoveTowards(Vector2 target)
        {
            position = Vector2.Lerp(position, target, 0.1f);
            view = Matrix.CreateTranslation(new Vector3(PlatformerGame.screenCenter - position, 0f)); // try putting in loadContent method for optimization
        }
    }
}
