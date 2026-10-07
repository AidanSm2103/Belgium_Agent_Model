namespace AgentSim.Core.Persistence
{
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

        public string? LastAppliedScript { get; set; }
    }
}