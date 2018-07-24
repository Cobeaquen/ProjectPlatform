using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectPlatformer
{
    public class Timer
    {
        private double time;

        public Timer()
        {
            time = 0;
        }

        public void Run()
        {
            time += TimeSpan.FromTicks(1000).TotalMilliseconds;
        }
        public void Reset()
        {
            time = 0;
        }
        public void SetTime(double newTime)
        {
            time = newTime;
        }
        public double GetTime()
        {
            return time;
        }
    }
}
