using MICE.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MICE.Components
{
    public class LockedFramerateClock : IClock
    {

        private readonly Stopwatch stopwatch = new Stopwatch();

        public LockedFramerateClock(double desiredFrameRate = 60)
        {
            this.DesiredFrameRate = desiredFrameRate;
            var tickFrequency = Stopwatch.Frequency / 10000;
            this.ApproximateFrameDuration = 1 / this.DesiredFrameRate * tickFrequency;
        }

        public double DesiredFrameRate { get; }

        public void Reset()
        {
            stopwatch.Restart();
        }

        public double ApproximateFrameDuration { get; } = 16666;

        public void Delay()
        {
            double FrameTickDuration = TimeSpan.TicksPerSecond / this.DesiredFrameRate;

            long timer = stopwatch.Elapsed.Ticks;

            if (timer < FrameTickDuration)
            {
                if (timer < FrameTickDuration - TimeSpan.TicksPerMillisecond)
                {
                    Thread.Sleep((int)(FrameTickDuration / TimeSpan.TicksPerMillisecond) - 1);
                }

                // Do some active wait, even though this is bad…
                while (stopwatch.ElapsedTicks < ApproximateFrameDuration)
                {
                    Thread.SpinWait(1000);
                }
            }

            Reset();
        }
    }
}
