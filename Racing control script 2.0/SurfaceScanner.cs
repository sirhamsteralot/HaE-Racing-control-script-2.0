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
        public class SurfaceScanner
        {
            float scanDist = 30f;
            float surfaceAngleDist = 5f;

            IMyCameraBlock groundCam;
            IMyShipController rc;
            double groundClearance;

            public SurfaceScanner(IMyCameraBlock groundCam, IMyShipController rc)
            {
                this.rc = rc;
                this.groundCam = groundCam;
                groundCam.EnableRaycast = true;
            }

            public MyDetectedEntityInfo Main()
            {
                MyDetectedEntityInfo tempInfo = new MyDetectedEntityInfo();
                if (groundCam?.CanScan(scanDist) ?? false)
                {
                    tempInfo = groundCam.Raycast(scanDist);
                }

                if (!tempInfo.IsEmpty())
                {
                    groundClearance = Vector3D.Distance(tempInfo.HitPosition.Value, groundCam.GetPosition());
                }

                return tempInfo;
            }

            public Vector3D GetGroundUp()
            {
                MyDetectedEntityInfo left;
                Vector3D leftLoc = groundCam.GetPosition() + (Vector3D.Normalize(rc.WorldMatrix.Left + groundCam.WorldMatrix.Forward * 2) * surfaceAngleDist);

                MyDetectedEntityInfo forward;
                Vector3D forwardLoc = groundCam.GetPosition() + (Vector3D.Normalize(rc.WorldMatrix.Forward + groundCam.WorldMatrix.Forward * 2) * surfaceAngleDist);

                MyDetectedEntityInfo center;

                if (groundCam?.CanScan(surfaceAngleDist * 2) ?? false)
                {
                    center = groundCam.Raycast(surfaceAngleDist);
                    forward = groundCam.Raycast(forwardLoc);
                    left = groundCam.Raycast(leftLoc);
                    

                    if (!left.HitPosition.HasValue || !forward.HitPosition.HasValue || !center.HitPosition.HasValue)
                        return Vector3D.Zero;


                    Vector3D forwardHitDir = forward.HitPosition.Value - center.HitPosition.Value;
                    Vector3D leftHitDir = left.HitPosition.Value - center.HitPosition.Value;
                    Vector3D groundUpDirection = Vector3D.Cross(forwardHitDir, leftHitDir);
                    groundUpDirection.Normalize();

                    if (!groundUpDirection.IsValid())
                        return Vector3D.Zero;

                    return groundUpDirection;
                }

                return Vector3D.Zero;
            }

            public double GetClearance()
            {
                return groundClearance;
            }
        }
    }
}
