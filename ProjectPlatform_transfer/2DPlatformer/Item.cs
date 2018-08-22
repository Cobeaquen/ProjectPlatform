using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectPlatformer.Items
{
    public class Item
    {
        public Items item;

        protected Item()
        {

        }
    }

    public enum Items
    {
        Katana,
        LaserPistol
    }
}