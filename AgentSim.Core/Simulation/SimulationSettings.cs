using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.Simulation
{
    //Parameters for a simulation run
    //The UI's inputs from sliders etc set these before callign SumulationEngine.Setup()

    public class SimulationSettings
    {
        public int AgentCount{ get; set;} = 50;
        public double WorldWidth{get; set; } = 400;
        public double WorldHeight { get; set; } = 400;

        //How far our agent moves every tick
        //Maps to a speed slider
        public double StepSize { get; set; } = 2.0;
        //Max degrees agent turns every tick
        public double MaxTurnDegrees{ get; set; } = 25;
        //Set to a fixes nr for reproducible test runs later on
        public int? Seed { get; set; } = null;
    }
}
