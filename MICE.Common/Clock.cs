using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MICE.Common
{
    public class Clock
    {
        private Task clockTask;
        private double rateHz;

        public Clock(double frequencyHz)
        {
            this.FrequencyHz = frequencyHz;
        }

        public double FrequencyHz { get; }
        public bool IsRunning { get; set; }

        public ManualResetEvent Tick { get; set; } = new ManualResetEvent(initialState: false);

        public Task Start()
        {
            this.IsRunning = true;
            this.clockTask = Task.Factory.StartNew(() =>
            {
                var then = DateTime.Now;

                while (this.IsRunning)
                {
                    Tick.Set();
                    this.rateHz++;
                    Tick.Reset();
                }

                var now = DateTime.Now;
                var totalTime = now - then;

                Console.WriteLine($"Ran for {totalTime.TotalMilliseconds}ms and Set/Reset {this.rateHz} times");
            },
            TaskCreationOptions.LongRunning);

            return this.clockTask;
        }
    }
}
