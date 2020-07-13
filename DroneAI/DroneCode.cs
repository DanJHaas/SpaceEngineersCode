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
        List<IMyBroadcastListener> listeners = new List<IMyBroadcastListener>();
        List<IMyTerminalBlock> term = new List<IMyTerminalBlock>();
        List<IMyThrust> onGridThrust = new List<IMyThrust>();
        MyIGCMessage packet = new MyIGCMessage();
        IMyTextPanel info;
        IMyRemoteControl brain;
        IMyRadioAntenna antenna;
        IMyGyro gyro;
        Vector3D targetPos;
        Vector3D homebase;
        Vector3D homebasevelocity;
        
        string sentcord = "";
        string sentvelocity = "";
        double[] coord = { 0.0, 0.0, 0.0 };
        double[] velocityout = { 0.0, 0.0, 0.0 };

        public Program()
        {
            //testing git unga
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            info = GridTerminalSystem.GetBlockWithName("Lcd") as IMyTextPanel;
            brain = GridTerminalSystem.GetBlockWithName("controller") as IMyRemoteControl;
            GridTerminalSystem.GetBlocksOfType<IMyThrust>(term);
            antenna = GridTerminalSystem.GetBlockGroupWithName("receiver") as IMyRadioAntenna;
            gyro = GridTerminalSystem.GetBlockWithName("gyro") as IMyGyro;

            IGC.RegisterBroadcastListener("pack");
            IGC.RegisterBroadcastListener("pack2");
            IGC.GetBroadcastListeners(listeners);

        }
        public void Main(string argument, UpdateType updateType)
        {
            Vector3D homebase = new Vector3D(coord[0], coord[1], coord[2]);
            Vector3D homebasevelocity = new Vector3D(velocityout[0], velocityout[1], velocityout[2]);
            MatrixD m = brain.WorldMatrix;
            Matrix test;

            

            switch (argument.ToLower())
            {
                case "auto":
                    brain.SetAutoPilotEnabled(true);
                    brain.SetCollisionAvoidance(true);
                    brain.DampenersOverride = true;
                    Echo("Auto Pilot Enabled.");
                    break;
                case "stop":
                    brain.SetAutoPilotEnabled(false);
                    brain.SetCollisionAvoidance(false);
                    brain.DampenersOverride = false;
                    Echo("Auto Pilot Disabled.");
                    break;
            }
            


            info.WritePublicText("");
            info.WritePublicText(brain.CalculateShipMass().TotalMass.ToString() + " : Mass \n", true);
            
            //calculate manhattan distance
            int dist = (int)Math.Ceiling(Math.Abs(homebase.X - brain.GetPosition().X) + Math.Abs(homebase.Y - brain.GetPosition().Y) + Math.Abs(homebase.Z - brain.GetPosition().Z));
            info.WritePublicText(dist.ToString()+" :Distance \n",true);
            //debugging to an lcd screen - used as a visual aid
            info.WritePublicText(m.Forward.X.ToString(),true);

            //found out how to use and turn gyro
            //if (dist < 20 && m.Forward.X < 0.1)
            //{
            //    gyro.SetValueFloat("Yaw", 2);
            //    gyro.GetActionWithName("Override").Apply(gyro);
            //}
           
            //check for new homebase coords
            if (listeners[0].HasPendingMessage)
            {
                packet = listeners[0].AcceptMessage();
                string messagetext = packet.Data.ToString();
                sentcord = messagetext;
            }

            //check for new homebase velocity
            if (listeners[1].HasPendingMessage)
            {
                packet = listeners[1].AcceptMessage();
                string messagetext1 = packet.Data.ToString();
                sentvelocity = messagetext1;
            }
            string[] coords = sentcord.Split(' ');
            if (coords[0] != "")
            {
                coord[0] = double.Parse(coords[0].Remove(0, 2));
                coord[1] = double.Parse(coords[1].Remove(0, 2));
                coord[2] = double.Parse(coords[2].Remove(0, 2));
            }

            string[] velocity = sentvelocity.Split(' ');
            if (velocity[0] != "")
            {
                velocityout[0] = double.Parse(velocity[0].Remove(0, 2));
                velocityout[1] = double.Parse(velocity[1].Remove(0, 2));
                velocityout[2] = double.Parse(velocity[2].Remove(0, 2));
            }
            GetPredictedTargetPosition(homebase, homebasevelocity);

            //add new thrusters to list
            for (int i = 0; i < term.Count; i++)
            {
                onGridThrust.Add((IMyThrust)term[i]);
            }
        }

        public void GetPredictedTargetPosition(Vector3D homebase,Vector3D homebasevelocity)
        {  
            Vector3D homeship = homebase; //Get the current position of home
            Vector3D shippos = brain.GetPosition(); //Get the position of the drone, so we can calculate distance to home

            Vector3D toTarget = homeship - shippos; //Substract positions so we're left with a vector of the lenght and direction between ship and home

            Vector3D diffVelocity = homebasevelocity; //Get the targets velocity vector, needs to be redone if the turret isn't static

            double projectileSpeed = brain.GetShipSpeed(); //Define projectile speed of our guns

            double a = diffVelocity.LengthSquared() - projectileSpeed * projectileSpeed; //Target velocity squared minus projectile velocity squared  -- Why?
            double b = 2 * Vector3D.Dot(diffVelocity, toTarget); // 2 * dot product of target velocity and target vector  -- Why?
            double c = toTarget.LengthSquared(); // distance to target squared  -- why?

            // x = (-b +/- sqrt(b^2 - 4 * a * c )) / (2 * a)  - Solving 2. degree equiation
            double p = -b / (2 * a);
            double q = Math.Sqrt((b * b) - 4 * a * c) / (2 * a);
            // x = (-b +/- sqrt(b^2 - 4 * a * c )) / (2 * a)  - Solving 2. degree equiation

            double t1 = p - q; //1. solution
            double t2 = p + q; //2. solution
            double t;

            if (t1 > t2 && t2 > 0)//Use the smallest of the two solutions, but only if it is above 0
            {
                t = t2;
            }
            else
            {
                t = t1;
            }

            //t is the time it takes for the projectile to reach the target

            targetPos = homeship + diffVelocity * t;

            if (targetPos.X.ToString() == "NaN")
            {
                Echo("current target is stopped");
            }
            else
            {
                Echo(targetPos.ToString());
            }
            Echo(homebasevelocity.ToString());
            
        }
    }
}