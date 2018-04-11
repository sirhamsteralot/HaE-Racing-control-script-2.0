﻿using Sandbox.Game.EntityComponents;
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
        public static class GyroUtils
        {

            public static void PointInDirection(List<IMyGyro> gyros, IMyShipController reference, Vector3D direction, double multiplier = 1)
            {
                double yaw, pitch;

                Vector3D relativeDirection = Vector3D.TransformNormal(direction, Matrix.Transpose(reference.WorldMatrix));

                DirectionToPitchYaw(reference.WorldMatrix.Forward, reference.WorldMatrix.Left, reference.WorldMatrix.Up, direction, out yaw, out pitch);

                ApplyGyroOverride(gyros, reference, pitch * multiplier, yaw * multiplier, 0);
            }

            public static void PointInDirection(List<IMyGyro> gyros, MatrixD reference, Vector3D direction, double multiplier = 1)
            {
                double yaw, pitch;

                Vector3D relativeDirection = Vector3D.TransformNormal(direction, Matrix.Transpose(reference));

                DirectionToPitchYaw(reference.Forward, reference.Left, reference.Up, direction, out yaw, out pitch);

                ApplyGyroOverride(gyros, reference, pitch * multiplier, yaw * multiplier, 0);
            }

            public static void ApplyGyroOverride(List<IMyGyro> gyros, MatrixD reference, double pitch, double yaw, double roll)
            {
                Vector3D localRotation = new Vector3D(-pitch, yaw, roll);

                Vector3D relativeRotation = Vector3D.TransformNormal(localRotation, reference);

                foreach (IMyGyro gyro in gyros)
                {
                    Vector3D gyroRotation = Vector3D.TransformNormal(relativeRotation, Matrix.Transpose(gyro.WorldMatrix));

                    gyro.Pitch = (float)gyroRotation.X;
                    gyro.Yaw = (float)gyroRotation.Y;
                    gyro.Roll = (float)gyroRotation.Z;

                    gyro.GyroOverride = true;
                }
            }

            public static void ApplyGyroOverride(List<IMyGyro> gyros, IMyShipController reference, double pitch, double yaw, double roll)
            {
                Vector3D localRotation = new Vector3D(-pitch, yaw, roll);

                Vector3D relativeRotation = Vector3D.TransformNormal(localRotation, reference.WorldMatrix);

                foreach (IMyGyro gyro in gyros)
                {
                    Vector3D gyroRotation = Vector3D.TransformNormal(relativeRotation, Matrix.Transpose(gyro.WorldMatrix));

                    gyro.Pitch = (float)gyroRotation.X;
                    gyro.Yaw = (float)gyroRotation.Y;
                    gyro.Roll = (float)gyroRotation.Z;

                    gyro.GyroOverride = true;
                }
            }

            private static void DirectionToPitchYaw(Vector3D forward, Vector3D left, Vector3D Up, Vector3D direction, out double yaw, out double pitch)
            {
                Vector3D projectTargetUp = Project(direction, Up);
                Vector3D projTargetFrontLeft = direction - projectTargetUp;

                yaw = GetAngle(forward, projTargetFrontLeft);
                pitch = GetAngle(direction, projTargetFrontLeft);

                //damnit keen using left hand rule and everything smh
                yaw = -1 * Math.Sign(left.Dot(direction)) * yaw;

                //use the sign bit
                pitch = Math.Sign(Up.Dot(direction)) * pitch;

                //check if the target doesnt pull a 180 on us
                if ((pitch == 0) && (yaw == 0) && (direction.Dot(forward) < 0))
                    yaw = Math.PI;
            }

            private static Vector3D Project(Vector3D one, Vector3D two) //project a on b
            {
                Vector3D projection = one.Dot(two) / two.LengthSquared() * two;
                return projection;
            }

            private static Vector3D Reflect(Vector3D a, Vector3D b, double rejectionFactor = 1) //mirror a over b
            {
                Vector3D project_a = Project(a, b);
                Vector3D reject_a = a - project_a;
                Vector3D reflect_a = project_a - reject_a * rejectionFactor;
                return reflect_a;
            }

            private static double GetAngle(Vector3D One, Vector3D Two) //returns angle in radians
            {
                return Math.Acos(MathHelper.Clamp(One.Dot(Two) / Math.Sqrt(One.LengthSquared() * Two.LengthSquared()), -1, 1));
            }
        }
    }
}
