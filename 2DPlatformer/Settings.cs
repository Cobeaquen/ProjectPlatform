﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ProjectPlatformer
{
    public class Settings
    {
        public bool multiplayer { get; set; }

        public int resolutionWidth { get; set; }
        public int resolutionHeight { get; set; }
        public bool isBorderless { get; set; }
        public bool vsync { get; set; }
    }
}
