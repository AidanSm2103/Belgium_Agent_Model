using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSim.Core.Utilities; 
using AgentSim.Core.Worlds;
using AgentSim.Core.Agents;
using System.Runtime.CompilerServices;

//Description
//This class owns the tick loop and the current state of the simulation
//It does not have a timer but rather the UI calls Tick() repeatedly
//once the user clicks "Start" for example
namespace AgentSim.Core.Simulation
{
    public class SimulationEngine
    {
        public World Worlds {get; private set;}
        public SimulationSettings Settings { get;}
        public int TickCount{get; private set;}
        public bool IsRunning {get; private set;}

        private RandomProvider _rng;

        //The rendering side of the UI will be subscribed to this
        //this is to know when it is necessary to redraw the canvas
        public event EventHandler? Ticked;

        public SimulationEngine(SimulationSettings settings)
        {
            Settings = settings;
            _rng = new RandomProvider(settings.Seed);
            Worlds = new World(settings.WorldWidth, settings.WorldHeight);
        }

        //SETUP button in NetLogo
        //below is used to reinitialize the world and have the agents be spawned fresh
        //THis is called before the first Tick() and again any time the user wants to restart
        public void Setup()
        {
            _rng = new RandomProvider(Settings.Seed);
            Worlds = new World(Settings.WorldWidth, Settings.WorldHeight);
            TickCount = 0;

            for(int i=0; i<Settings.AgentCount; i++)
            {
                double x = _rng.NextDouble()*Settings.WorldWidth;
                double y = _rng.NextDouble()*Settings.WorldHeight;
                double heading = _rng.NextDouble()*360;

                var behavior = new RandomWalkBehavior(Settings.MaxTurnDegrees, Settings.StepSize);
                Worlds.AddAgent(new Agent(i, x, y, heading, behavior));
            }
            Ticked?.Invoke(this,EventArgs.Empty);
        }

        //GO button in NetLogo
        //This will advance our simulation by exactly one tick - every agent moves once
        //The UI calls this on a timer for continuous running or only onxe per click for the Setup button from above
        public void Tick()
        {
            foreach(var agent in Worlds.Agents)
            {
                agent.Step(Worlds, _rng);
            }

            TickCount++;
            Ticked?.Invoke(this, EventArgs.Empty);
        }

        public void Start() => IsRunning = true;
        public void Stop() => IsRunning = false;
    }
}
