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
        public class DynamicCOM
        {
            public INISerializer DynamicCOMSettings = new INISerializer("DynamicCOM Settings");

            public float TuneMultiplier { get { return (float)DynamicCOMSettings.GetValue("TuneMultiplier"); } set { DynamicCOMSettings.SetValue("TuneMultiplier", value); } }
            public float CorneringWeight { get { return (float)DynamicCOMSettings.GetValue("CorneringWeight"); } set { DynamicCOMSettings.SetValue("CorneringWeight", value); } }

            List<IMySpaceBall> spaceBalls = new List<IMySpaceBall>();
            List<IMySpaceBall> leftSpaceBalls = new List<IMySpaceBall>();
            List<IMySpaceBall> rightSpaceBalls = new List<IMySpaceBall>();
            List<IMySpaceBall> frontSpaceBalls = new List<IMySpaceBall>();
            List<IMySpaceBall> rearSpaceBalls = new List<IMySpaceBall>();

            IMyShipController rc;
            Program P;

            public DynamicCOM(IMyShipController rc, Program p)
            {
                this.rc = rc;
                P = p;

                DynamicCOMSettings.AddValue("TuneMultiplier", x => float.Parse(x), 1f);
                DynamicCOMSettings.AddValue("CorneringWeight", x => float.Parse(x), 1000f);

                string temp = P.Me.CustomData;
                DynamicCOMSettings.FirstSerialization(ref temp);
                P.Me.CustomData = temp;

                DynamicCOMSettings.DeSerialize(P.Me.CustomData);

                P.GridTerminalSystem.GetBlocksOfType(spaceBalls);

                Vector3D COM = rc.CenterOfMass;

                //Sort Balls
                foreach (IMySpaceBall SpaceBall in spaceBalls)
                {
                    Vector3D wheelVecFromCOM = SpaceBall.GetPosition() - COM;
                    double forwardDot = Vector3D.Dot(wheelVecFromCOM, rc.WorldMatrix.Forward);
                    double leftDot = Vector3D.Dot(wheelVecFromCOM, rc.WorldMatrix.Left);

                    //Forward/Rear Balls
                    if (forwardDot > 0)
                        frontSpaceBalls.Add(SpaceBall);
                    else
                        rearSpaceBalls.Add(SpaceBall);

                    //Left/Right Balls
                    if (leftDot > 0)
                        leftSpaceBalls.Add(SpaceBall);
                    else
                        rightSpaceBalls.Add(SpaceBall);
                }
            }

            public void SetMassChanges()
            {
                double right = Vector3D.Dot(rc.WorldMatrix.Right, Vector3D.Normalize(rc.GetNaturalGravity()));
                float absRight = (float)Math.Abs(right);
                if (right > 0)          //Left side hill
                {
                    SetMass(leftSpaceBalls, CorneringWeight * TuneMultiplier * absRight);
                    SetMass(rightSpaceBalls, 0);
                }
                else if (right < 0)     //Right side hill
                {
                    SetMass(rightSpaceBalls, CorneringWeight * TuneMultiplier * absRight);
                    SetMass(leftSpaceBalls, 0);
                }
                else
                {
                    SetMass(spaceBalls, 0);
                }
            }

            public void SetMass(List<IMySpaceBall> list, float mass)
            {
                foreach (var ball in list)
                    ball.VirtualMass = mass;
            }
        }
    }
}
