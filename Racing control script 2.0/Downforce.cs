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
        public class Downforce
        {
            public List<IMyThrust> spoiler;
            IMyShipController rc;

            public float disabledist = 3;
            public float maxDownforce = 150;
            public float corneringDownforce = 100;
            public float minSpeed = 5;

            public double err;

            public double currentDownforce;

            public Vector3D prevVel;

            Program P;

            public Downforce(IMyShipController rc, Program P, string DownforceName)
            {
                this.P = P;

                spoiler = new List<IMyThrust>();
                IMyBlockGroup tempG = P.GridTerminalSystem.GetBlockGroupWithName(DownforceName);
                tempG?.GetBlocksOfType(spoiler);

                this.rc = rc;
            }

            public void Main(float currentAlt, MyDetectedEntityInfo surfaceType)
            {
                if (rc.GetShipVelocities().LinearVelocity.Length() > minSpeed)
                {
                    SetDownForceBasedVel();
                    SetDownForceBasedCorner();
                    SetDownForceBasedAlt(currentAlt);
                    SetDownForceBasedJump();
                } else
                {
                    SetDownForce(0);
                }

                SetDownForceBasedBreaks();

                ApplyDownForce();
            }

            private void ApplyDownForce()
            {
                foreach (IMyThrust thruster in spoiler)
                {
                    //thruster.SetValueFloat("Override", (float)currentDownforce);
                    thruster.ThrustOverridePercentage = (float)currentDownforce;
                }
            }

            private void SetDownForce(float percent)
            {
                currentDownforce = percent;
            }

            private float GetDownForce()
            {
                float average = 0;

                foreach (IMyThrust thruster in spoiler)
                {
                    //average += thruster.GetValueFloat("Override");
                    average += thruster.ThrustOverridePercentage;
                }

                return average / spoiler.Count();
            }

            private void SetDownForceBasedCorner()
            {
                double right = Vector3D.Dot(rc.MoveIndicator, Vector3D.Right);
                double left = Vector3D.Dot(rc.MoveIndicator, Vector3D.Left);

                if (right > 0 || left > 0)
                {
                    SetDownForce(corneringDownforce);
                }
            }

            private void SetDownForceBasedVel()
            {
                Vector3D currentVelocity = Vector3D.Normalize(rc.GetShipVelocities().LinearVelocity);
                Vector3D shipforward = rc.WorldMatrix.Left; //left cuz  account for 90deg = 0

                // should produce value between 1 and 0 since its two unit vectors
                err = Math.Abs(Vector3D.Dot(currentVelocity, shipforward));

                //err in percent
                err *= maxDownforce;

                //Clamp to degrees
                err = MyMath.Clamp((float)err, 0, 100);

                SetDownForce((float)err);
            }

            private void SetDownForceBasedJump()
            {
                if (prevVel == null)
                {
                    prevVel = rc.GetShipVelocities().LinearVelocity;
                    return;
                }

                Vector3D acceleration = rc.GetShipVelocities().LinearVelocity - prevVel;
                double upwards = Vector3D.Dot(Vector3D.Normalize(acceleration), -Vector3D.Normalize(rc.GetNaturalGravity()));

                if (upwards > GetDownForce())
                    SetDownForce((float)upwards * 100);


                prevVel = rc.GetShipVelocities().LinearVelocity;
            }

            private void SetDownForceBasedAlt(float currentAlt)
            {
                if (currentAlt > disabledist)
                {
                    SetDownForce(0);
                }
            }

            private void SetDownForceBasedBreaks()
            {
                double upwards = Vector3D.Dot(rc.MoveIndicator, Vector3D.Up);

                if (upwards > 0 || rc.HandBrake)
                {
                    SetDownForce(maxDownforce);
                }
            }

            public double GetDownForcePercent()
            {
                return err;
            }
        }
    }
}
