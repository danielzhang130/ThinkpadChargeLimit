using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ThinkpadChargeLimit
{
    public class ThinkTimer
    {
        private System.Timers.Timer timer;

        public virtual event ElapsedEventHandler Elapsed;

        public virtual void Start(float millis)
        {
            timer = new System.Timers.Timer(millis);
            timer.Elapsed += InternalElapsed;
            timer.Start();
        }

        private void InternalElapsed(object sender, ElapsedEventArgs e)
        {
            Stop();
            Elapsed(sender, e);
        }

        internal void Stop()
        {
            timer.Stop();
            timer.Dispose();
        }
    }
}
