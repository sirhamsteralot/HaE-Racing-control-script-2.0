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
        public class WheelControl
        {
            public INISerializer wheelSettingINI = new INISerializer("Suspension Settings");

            //Suspension friction parameters
            public float minFriction { get { return (float)wheelSettingINI.GetValue("minFriction"); } set { wheelSettingINI.SetValue("minFriction", value); } }
            public float maxFriction { get { return (float)wheelSettingINI.GetValue("maxFriction"); } set { wheelSettingINI.SetValue("maxFriction", value); } }
            public float onRoadMultiplier { get { return (float)wheelSettingINI.GetValue("onRoadMultiplier"); } set { wheelSettingINI.SetValue("onRoadMultiplier", value); } }
            public float offRoadMultiplier { get { return (float)wheelSettingINI.GetValue("offRoadMultiplier"); } set { wheelSettingINI.SetValue("offRoadMultiplier", value); } }
            public float snowFriction { get { return (float)wheelSettingINI.GetValue("snowFriction"); } set { wheelSettingINI.SetValue("snowFriction", value); } }
            public float brakeMultiplier { get { return (float)wheelSettingINI.GetValue("brakeMultiplier"); } set { wheelSettingINI.SetValue("brakeMultiplier", value); } }
            public float speedSignificanceMultiplier { get { return (float)wheelSettingINI.GetValue("speedSignificanceMultiplier"); } set { wheelSettingINI.SetValue("speedSignificanceMultiplier", value); } }
            public float antiFlipMultiplier { get { return (float)wheelSettingINI.GetValue("antiFlipMultiplier"); } set { wheelSettingINI.SetValue("antiFlipMultiplier", value); } }
            public float antiFlipDotTreshhold { get { return (float)wheelSettingINI.GetValue("antiFlipDotTreshhold"); } set { wheelSettingINI.SetValue("antiFlipDotTreshhold", value); } }

            //Suspension Clearance parameters
            public float maxSpeed { get { return (float)wheelSettingINI.GetValue("maxSpeed"); } set { wheelSettingINI.SetValue("maxSpeed", value); } }
            public float MaxOffset { get { return (float)wheelSettingINI.GetValue("MaxOffset"); } set { wheelSettingINI.SetValue("MaxOffset", value); } }
            public float OffroadOffset { get { return (float)wheelSettingINI.GetValue("OffroadOffset"); } set { wheelSettingINI.SetValue("OffroadOffset", value); } }
            public float NormalOffset { get { return (float)wheelSettingINI.GetValue("NormalOffset"); } set { wheelSettingINI.SetValue("NormalOffset", value); } }
            public float MinClearance { get { return (float)wheelSettingINI.GetValue("MinClearance"); } set { wheelSettingINI.SetValue("MinClearance", value); } }


            //Suspension Steering parameters
            public float RoughTerrainSteering { get { return (float)wheelSettingINI.GetValue("RoughTerrainSteering"); } set { wheelSettingINI.SetValue("RoughTerrainSteering", value); } }
            public float PavedTerrainSteering { get { return (float)wheelSettingINI.GetValue("PavedTerrainSteering"); } set { wheelSettingINI.SetValue("PavedTerrainSteering", value); } }
            public float TuningMultiplier { get { return (float)wheelSettingINI.GetValue("TuningMultiplier"); } set { wheelSettingINI.SetValue("TuningMultiplier", value); } }
            public float MaxSteerMultiplier { get { return (float)wheelSettingINI.GetValue("MaxSteerMultiplier"); } set { wheelSettingINI.SetValue("MaxSteerMultiplier", value); } }

            //Suspension Airtime Parameters
            double deployDist { get { return (float)wheelSettingINI.GetValue("deployDist"); } set { wheelSettingINI.SetValue("deployDist", value); } }
            int cooldown { get { return (int)wheelSettingINI.GetValue("cooldown"); } set { wheelSettingINI.SetValue("cooldown", value); } }

            //Suspension Torque vectoring Parameters
            float RawPower { get { return (float)wheelSettingINI.GetValue("RawPower"); } set { wheelSettingINI.SetValue("RawPower", value); } }
            float turnMultiplier { get { return (float)wheelSettingINI.GetValue("turnMultiplier"); } set { wheelSettingINI.SetValue("turnMultiplier", value); } }
            float onroadFrontWheelMultiplier { get { return (float)wheelSettingINI.GetValue("onroadFrontWheelMultiplier"); } set { wheelSettingINI.SetValue("onroadFrontWheelMultiplier", value); } }
            float onroadRearWheelMultiplier { get { return (float)wheelSettingINI.GetValue("onroadRearWheelMultiplier"); } set { wheelSettingINI.SetValue("onroadRearWheelMultiplier", value); } }
            float offroadFrontWheelMultiplier { get { return (float)wheelSettingINI.GetValue("offroadFrontWheelMultiplier"); } set { wheelSettingINI.SetValue("offroadFrontWheelMultiplier", value); } }
            float offroadRearWheelMultiplier { get { return (float)wheelSettingINI.GetValue("offroadRearWheelMultiplier"); } set { wheelSettingINI.SetValue("offroadRearWheelMultiplier", value); } }

            //Suspension Stiffness parameters
            public float DefaultStiffness { get { return (float)wheelSettingINI.GetValue("DefaultStiffness"); } set { wheelSettingINI.SetValue("DefaultStiffness", value); } }
            public float AntiRollMultiplier { get { return (float)wheelSettingINI.GetValue("AntiRollMultiplier"); } set { wheelSettingINI.SetValue("AntiRollMultiplier", value); } }

            //Suspension Speed Settings
            public bool WheelRotationLimiter { get { return (bool)wheelSettingINI.GetValue("WheelRotationLimiter"); } set { wheelSettingINI.SetValue("WheelRotationLimiter", value); } }
            public float WheelRotationLimiterBuffer { get { return (float)wheelSettingINI.GetValue("WheelRotationLimiterBuffer"); } set { wheelSettingINI.SetValue("WheelRotationLimiterBuffer", value); } }

            int counter = 0;

            float percent;
            float currentOffset;

            public bool snowMode = false;

            List<IMyMotorSuspension> wheels = new List<IMyMotorSuspension>();
            List<IMyMotorSuspension> frontWheels = new List<IMyMotorSuspension>();
            List<IMyMotorSuspension> rearWheels = new List<IMyMotorSuspension>();
            List<IMyMotorSuspension> leftWheels = new List<IMyMotorSuspension>();
            List<IMyMotorSuspension> rightWheels = new List<IMyMotorSuspension>();

            IMyShipController rc;
            Program P;

            public WheelControl(IMyShipController rc, Program P)
            {
                this.P = P;

                P.GridTerminalSystem.GetBlocksOfType(wheels, x => x.Enabled);
                this.rc = rc;

                /*==========| INI Settings |==========*/
                //Friction Settings
                wheelSettingINI.AddValue("minFriction", x => float.Parse(x), 90f);
                wheelSettingINI.AddValue("maxFriction", x => float.Parse(x), 100f);
                wheelSettingINI.AddValue("onRoadMultiplier", x => float.Parse(x), 1f);
                wheelSettingINI.AddValue("offRoadMultiplier", x => float.Parse(x), 1f);
                wheelSettingINI.AddValue("snowFriction", x => float.Parse(x), 100f);
                wheelSettingINI.AddValue("brakeMultiplier", x => float.Parse(x), 0.5f);
                wheelSettingINI.AddValue("speedSignificanceMultiplier", x => float.Parse(x), 1.5f);
                wheelSettingINI.AddValue("antiFlipMultiplier", x => float.Parse(x), 0f);
                wheelSettingINI.AddValue("antiFlipDotTreshhold", x => float.Parse(x), 0.05f);

                //Suspension Clearance Settings
                wheelSettingINI.AddValue("maxSpeed", x => float.Parse(x), 90f);
                wheelSettingINI.AddValue("MaxOffset", x => float.Parse(x), -1f);
                wheelSettingINI.AddValue("OffroadOffset", x => float.Parse(x), -0.15f);
                wheelSettingINI.AddValue("NormalOffset", x => float.Parse(x), -0.15f);
                wheelSettingINI.AddValue("MinClearance", x => float.Parse(x), 0.05f);

                //Suspension Steering Settings
                wheelSettingINI.AddValue("RoughTerrainSteering", x => float.Parse(x), 7f);
                wheelSettingINI.AddValue("PavedTerrainSteering", x => float.Parse(x), 7f);
                wheelSettingINI.AddValue("TuningMultiplier", x => float.Parse(x), 1f);
                wheelSettingINI.AddValue("MaxSteerMultiplier", x => float.Parse(x), 2f);

                //Suspension Airtime Settings
                wheelSettingINI.AddValue("deployDist", x => float.Parse(x), 3f);
                wheelSettingINI.AddValue("cooldown", x => int.Parse(x), 10);

                //Suspension Torque vectoring Settings
                wheelSettingINI.AddValue("RawPower", x => float.Parse(x), 100f);
                wheelSettingINI.AddValue("turnMultiplier", x => float.Parse(x), 0.9f);
                wheelSettingINI.AddValue("onroadFrontWheelMultiplier", x => float.Parse(x), 0.8f);
                wheelSettingINI.AddValue("onroadRearWheelMultiplier", x => float.Parse(x), 1f);
                wheelSettingINI.AddValue("offroadFrontWheelMultiplier", x => float.Parse(x), 1f);
                wheelSettingINI.AddValue("offroadRearWheelMultiplier", x => float.Parse(x), 1f);

                //Suspension Stiffness Settings
                wheelSettingINI.AddValue("DefaultStiffness", x => float.Parse(x), 25f);
                wheelSettingINI.AddValue("AntiRollMultiplier", x => float.Parse(x), 1.25f);

                //Suspension Speed Settings
                wheelSettingINI.AddValue("WheelRotationLimiter", x => bool.Parse(x), true);
                wheelSettingINI.AddValue("WheelRotationLimiterBuffer", x => float.Parse(x), 25f);

                string temp = P.Me.CustomData;
                wheelSettingINI.FirstSerialization(ref temp);
                P.Me.CustomData = temp;

                wheelSettingINI.DeSerialize(P.Me.CustomData);

                Vector3D COM = rc.CenterOfMass;

                //Sort wheels
                foreach (IMyMotorSuspension wheel in wheels)
                {
                    Vector3D wheelVecFromCOM = wheel.GetPosition() - COM;
                    double forwardDot = Vector3D.Dot(wheelVecFromCOM, rc.WorldMatrix.Forward);
                    double leftDot = Vector3D.Dot(wheelVecFromCOM, rc.WorldMatrix.Left);

                    //Forward/Rear wheels
                    if (forwardDot > 0)
                        frontWheels.Add(wheel);
                    else
                        rearWheels.Add(wheel);

                    //Left/Right wheels
                    if (leftDot > 0)
                        leftWheels.Add(wheel);
                    else
                        rightWheels.Add(wheel);
                }

            }

            public void Main(MyDetectedEntityInfo surfaceType, double groundClearance, Vector3D groundUpDir)
            {
                ReAttach();

                Surface(surfaceType, groundUpDir);

                AirtimeOffset(groundClearance);
                KeepMinClearance(groundClearance);
                SetOffsets();
                TorqueVectoring();
                WheelRotationSpeedLimiter();
            }

            public void ToggleSnowMode()
            {
                snowMode = !snowMode;
            }

            public void SnowMode()
            {
                SetFriction(wheels, snowFriction);
            }

            public void AirtimeOffset(double groundClearance)
            {
                if (groundClearance > deployDist)
                    counter = 0;

                if (counter <= cooldown)
                    SetMaxOffset();

                counter++;
            }

            private void WheelRotationSpeedLimiter()
            {
                if (!WheelRotationLimiter)
                    return;

                float maxWheelSpeed = (float)rc.GetShipSpeed() + WheelRotationLimiterBuffer;
                maxWheelSpeed *= 3.6f;

                foreach (var wheel in wheels)
                    wheel.SetValueFloat("Speed Limit", maxWheelSpeed);
            }

            private void TorqueVectoring()
            {
                double right = Vector3D.Dot(rc.MoveIndicator, Vector3D.Right);

                if (right < 0)          //Going left
                {
                    SetPowerMultiplier(leftWheels, turnMultiplier);
                    SetStrengthMultiplier(rightWheels, AntiRollMultiplier);

                }
                else if (right > 0)     //Going right
                {
                    SetPowerMultiplier(rightWheels, turnMultiplier);
                    SetStrengthMultiplier(leftWheels, AntiRollMultiplier);
                } else
                {
                    SetStrengthMultiplier(wheels, 1f);
                }
            }

            private void ReAttach()
            {
                foreach (IMyMotorSuspension wheel in wheels)
                {
                    if (!wheel.IsAttached && wheel.Enabled)
                    {
                        wheel.ApplyAction("Add Top Part");
                    }
                }
            }

            private void Surface(MyDetectedEntityInfo surfaceType, Vector3D groundUpDir)
            {
                float steerAngle;

                if (surfaceType.Type == MyDetectedEntityType.LargeGrid || surfaceType.Type == MyDetectedEntityType.SmallGrid)
                {
                    steerAngle = PavedTerrainSteering;
                    SetNormalOffset();
                    Friction(onRoadMultiplier);
                    SetOnroadPowersetting();
                    Braking();
                }
                else
                {
                    SetOffroadOffset();
                    steerAngle = RoughTerrainSteering;

                    if (!snowMode)
                        Friction(offRoadMultiplier);
                    else
                        SnowMode();
                    SetOffroadPowersetting();
                }

                if (groundUpDir != Vector3D.Zero)
                {
                    double upDot = Vector3D.Dot(rc.WorldMatrix.Up, groundUpDir);
                    double upDotRange = 1 - antiFlipDotTreshhold;

                    if (upDot < upDotRange)
                    {
                        percent = GetCurrentFriction() * antiFlipMultiplier;

                        ApplyFriction();
                    }
                }



                steerAngle *= (float)Math.Min(1 / ((rc.GetShipSpeed() / 100) * TuningMultiplier), MaxSteerMultiplier);

                foreach (IMyMotorSuspension wheel in wheels)
                {
                    if (wheel.Steering)
                    {
                        wheel.SetValueFloat("MaxSteerAngle", steerAngle);
                    }
                }
            }

            private double DegreeToRadian(double angle)
            {
                return Math.PI * angle / 180.0;
            }

            private void SetOnroadPowersetting()
            {
                SetPower(frontWheels, RawPower * onroadFrontWheelMultiplier);
                SetPower(rearWheels, RawPower * onroadRearWheelMultiplier);
            }

            private void SetOffroadPowersetting()
            {
                SetPower(frontWheels, RawPower * offroadFrontWheelMultiplier);
                SetPower(rearWheels, RawPower * offroadRearWheelMultiplier);
            }

            private void Friction(float multiplier)
            {

                double currentVelocity = rc.GetShipVelocities().LinearVelocity.Length();

                percent = (float)currentVelocity * speedSignificanceMultiplier / maxSpeed * maxFriction * multiplier;

                percent = MyMath.Clamp(percent, minFriction, maxFriction);

                ApplyFriction();
            }

            private void Braking()
            {
                double upwards = Vector3D.Dot(rc.MoveIndicator, Vector3D.Up);

                if (upwards > 0 || rc.HandBrake)
                {
                    float temp = GetCurrentFriction() * brakeMultiplier;

                    SetFriction(frontWheels, temp);
                }
            }

            private void SetPower(List<IMyMotorSuspension> wheelList, float percentage)
            {

                foreach (IMyMotorSuspension wheel in wheelList)
                {
                    wheel.SetValueFloat("Power", percentage);
                }
            }
            private void SetPowerMultiplier(List<IMyMotorSuspension> wheelList, float Multiplier)
            {
                foreach (IMyMotorSuspension wheel in wheelList)
                {
                    wheel.SetValueFloat("Power", GetPower(wheel) * Multiplier);
                }
            }

            private void SetStrengthMultiplier(List<IMyMotorSuspension> wheelList, float Multiplier)
            {
                foreach (IMyMotorSuspension wheel in wheelList)
                {
                    wheel.SetValueFloat("Strength", DefaultStiffness * Multiplier);
                }
            }

            private float GetPower(IMyMotorSuspension wheel)
            {
                return wheel.Power;
            }

            private void SetFriction(List<IMyMotorSuspension> wheelList, float percentage)
            {
                foreach (IMyMotorSuspension wheel in wheelList)
                {
                    wheel.SetValueFloat("Friction", percentage);
                }
            }

            private void ApplyFriction()
            {
                foreach (IMyMotorSuspension wheel in wheels)
                {
                    wheel.SetValueFloat("Friction", percent);
                }
            }

            public float GetCurrentFriction()
            {
                return percent;
            }

            public void SetMaxOffset()
            {
                currentOffset = MaxOffset;
            }

            public void SetOffroadOffset()
            {
                currentOffset = OffroadOffset;
            }


            public void SetNormalOffset()
            {
                currentOffset = NormalOffset;
            }

            public void KeepMinClearance(double currentClearance)
            {
                if (currentClearance < MinClearance)
                {
                    currentOffset += -0.10f;
                }
            }

            public void SetOffsets()
            {
                foreach (IMyMotorSuspension wheel in wheels)
                {
                    wheel.SetValueFloat("Height", currentOffset);
                }
            }
        }
    }
}
