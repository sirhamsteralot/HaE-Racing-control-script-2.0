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
        public class Lights
        {
            IMyShipController rc;
            List<IMyLightingBlock> frontLights;
            List<IMyLightingBlock> rearLights;
            List<LightStruct> lights;

            bool braking = false;

            Color brakeColor;
            Color normalColor;

            Program P;

            public Lights(IMyShipController rc, Program P, string RearLightGroupName)
            {
                this.P = P;

                //Front lights dont matter right now
                //frontLights = new List<IMyLightingBlock>();
                //P.GridTerminalSystem.GetBlockGroupWithName(P.FRONTLIGHTGROUPNAME).GetBlocksOfType(frontLights);
                rearLights = new List<IMyLightingBlock>();
                P.GridTerminalSystem.GetBlockGroupWithName(RearLightGroupName)?.GetBlocksOfType(rearLights);

                List<IMyLightingBlock> tLights = new List<IMyLightingBlock>();
                lights = new List<LightStruct>();
                P.GridTerminalSystem.GetBlocksOfType(tLights);

                foreach(var light in tLights)
                {
                    LightStruct tLight;
                    tLight.light = light;
                    tLight.defaultRadius = light.Radius;
                    tLight.defaultIntensity = light.Intensity;
                    tLight.defaultFallof = light.Falloff;
                    tLight.defaultBlinkInterval = light.BlinkIntervalSeconds;
                    tLight.defaultBlinkLength = light.BlinkLength;
                    tLight.defaultBlinkOffset = light.BlinkOffset;
                    tLight.defaultColor = light.Color;

                    lights.Add(tLight);
                }

                brakeColor = Color.Red;
                normalColor = Color.DarkRed;

                this.rc = rc;
            }

            public void Main()
            {
                if (!cycle)
                    BrakeLights();
                CycleLights();
            }

            public bool BrakeLights()
            {
                double upwards = Vector3D.Dot(Vector3D.Normalize(rc.MoveIndicator), Vector3D.Up);

                if (!rearLights.Any())
                    return upwards > 0 || rc.HandBrake;

                if (upwards > 0 || rc.HandBrake)
                {

                    foreach (IMyLightingBlock light in rearLights)
                    {
                        light.Color = brakeColor;
                    }

                    braking = true;

                    return true;
                }

                if (braking)
                {
                    foreach (IMyLightingBlock light in rearLights)
                    {
                        light.Color = normalColor;
                    }

                    braking = false;
                }


                return false;
            }

            IEnumerator<bool> loadSpreader;
            bool cycle = false;
            public void SetCycle(bool set)
            {
                cycle = set;
            }

            bool once = false;
            public void CycleLights()
            {
                if (cycle)
                {
                    if (loadSpreader == null)
                    {
                        loadSpreader = Cycle();
                    }
                    else if (!loadSpreader.MoveNext())
                    {
                        loadSpreader.Dispose();

                        loadSpreader = Cycle();
                    }
                    once = false;
                } else if (!once)
                {
                    once = true;
                    BackToDefault();
                }

            }

            double counter = 0;
            IEnumerator<bool> Cycle()
            {
                while (true)
                {
                    counter += MyMath.Clamp((float)rc.GetShipVelocities().LinearVelocity.LengthSquared() / 2000, 0.025f, 0.1f);
                    if (counter >= 1)
                        counter = 0;

                    foreach (var lightStruct in lights)
                    {
                        Color color = HSL2RGB(counter, 0.5, 0.5);

                        lightStruct.light.Color = color;
                    }

                    yield return true;
                }
            }

            public void BackToDefault()
            {
                foreach(var light in lights)
                {
                    light.light.Color = light.defaultColor;
                    light.light.Radius = light.defaultRadius;
                    light.light.Intensity = light.defaultIntensity;
                    light.light.Falloff = light.defaultFallof;
                    light.light.BlinkIntervalSeconds = light.defaultBlinkInterval;
                    light.light.BlinkLength = light.defaultBlinkLength;
                    light.light.BlinkOffset = light.defaultBlinkOffset;
                }
            }

            public static Color HSL2RGB(double h, double sl, double l)
            {
                double v;
                double r, g, b;

                r = l;   // default to gray
                g = l;
                b = l;
                v = (l <= 0.5) ? (l * (1.0 + sl)) : (l + sl - l * sl);
                if (v > 0)
                {
                    double m;
                    double sv;
                    int sextant;
                    double fract, vsf, mid1, mid2;

                    m = l + l - v;
                    sv = (v - m) / v;
                    h *= 6.0;
                    sextant = (int)h;
                    fract = h - sextant;
                    vsf = v * sv * fract;
                    mid1 = m + vsf;
                    mid2 = v - vsf;
                    switch (sextant)
                    {
                        case 0:
                            r = v;
                            g = mid1;
                            b = m;
                            break;
                        case 1:
                            r = mid2;
                            g = v;
                            b = m;
                            break;
                        case 2:
                            r = m;
                            g = v;
                            b = mid1;
                            break;
                        case 3:
                            r = m;
                            g = mid2;
                            b = v;
                            break;
                        case 4:
                            r = mid1;
                            g = m;
                            b = v;
                            break;
                        case 5:
                            r = v;
                            g = m;
                            b = mid2;
                            break;
                    }
                }
                Color rgb = new Color();
                rgb.R = Convert.ToByte(r * 255.0f);
                rgb.G = Convert.ToByte(g * 255.0f);
                rgb.B = Convert.ToByte(b * 255.0f);
                return rgb;
            }

            struct LightStruct
            {
                public IMyLightingBlock light;
                public Color defaultColor;
                public float defaultRadius;
                public float defaultIntensity;
                public float defaultFallof;
                public float defaultBlinkInterval;
                public float defaultBlinkLength;
                public float defaultBlinkOffset;
            }
        }
    }
}
