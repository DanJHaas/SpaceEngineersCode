using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;

using VRageMath;
using VRage.Game;
using VRage.Collections;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;

namespace SpaceEngineers.UWBlockPrograms.BatteryMonitor
{
    public sealed class Program : MyGridProgram
    {
        IMyCameraBlock Cam;
        IMyTextPanel lcd;
        string[] splitco;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update1;
            Cam = GridTerminalSystem.GetBlockWithName("cam") as IMyCameraBlock;
            lcd = GridTerminalSystem.GetBlockWithName("lcd") as IMyTextPanel;
        }
        MyDetectedEntityInfo info;
        public void Main(string argument, UpdateType updateType)
        {
            if (!Cam.EnableRaycast){
                Cam.EnableRaycast = true;
            }
            info = Cam.Raycast(30);
            if (!info.IsEmpty()){
                splitco = info.Position.ToString().Split(' ');
                lcd.WritePublicText(splitco[0]+ "\n"+splitco[1]+"\n"+splitco[2]);
            }
        }
    }
}