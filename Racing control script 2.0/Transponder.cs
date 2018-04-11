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
        public class Transponder
        {
            List<IMyShipController> cockpits;
            IMySensorBlock sensor;
            MyDetectedEntityInfo? temp;
            IMyRadioAntenna antenna;
            Program P;

            bool filled = false;

            List<MyDetectedEntityInfo> entities;

            public Transponder(Program P, string PersonSensorName, string AntennaTag)
            {
                this.P = P;

                cockpits = new List<IMyShipController>();
                entities = new List<MyDetectedEntityInfo>();

                P.GridTerminalSystem.GetBlocksOfType(cockpits);
                sensor = P.GridTerminalSystem.GetBlockWithName(PersonSensorName) as IMySensorBlock;


                List<IMyRadioAntenna> tempAntennas = new List<IMyRadioAntenna>();
                P.GridTerminalSystem.GetBlocksOfType(tempAntennas);

                foreach (IMyRadioAntenna anten in tempAntennas)
                {
                    if (anten.CustomName.Contains(AntennaTag))
                    {
                        antenna = anten;
                    }
                }
            }

            public void Main()
            {
                if (antenna != null)
                {
                    temp = GetCharacterInShipControllers();

                    if (filled && temp.HasValue)
                    {
                        antenna.CustomName = "[Transponder] " + temp.Value.Name;
                    }
                    else if (!filled)
                    {
                        antenna.CustomName = "[Transponder] [EMPTY]";
                    }
                }
            }

            // No normal way to get, workaround!
            public MyDetectedEntityInfo? GetCharacterInShipControllers()
            {
                if (cockpitsEmpty(cockpits))
                {

                    sensor.DetectedEntities(entities);
                }
                else if (!cockpitsEmpty(cockpits))
                {
                    List<MyDetectedEntityInfo> temp = new List<MyDetectedEntityInfo>();

                    sensor.DetectedEntities(temp);

                    MyDetectedEntityInfo? lost = GetLostPlayerEntity(entities, temp);

                    filled = true;
                    return lost;
                }

                return null;
            }

            private bool cockpitsEmpty(List<IMyShipController> controllers)
            {
                foreach (IMyShipController controller in controllers)
                {
                    if (controller.IsUnderControl)
                    {
                        return false;
                    }
                }
                filled = false;
                return true;
            }

            private MyDetectedEntityInfo? GetLostPlayerEntity(List<MyDetectedEntityInfo> old, List<MyDetectedEntityInfo> newList)
            {
                List<MyDetectedEntityInfo> tempList = new List<MyDetectedEntityInfo>();
                foreach (MyDetectedEntityInfo tempOld in old)
                {
                    if (!newList.Contains(tempOld))
                    {
                        return tempOld;
                    }
                }
                return null;
            }
        }
    }
}
