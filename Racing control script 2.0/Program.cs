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
    partial class Program : MyGridProgram
    {


        /*===========|  Global Vars  |===========*/
        public bool MISCINFO = false;
        public bool SCROLLING = true;
        public bool NOSCROLL = false;

        public bool DEBUGMODE = true;

        INISerializer nameSerializer = new INISerializer("BlockNames");

        public string RC_Name { get { return (string)nameSerializer.GetValue("RC_Name"); } set { nameSerializer.SetValue("RC_name", value); } }
        public string LCD_CLOSESTPERSON { get { return (string)nameSerializer.GetValue("LCD_Closestperson"); } set { nameSerializer.SetValue("LCD_Closestperson", value); } }
        public string LCD_SPEDOMETER { get { return (string)nameSerializer.GetValue("LCD_Spedometer"); } set { nameSerializer.SetValue("LCD_Spedometer", value); } }
        public string LCD_MISCINFO { get { return (string)nameSerializer.GetValue("LCD_Miscinfo"); } set { nameSerializer.SetValue("LCD_Miscinfo", value); } }
        public string AIRBAGTAG { get { return (string)nameSerializer.GetValue("AirbagTag"); } set { nameSerializer.SetValue("AirbagTag", value); } }
        public string CAMERANAME { get { return (string)nameSerializer.GetValue("CameraName"); } set { nameSerializer.SetValue("CameraName", value); } }
        public string DOWNFORCENAME { get { return (string)nameSerializer.GetValue("DownforceName"); } set { nameSerializer.SetValue("DownforceName", value); } }
        public string PERSONSENSORNAME { get { return (string)nameSerializer.GetValue("PersonSensorName"); } set { nameSerializer.SetValue("PersonSensorName", value); } }
        public string ANTENNATAG { get { return (string)nameSerializer.GetValue("AntennaTag"); } set { nameSerializer.SetValue("AntennaTag", value); } }
        public string REARLIGHTGROUPNAME { get { return (string)nameSerializer.GetValue("RearlightGroupName"); } set { nameSerializer.SetValue("RearlightGroupName", value); } }

        public Lcd lcd;
        public Sensor sensor;
        public WheelControl wheel;
        public IMyShipController rc;
        public Downforce downforce;
        public Stopwatch stopwatch;
        public Logo logo;
        public Transponder transponder;
        public Lights lights;
        public SurfaceScanner surfaceScanner;
        public GyroControl gyroControl;
        public DynamicCOM dynamicCOM;

        private bool Loaded = false;


        IEnumerator<bool> loadSpreader;

        public IEnumerator<bool> Init()
        {
            rc = GridTerminalSystem.GetBlockWithName(RC_Name) as IMyShipController;
            lcd = new Lcd(this);
            Echo("lcd loaded.");
            Echo("[|..........]");
            yield return true;

            sensor = new Sensor(rc, this);
            Echo("sensor loaded.");
            Echo("[||.........]");
            yield return true;

            wheel = new WheelControl(rc, this);
            Echo("wheel loaded.");
            Echo("[|||........]");
            yield return true;

            downforce = new Downforce(rc, this, DOWNFORCENAME);
            Echo("downforce loaded.");
            Echo("[||||.......]");
            yield return true;

            stopwatch = new Stopwatch(rc);
            Echo("stopwatch loaded.");
            Echo("[|||||......]");
            yield return true;

            logo = new Logo(this);
            Echo("logo loaded.");
            Echo("[||||||.....]");
            yield return true;

            transponder = new Transponder(this, PERSONSENSORNAME, ANTENNATAG);
            Echo("transponder loaded.");
            Echo("[|||||||....]");
            yield return true;

            lights = new Lights(rc, this, REARLIGHTGROUPNAME);
            Echo("lights loaded.");
            Echo("[||||||||...]");
            yield return true;

            gyroControl = new GyroControl(this, rc);
            Echo("gyrocontrol loaded.");
            Echo("[|||||||||..]");
            yield return true;

            IMyCameraBlock cam = GridTerminalSystem.GetBlockWithName(CAMERANAME) as IMyCameraBlock;
            surfaceScanner = new SurfaceScanner(cam, rc);
            Echo("surface scanner loaded");
            Echo("[||||||||||.]");
            yield return true;

            dynamicCOM = new DynamicCOM(rc, this);
            Echo("DynamicCOM loaded");
            Echo("[|||||||||||]");

            Loaded = true;
        }

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;

            nameSerializer.AddValue("RC_Name", x => x, "Remote control");                   //The remote control (make sure its pointed forward in the correct orientation)
            nameSerializer.AddValue("LCD_Closestperson", x => x, "LCD_Closest");            //LCD to show the direction of the closest opponent (within range of the sensors)
            nameSerializer.AddValue("LCD_Spedometer", x => x, "LCD_Speed");                 //LCD to show the current speed
            nameSerializer.AddValue("LCD_Miscinfo", x => x, "LCD_Misc");                    //LCD to show misc info such as current downforce, friction etc. also displays the timer, use the timer by running the PB with "Timer"
            nameSerializer.AddValue("AirbagTag", x => x, "[Airbag]");                       //Tag for suspensionblocks that deploys a wheel when you go too high so you dont crash (over 3 m by default)
            nameSerializer.AddValue("CameraName", x => x, "Cameras");                       //Rename your camera(that is pointed downward from the bottom of your car) to this.
            nameSerializer.AddValue("DownforceName", x => x, "Downforce");                  //The name of the group of downwards pointing thrusters
            nameSerializer.AddValue("PersonSensorName", x => x, "Peopledetector");          //The name of the sensor used to get the player entering/exeting a seat (requires you to be friendly with the sensor)
            nameSerializer.AddValue("AntennaTag", x => x, "[Transponder]");                 //The tag for the antenna (this will display the current driver name)
            nameSerializer.AddValue("RearlightGroupName", x => x, "Back lights");           //The name of the group of rear lights

            if (Me.CustomData == "")
            {
                string temp = Me.CustomData;
                nameSerializer.FirstSerialization(ref temp);
                Me.CustomData = temp;
            } else
            {
                nameSerializer.DeSerialize(Me.CustomData);
            }
            


            loadSpreader = Init();
        }

        public void SubMain(string argument, UpdateType uType)
        {
            if (Runtime.TimeSinceLastRun.Milliseconds < 10 && uType != UpdateType.Antenna && uType != UpdateType.Terminal && uType != UpdateType.Trigger && uType != UpdateType.Mod)
                return;

            if (!Loaded)
            {
                if (loadSpreader.MoveNext())
                    return;
                return;
            }


            /*============|  Argument  |=============*/
            switch (argument)
            {
                case "Timer":
                    stopwatch.Toggle();
                    break;
                case "SnowMode":
                    wheel.ToggleSnowMode();
                    break;
                case "Parteh":
                    lights.SetCycle(true);
                    break;
                case "NoParteh":
                    lights.SetCycle(false);
                    break;
            }

            /*=====|  Each time the pb is run  |=====*/
            stopwatch.Main();
            RunEnumerator();
            logo.DoTick();
            gyroControl.Main();
        }

        /*=========|  HelperFunctions  |=========*/
        int ticks;
        public void RunEnumerator()
        {
            ticks++;
            //if (ticks % 2 != 0)
            //    return;

            if (loadSpreader == null)
            {
                loadSpreader = SpreadLoad();
            }
            else if (!loadSpreader.MoveNext())
            {
                loadSpreader.Dispose();

                loadSpreader = SpreadLoad();
            }

            LcdWriter();
        }
        
        public IEnumerator<bool> SpreadLoad()
        {
            lights.Main();
            yield return true;
            MyDetectedEntityInfo surface = surfaceScanner.Main();
            yield return true;
            Vector3D groundUp = surfaceScanner.GetGroundUp();
            yield return true;
            wheel.Main(surface, surfaceScanner.GetClearance(), groundUp);
            yield return true;
            gyroControl.StabelizeUpwards(groundUp);
            yield return true;
            downforce.Main((float)surfaceScanner.GetClearance(), surface);
            yield return true;
            transponder.Main();
            yield return true;
            dynamicCOM.SetMassChanges();
        }

        int lcdCounter = 0;
        public void LcdWriter()
        {
            if (ticks % 31 == 0)
                lcdCounter++;
            if (lcdCounter > 6)
                lcdCounter = 0;

            /*===========|  LCD's  |===========*/
            if (sensor.Main())
            {
                lcd.SetDirection(LCD_CLOSESTPERSON, " " + sensor.DistanceToClosest.ToString(), sensor.GetVector2());

                if (MISCINFO)
                {
                    //misc entity info
                    MyDetectedEntityInfo temp = sensor.closest;
                    var sb = new StringBuilder();
                    if (SCROLLING)
                    {
                        sb.AppendLine("Closest info:");
                        sb.AppendLine("Name: " + temp.Name);
                        sb.AppendLine("Speed: " + Math.Round(temp.Velocity.Length() * 2.23693629, 2) + " mph");
                        sb.AppendLine("Size: " + Math.Round(temp.BoundingBox.Size.Length() * 3.2808399, 2) + "ft");
                    }
                    else if (NOSCROLL)
                    {
                        sb.AppendLine("Closest info:");
                        sb.AppendLine("Name: " + temp.Name);
                        sb.AppendLine("Speed: " + Math.Round(temp.Velocity.Length() * 3.6, 2) + " km/h");
                        sb.AppendLine("Size: " + Math.Round(temp.BoundingBox.Size.Length(), 2) + "m");
                    }
                    else
                    {
                        sb.AppendLine("Closest info:");
                        sb.AppendLine("Name: " + temp.Name);
                        sb.AppendLine("Speed: " + Math.Round(temp.Velocity.Length(), 2) + " m/s");
                        sb.AppendLine("Size: " + Math.Round(temp.BoundingBox.Size.Length(), 2) + "m");
                    }


                    lcd.WriteTo(LCD_MISCINFO, sb.ToString());
                }
            }

            if (!MISCINFO)
            {
                var sb = new StringBuilder();
                sb.AppendLine("Craft info:");
                if (SCROLLING)
                {
                    if (lcdCounter == 0)
                        sb.AppendLine("Friction: " + Math.Round(wheel.GetCurrentFriction()).ToString() + "%");
                    if (lcdCounter == 1)
                        sb.AppendLine("Downforce: " + Math.Round(downforce.GetDownForcePercent(), 2).ToString() + "%");
                    if (lcdCounter == 2)
                        sb.AppendLine("Stopwatch:");
                    if (lcdCounter == 3)
                        sb.AppendLine("Time: " + stopwatch.GetTime() + "s");
                    if (lcdCounter == 4)
                        sb.AppendLine("Avg speed: " + Math.Round(stopwatch.GetSpeedAvg() * 3.6, 1) + "km/h");
                    if (lcdCounter == 5)
                        sb.AppendLine("Travelled distance: " + Math.Round(stopwatch.GetDistance() / 1000.0, 3) + "km");
                }
                else if (NOSCROLL)
                {
                    sb.AppendLine("Craft info:");
                    sb.AppendLine("Friction: " + Math.Round(wheel.GetCurrentFriction()).ToString() + "%");
                    sb.AppendLine("Downforce: " + Math.Round(downforce.GetDownForcePercent(), 2).ToString() + "%");
                    sb.AppendLine("Stopwatch:");
                    sb.AppendLine("Time: " + stopwatch.GetTime() + "s");
                    sb.AppendLine("Avg speed: " + Math.Round(stopwatch.GetSpeedAvg() * 3.6, 1) + "km/h");
                    sb.AppendLine("Travelled distance: " + Math.Round(stopwatch.GetDistance() / 1000.0, 3) + "km");
                }
                else
                {
                    sb.AppendLine("Craft info:");
                    sb.AppendLine("Friction: " + Math.Round(wheel.GetCurrentFriction()).ToString() + "%");
                    sb.AppendLine("Downforce: " + Math.Round(downforce.GetDownForcePercent(), 2).ToString() + "%");
                    sb.AppendLine("Stopwatch");
                    sb.AppendLine("Time: " + stopwatch.GetTime() + "s");
                    sb.AppendLine("Avg speed: " + Math.Round(stopwatch.GetSpeedAvg(), 2) + "m/s");
                    sb.AppendLine("Travelled distance: " + stopwatch.GetDistance() + "m");
                }

                if (lcdCounter == 6 || NOSCROLL)
                    sb.AppendLine("SnowMode: " + wheel.snowMode.ToString());

                lcd.WriteTo(LCD_MISCINFO, sb.ToString());
            }


            lcd.WriteTo(LCD_SPEDOMETER, Math.Round(rc.GetShipSpeed() * 3.6, 2).ToString() + "\nkm/h");
        }

        void Main(string argument, UpdateType uType)
        { //By inflex
            if (DEBUGMODE)
            {
                try
                {
                    SubMain(argument, uType);
                }
                catch (Exception e)
                {
                    var sb = new StringBuilder();

                    sb.AppendLine("Exception Message:");
                    sb.AppendLine($"   {e.Message}");
                    sb.AppendLine();

                    sb.AppendLine("Stack trace:");
                    sb.AppendLine(e.StackTrace);
                    sb.AppendLine();

                    var exceptionDump = sb.ToString();

                    Echo(exceptionDump);

                    //Optionally rethrow
                    throw;
                }
            } else
            {
                SubMain(argument, uType);
            }
        }
    }
}