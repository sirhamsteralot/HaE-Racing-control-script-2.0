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
        public class Lcd
        {
            List<IMyTextPanel> lcds = new List<IMyTextPanel>();

            string[] Arrows = {
                                "     \n   · \n    \\",       // DOWNRIGHT
                                "     \n   · \n   | ",        // DOWN
                                "     \n   · \n  /  ",        // DOWNLEFT
                                "     \n  -· \n",           // LEFT
                                "  \\  \n   · \n  ",          // UPLEFT
                                "   | \n   · \n  ",           // UP
                                "    /\n   · \n",             // UPRIGHT
                                "     \n   ·-\n  "           // RIGHT
                              };

            public Lcd(Program P)
            {
                P.GridTerminalSystem.GetBlocksOfType(lcds);
            }

            public void SetDirection(string lcdName, string addedText, Vector2D direction)
            {
                double angle = Math.Atan2(direction.Y, direction.X);
                angle *= -1;
                int octant = (int)(Math.Round(8 * angle / (2 * Math.PI) + 8) % 8);

                octant = 7 - octant;

                var sb = new StringBuilder();

                sb.AppendLine(Arrows[octant]);
                sb.AppendLine(addedText);

                WriteTo(lcdName, sb.ToString());
            }

            public void WriteTo(string lcdName, string text, bool colored = false)
            {
                foreach (IMyTextPanel lcd in lcds)
                {
                    if (lcdName == lcd.CustomName)
                    {
                        lcd.WritePublicText(text, false);

                        if (colored)
                        {
                            lcd.SetValue("BackgroundColor", Color.Red);
                        }
                        else
                        {
                            lcd.SetValue("BackgroundColor", Color.Black);
                        }
                    }
                }
            }
        }
    }
}
