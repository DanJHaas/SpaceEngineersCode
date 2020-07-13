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
        IMyRadioAntenna antenna;
        IMyShipController controller;
        IMyProgrammableBlock pb;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            antenna = GridTerminalSystem.GetBlockWithName("send") as IMyRadioAntenna;
            controller = GridTerminalSystem.GetBlockWithName("controller") as IMyShipController;


        }

        public void Main(string argument, UpdateType updateType)
        {
            string tag1 = "pack";
            string tag2 = "pack2";
            string messageOut1 = antenna.GetPosition().ToString();
            string messageOut2 = controller.GetShipVelocities().LinearVelocity.ToString();
            Echo(messageOut2);
            IGC.SendBroadcastMessage(tag1, messageOut1, TransmissionDistance.TransmissionDistanceMax);
            IGC.RegisterBroadcastListener(tag1);
            IGC.SendBroadcastMessage(tag2, messageOut2, TransmissionDistance.TransmissionDistanceMax);
            IGC.RegisterBroadcastListener(tag2);

        }
    }
}