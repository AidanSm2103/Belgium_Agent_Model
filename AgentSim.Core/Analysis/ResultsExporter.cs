using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace AgentSim.Core.Analysis
{
    // Exports Monte Carlo results to CSV for external analysis (Excel,
    // Python, etc.). This is ordinary C# file I/O, not a user script — it
    // is NOT affected by ScriptSafetyChecker's denylist, which only
    // applies to compiled user scripts.
    //
    // Numbers are always written with a '.' decimal point regardless of the
    // computer's language settings; with a ',' decimal (e.g. a Belgian or
    // South African locale) the values would split into extra columns.
    public static class ResultsExporter
    {
        // One row per trial.
        public static void ExportToCsv(MonteCarloSummary summary, string filePath)
        {
            string primary = SafeName(summary.PrimarySpecies, "Primary");
            string secondary = SafeName(summary.SecondarySpecies, "Secondary");

            var header = new List<string> { "TrialIndex", "Seed", "StartAgentCount" };
            if (summary.HasSecondarySpecies)
            {
                header.Add("Start_" + primary);
                header.Add("Start_" + secondary);
            }
            header.Add("FinalAgentCount");
            if (summary.HasSecondarySpecies)
            {
                header.Add("Final_" + primary);
                header.Add("Final_" + secondary);
            }
            header.AddRange(new[] { "NetChange", "SurvivalRate", "FinalMeanPatchValue", "FinalTickCount" });

            var lines = new List<string> { string.Join(",", header) };

            foreach (var t in summary.Trials)
            {
                var row = new List<string>
                {
                    I(t.TrialIndex), I(t.Seed), I(t.StartAgentCount)
                };
                if (summary.HasSecondarySpecies)
                {
                    row.Add(I(t.StartPrimaryCount));
                    row.Add(I(t.StartSecondaryCount));
                }
                row.Add(I(t.FinalAgentCount));
                if (summary.HasSecondarySpecies)
                {
                    row.Add(I(t.FinalPrimaryCount));
                    row.Add(I(t.FinalSecondaryCount));
                }
                row.Add(I(t.NetChange));
                row.Add(D(t.SurvivalRate));
                row.Add(D(t.FinalMeanPatchValue));
                row.Add(I(t.FinalTickCount));

                lines.Add(string.Join(",", row));
            }

            File.WriteAllLines(filePath, lines);
        }

        // Summary statistics across all trials, one row per measurement,
        // followed by a few single-value statistics.
        public static void ExportSummaryToCsv(MonteCarloSummary summary, string filePath)
        {
            string primary = SafeName(summary.PrimarySpecies, "Primary");
            string secondary = SafeName(summary.SecondarySpecies, "Secondary");

            var lines = new List<string>
            {
                "Metric,Count,Mean,Median,StdDev,SampleStdDev,StdError,CI95Low,CI95High,Min,Max"
            };

            lines.Add(StatsRow("StartAgentCount", summary.StartTotal));
            lines.Add(StatsRow("FinalAgentCount", summary.FinalTotal));
            lines.Add(StatsRow("Final_" + primary, summary.FinalPrimary));
            if (summary.HasSecondarySpecies)
            {
                lines.Add(StatsRow("Final_" + secondary, summary.FinalSecondary));
            }
            lines.Add(StatsRow("SurvivalRate", summary.SurvivalRate));
            lines.Add(StatsRow("FinalMeanPatchValue", summary.FinalMeanPatchValue));

            lines.Add("");
            lines.Add("Statistic,Value");
            lines.Add("TrialCount," + I(summary.Trials.Count));
            lines.Add("TicksPerTrial," + I(summary.Trials.FirstOrDefault()?.FinalTickCount ?? 0));
            lines.Add("BaseStartAgentCount," + I(summary.BaseStartAgentCount));
            lines.Add("StartCountVariancePercent," + D(summary.StartCountVariancePercent));
            lines.Add("StartFinalCorrelation," + D(summary.StartFinalCorrelation));

            File.WriteAllLines(filePath, lines);
        }

        private static string StatsRow(string metric, SeriesStats s) =>
            string.Join(",", new[]
            {
                metric, I(s.Count), D(s.Mean), D(s.Median), D(s.StdDev), D(s.SampleStdDev),
                D(s.StdError), D(s.CiLow), D(s.CiHigh), D(s.Min), D(s.Max)
            });

        private static string I(int value) => value.ToString(CultureInfo.InvariantCulture);
        private static string D(double value) => value.ToString("0.####", CultureInfo.InvariantCulture);

        // Species names go into column headers, so keep only characters that
        // can't break the CSV (no commas, quotes, or spaces).
        private static string SafeName(string name, string fallback)
        {
            var sb = new StringBuilder();
            foreach (char c in name ?? "")
            {
                sb.Append(char.IsLetterOrDigit(c) ? c : '_');
            }
            return sb.Length == 0 ? fallback : sb.ToString();
        }
    }
}

