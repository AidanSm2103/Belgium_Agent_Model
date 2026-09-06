using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.Analysis
{
    // Exports Monte Carlo results to CSV for external analysis 
    public static class ResultsExporter
    {
        public static void ExportToCsv(MonteCarloSummary summary, string filePath)
        {
            var lines = new List<string> { "TrialIndex,Seed,FinalAgentCount,FinalTickCount" };
            lines.AddRange(summary.Trials.Select(t =>
                $"{t.TrialIndex},{t.Seed},{t.FinalAgentCount},{t.FinalTickCount}"));

            File.WriteAllLines(filePath, lines);
        }
    }
}
