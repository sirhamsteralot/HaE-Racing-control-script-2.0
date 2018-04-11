using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public class Stopwatch
        {
            bool started = false;

            long distanceTravelled;
            long ticks;
            double speed;

            Vector3D prevLoc;

            IMyShipController rc;

            public Stopwatch(IMyShipController rc)
            {
                this.rc = rc;

                Reset();
            }

            public void Main()
            {
                if (started)
                {
                    ticks++;

                    speed += rc.GetShipSpeed();

                    distanceTravelled += (long)Math.Round(Vector3D.Distance(prevLoc, rc.GetPosition()));
                    prevLoc = rc.GetPosition();
                }
            }

            int state = 0;
            public void Toggle()
            {
                if (state == 0)
                {
                    Start();
                    state++;
                }
                else if (state == 1)
                {
                    Stop();
                    state++;
                }
                else if (state == 2)
                {
                    Reset();
                    state = 0;
                }
            }

            public void Stop()
            {
                started = false;
            }

            public void Start()
            {
                Reset();
                prevLoc = rc.GetPosition();

                started = true;
            }

            public void Reset()
            {
                ticks = 0;
                speed = 0;
                distanceTravelled = 0;
                started = false;
            }

            public double GetTime()
            {
                return ticks / 60;
            }

            public double GetSpeedAvg()
            {
                return speed / ticks;
            }

            public long GetDistance()
            {
                return distanceTravelled;
            }
        }
    }
}
