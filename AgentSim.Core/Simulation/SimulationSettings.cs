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
        public int AgentCount { get; set; } = 50;
        public double WorldWidth { get; set; } = 400;
        public double WorldHeight { get; set; } = 400;

        //How far our agent moves every tick
        //Maps to a speed slider
        public double StepSize { get; set; } = 2.0;
        //Max degrees agent turns every tick
        public double MaxTurnDegrees { get; set; } = 25;
        //Set to a fixes nr for reproducible test runs later on
        public int? Seed { get; set; } = null;

        // Multi-species support: how many of the spawned agents belong to
        // a second named group. The rest (AgentCount - SecondaryGroupCount)
        // belong to the primary group. Set SecondaryGroupCount = 0 (default)
        // for the old single-species behavior, unchanged.
        public int SecondaryGroupCount { get; set; } = 0;
        public string PrimaryGroupSpecies { get; set; } = "GroupA";
        public string SecondaryGroupSpecies { get; set; } = "GroupB";

        // Patch grid configuration, applied at Setup().
        // Grid resolution (values below 1 are treated as 1).
        public int PatchColumns { get; set; } = 20;
        public int PatchRows { get; set; } = 20;

        // Value every patch starts at. This is also the value an empty
        // patch regrows to.
        public double PatchInitialValue { get; set; } = 100;

        // How many ticks an empty patch (Value <= 0) stays empty before it
        // regrows to PatchInitialValue. 0 = patches never regrow.
        public int PatchRegrowthTicks { get; set; } = 0;
    }
}