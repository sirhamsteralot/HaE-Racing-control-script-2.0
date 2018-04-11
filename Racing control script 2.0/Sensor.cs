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
        public class Sensor
        {
            List<IMySensorBlock> sensors = new List<IMySensorBlock>();
            IMyShipController rc;

            public double DistanceToClosest;
            public MyDetectedEntityInfo closest;
            Program P;

            public Sensor(IMyShipController rc, Program P)
            {
                this.P = P;

                P.GridTerminalSystem.GetBlocksOfType(sensors);
                this.rc = rc;
            }

            public bool Main()
            {
                var tmpList = new List<MyDetectedEntityInfo>();
                foreach (IMySensorBlock sensor in sensors)
                {
                    if (sensor.IsActive)
                    {
                        tmpList.Add(sensor.LastDetectedEntity);
                    }
                }

                double distance = 0;

                foreach (MyDetectedEntityInfo info in tmpList)
                {
                    double tmpDist = Vector3D.Distance(rc.GetPosition(), info.Position);

                    if (tmpDist <= distance || distance == 0)
                    {
                        closest = info;
                        distance = tmpDist;
                        DistanceToClosest = Math.Round(tmpDist, 2);
                    }
                }

                if (distance != 0)
                    return true;

                return false;
            }

            public Vector2D GetVector2()
            {
                Vector3D direction = Vector3D.Normalize(closest.Position - rc.GetPosition());

                Vector3D local = Vector3D.TransformNormal(direction, MatrixD.Invert(rc.WorldMatrix));

                return new Vector2D(local.X, local.Z);
            }

            private void InitSensor()
            {
                foreach (IMySensorBlock block in sensors)
                {
                    block.LeftExtend = block.MaxRange;
                    block.RightExtend = block.MaxRange;
                    block.TopExtend = block.MaxRange;
                    block.BottomExtend = block.MaxRange;
                    block.FrontExtend = block.MaxRange;
                    block.BackExtend = block.MaxRange;

                    block.PlayProximitySound = false;
                    block.DetectPlayers = false;
                    block.DetectFloatingObjects = false;
                    block.DetectLargeShips = true;
                    block.DetectSmallShips = true;
                    block.DetectStations = false;
                    block.DetectSubgrids = false;
                    block.DetectAsteroids = false;
                    block.DetectOwner = false;
                    block.DetectFriendly = true;
                    block.DetectNeutral = true;
                    block.DetectEnemy = true;
                }
            }
        }
    }
}
