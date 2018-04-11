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
        public class GyroControl
        {
            public INISerializer GyroSettingINI = new INISerializer("Gyro Settings");

            float negligible { get { return (float)GyroSettingINI.GetValue("negligible"); } set { GyroSettingINI.SetValue("negligible", value); } }
            float lowStabelization { get { return (float)GyroSettingINI.GetValue("lowStabelization"); } set { GyroSettingINI.SetValue("lowStabelization", value); } }
            float highStabelization { get { return (float)GyroSettingINI.GetValue("highStabelization"); } set { GyroSettingINI.SetValue("highStabelization", value); } }

            float upwardStabelizationMultiplier { get { return (float)GyroSettingINI.GetValue("upwardStabelizationMultiplier"); } set { GyroSettingINI.SetValue("upwardStabelizationMultiplier", value); } }
            float pitchUpBias { get { return (float)GyroSettingINI.GetValue("pitchUpBias"); } set { GyroSettingINI.SetValue("pitchUpBias", value); } }

            List<IMyGyro> gyros = new List<IMyGyro>();
            IMyShipController rc;
            Program P;

            public GyroControl(Program P, IMyShipController rc)
            {
                P.GridTerminalSystem.GetBlocksOfType(gyros);

                GyroSettingINI.AddValue("negligible", x => float.Parse(x), 0.1f);
                GyroSettingINI.AddValue("lowStabelization", x => float.Parse(x), 0.1f);
                GyroSettingINI.AddValue("highStabelization", x => float.Parse(x), 1f);
                GyroSettingINI.AddValue("upwardStabelizationMultiplier", x => float.Parse(x), 1f);
                GyroSettingINI.AddValue("pitchUpBias", x => float.Parse(x), 0.15f);

                string temp = P.Me.CustomData;
                GyroSettingINI.FirstSerialization(ref temp);
                P.Me.CustomData = temp;

                GyroSettingINI.DeSerialize(P.Me.CustomData);

                this.rc = rc;
                this.P = P;
            }

            public void Main()
            {
                if (GyroInput())
                {
                    SetOverride(false);
                } 
                else if (LeftRightInput())
                {
                    SetPower(lowStabelization);
                }
                else
                {
                    SetOverride(true);
                    SetPower(highStabelization);
                }

            }

            public void StabelizeUpwards(Vector3D groundUpVector)
            {
                Vector3D upVector = groundUpVector;

                if (groundUpVector == Vector3D.Zero)
                    upVector = Vector3D.Normalize(-rc.GetNaturalGravity());

                upVector += pitchUpBias * rc.WorldMatrix.Backward;

                var refLeft = rc.WorldMatrix.Left;
                var refUp = rc.WorldMatrix.Backward;
                var refForward = rc.WorldMatrix.Up;

                double dotUp = 1 - Vector3D.Dot(upVector, refForward);
                double multiplier = (double)MyMath.Clamp((float)(dotUp * 2 * dotUp), 1, 10);

                MatrixD rotationMatrix = MatrixD.CreateFromDir(refForward, refUp);

                GyroUtils.PointInDirection(gyros, rotationMatrix, upVector, multiplier * upwardStabelizationMultiplier);
            }

            public bool LeftRightInput()
            {
                double right = Vector3D.Dot(rc.MoveIndicator, Vector3D.Right);
                double left = Vector3D.Dot(rc.MoveIndicator, Vector3D.Left);

                if (right > 0 || left > 0)
                    return true;
                else
                    return false;
            }

            public bool GyroInput()
            {
                if (Math.Abs(rc.RollIndicator) > negligible)
                    return true;

                if (rc.RotationIndicator.LengthSquared() > negligible * negligible)
                    return true;

                return false;
            }

            private void SetOverride(bool set)
            {
                foreach (IMyGyro gyro in gyros)
                {
                    gyro.GyroOverride = set;
                }
            }

            private void SetPower(float value)
            {
                foreach (IMyGyro gyro in gyros)
                {
                    gyro.GyroPower = value;
                }
            }
        }
    }
}
