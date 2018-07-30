using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectPlatformer.Time
{
    [Serializable]
    public class Timer
    {
        private float time;

        public Timer()
        {
            Reset();
        }

        public void Run()
        {
            time += (float)TimeSpan.FromTicks(1).TotalMilliseconds;
        }
        public void Reset()
        {
            time = 0;
        }
        public void SetTime(float newTime)
        {
            time = newTime;
        }
        public float GetTime()
        {
            return time;
        }
    }
}
