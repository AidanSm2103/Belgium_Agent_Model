using System.Collections.Generic;
using AgentSim.Core.Scripting;
 
namespace AgentSim.Core.Persistence
{
    public class AgentSaveData
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Heading { get; set; }
        public bool IsActive { get; set; } = true;
        public string Species { get; set; } = "Default";
        public int ScriptIndex { get; set; } = -1;
    }

    public class SimulationSaveData
    {
        public int AgentCount { get; set; }

        public double WorldWidth { get; set; }

        public double WorldHeight { get; set; }

        public double StepSize { get; set; }

        public double MaxTurnDegrees { get; set; }

        public int? Seed { get; set; }

        public int SecondaryGroupCount { get; set; }

        public string PrimaryGroupSpecies { get; set; } = "GroupA";

        public string SecondaryGroupSpecies { get; set; } = "GroupB";
        public int PatchColumns { get; set; } = 20;

        public int PatchRows { get; set; } = 20;

        public double PatchInitialValue { get; set; } = 100;

        public int PatchRegrowthTicks { get; set; } = 0;

        public string? LastAppliedScript { get; set; }

        public int? TickCount { get; set; }

        public List<string>? Scripts { get; set; }

        public List<AgentSaveData>? Agents { get; set; }

        public List<double>? PatchValues { get; set; }

        public List<int>? PatchTimers { get; set; }

        public List<ScriptAssignment>? ScriptAssignments { get; set; }
    }
}