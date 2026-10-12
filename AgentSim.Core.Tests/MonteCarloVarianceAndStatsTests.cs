using System;
using System.IO;
using System.Linq;
using AgentSim.Core.Analysis;
using AgentSim.Core.Scripting;
using AgentSim.Core.Simulation;
using Xunit;

namespace AgentSim.Core.Tests
{
    public class MonteCarloVarianceAndStatsTests
    {
        private static SimulationSettings MakeSettings(int agentCount, int secondary = 0) => new()
        {
            AgentCount = agentCount,
            SecondaryGroupCount = secondary,
            PrimaryGroupSpecies = "A",
            SecondaryGroupSpecies = "B",
            WorldWidth = 100,
            WorldHeight = 100,
            PatchColumns = 4,
            PatchRows = 4
        };

        private static MonteCarloSummary Run(
            int agentCount, int secondary, double variance, int trials = 30, int baseSeed = 1,
            params ScriptAssignment[] scripts) =>
            MonteCarloRunner.Run(new MonteCarloSettings
            {
                TrialCount = trials,
                TicksPerTrial = 3,
                BaseSeed = baseSeed,
                BaseSettings = MakeSettings(agentCount, secondary),
                StartCountVariancePercent = variance,
                ScriptAssignments = scripts
            });

        // ---------- Start-count variance ----------

        [Fact]
        public void ZeroVariance_EveryTrialStartsWithTheConfiguredCount()
        {
            var summary = Run(agentCount: 30, secondary: 0, variance: 0);

            Assert.All(summary.Trials, t => Assert.Equal(30, t.StartAgentCount));
        }

        [Fact]
        public void Variance_SpreadsStartCountsWithinThePercentage()
        {
            var summary = Run(agentCount: 30, secondary: 0, variance: 10);

            // 10% of 30 = +/- 3 agents.
            Assert.All(summary.Trials, t => Assert.InRange(t.StartAgentCount, 27, 33));
            Assert.True(summary.Trials.Select(t => t.StartAgentCount).Distinct().Count() > 1,
                "Expected the starting counts to differ between trials.");
        }

        [Fact]
        public void Variance_TrialsReallyStartWithTheirRecordedCount()
        {
            // With no ticks the engine's agent count is the start count.
            var summary = MonteCarloRunner.Run(new MonteCarloSettings
            {
                TrialCount = 10,
                TicksPerTrial = 0,
                BaseSeed = 5,
                BaseSettings = MakeSettings(30),
                StartCountVariancePercent = 20
            });

            Assert.All(summary.Trials, t => Assert.Equal(t.StartAgentCount, t.FinalAgentCount));
        }

        [Fact]
        public void Variance_SameSeedGivesTheSameStartCounts()
        {
            var first = Run(agentCount: 30, secondary: 0, variance: 10, baseSeed: 42);
            var second = Run(agentCount: 30, secondary: 0, variance: 10, baseSeed: 42);

            Assert.Equal(
                first.Trials.Select(t => t.StartAgentCount),
                second.Trials.Select(t => t.StartAgentCount));
        }

        [Fact]
        public void Variance_AppliesToEachSpeciesGroupSeparately()
        {
            var summary = Run(agentCount: 30, secondary: 10, variance: 10);

            foreach (var t in summary.Trials)
            {
                Assert.Equal(t.StartPrimaryCount + t.StartSecondaryCount, t.StartAgentCount);
                Assert.InRange(t.StartPrimaryCount, 18, 22);    // 20 +/- 2
                Assert.InRange(t.StartSecondaryCount, 9, 11);   // 10 +/- 1
            }
        }

        // ---------- Per-species totals ----------

        [Fact]
        public void SpeciesTotals_ReflectAScriptThatKillsOnlyTheSecondarySpecies()
        {
            var summary = Run(30, 10, variance: 10, trials: 5,
                scripts: new[] { new ScriptAssignment("B", "Engine.QueueKill(Agent);") });

            Assert.True(summary.HasSecondarySpecies);
            Assert.All(summary.Trials, t =>
            {
                Assert.Equal(0, t.FinalSecondaryCount);
                Assert.Equal(t.StartPrimaryCount, t.FinalPrimaryCount);
                Assert.Equal(t.FinalPrimaryCount, t.FinalAgentCount);
                Assert.Equal(-t.StartSecondaryCount, t.NetChange);
            });
        }

        [Fact]
        public void Summary_ReportsSurvivalAndStatsBlocks()
        {
            var summary = Run(10, 0, variance: 0, trials: 4);

            Assert.Equal(4, summary.FinalTotal.Count);
            Assert.Equal(10.0, summary.FinalTotal.Mean);
            Assert.Equal(10.0, summary.FinalTotal.Median);
            Assert.Equal(1.0, summary.SurvivalRate.Mean);
            Assert.Equal(summary.MeanFinalAgentCount, summary.FinalTotal.Mean);
        }

        // ---------- StatisticsHelper ----------

        [Fact]
        public void Median_HandlesOddAndEvenCounts()
        {
            Assert.Equal(3.0, StatisticsHelper.Median(new[] { 5.0, 1.0, 3.0 }));
            Assert.Equal(2.5, StatisticsHelper.Median(new[] { 4.0, 1.0, 3.0, 2.0 }));
            Assert.Equal(0.0, StatisticsHelper.Median(Array.Empty<double>()));
        }

        [Fact]
        public void SampleStdDev_AndStandardError_MatchKnownValues()
        {
            var values = new[] { 1.0, 2.0, 3.0, 4.0, 5.0 };

            Assert.Equal(1.5811, StatisticsHelper.SampleStdDev(values), precision: 3);
            Assert.Equal(0.7071, StatisticsHelper.StandardError(values), precision: 3);
        }

        [Fact]
        public void ConfidenceInterval95_UsesStudentT()
        {
            // mean 3, SE 0.7071, t(df = 4) = 2.776  ->  3 +/- 1.963
            var (low, high) = StatisticsHelper.ConfidenceInterval95(new[] { 1.0, 2.0, 3.0, 4.0, 5.0 });

            Assert.Equal(1.037, low, precision: 2);
            Assert.Equal(4.963, high, precision: 2);
        }

        [Fact]
        public void Correlation_PerfectAndUndefinedCases()
        {
            Assert.Equal(1.0, StatisticsHelper.Correlation(new[] { 1.0, 2.0, 3.0 }, new[] { 2.0, 4.0, 6.0 }), precision: 6);
            Assert.Equal(-1.0, StatisticsHelper.Correlation(new[] { 1.0, 2.0, 3.0 }, new[] { 6.0, 4.0, 2.0 }), precision: 6);
            Assert.Equal(0.0, StatisticsHelper.Correlation(new[] { 1.0, 2.0, 3.0 }, new[] { 5.0, 5.0, 5.0 }));
            Assert.Equal(0.0, StatisticsHelper.Correlation(new[] { 1.0 }, new[] { 1.0 }));
        }

        // ---------- CSV export ----------

        [Fact]
        public void Csv_HasPerSpeciesColumnsAndOneRowPerTrial()
        {
            var summary = Run(30, 10, variance: 10, trials: 6);
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");

            try
            {
                ResultsExporter.ExportToCsv(summary, path);
                var lines = File.ReadAllLines(path);

                Assert.Equal(7, lines.Length);   // header + 6 trials

                var header = lines[0].Split(',');
                Assert.Contains("StartAgentCount", header);
                Assert.Contains("Start_A", header);
                Assert.Contains("Start_B", header);
                Assert.Contains("FinalAgentCount", header);
                Assert.Contains("Final_A", header);
                Assert.Contains("Final_B", header);
                Assert.Contains("SurvivalRate", header);

                // A decimal comma would split a value into two columns.
                Assert.All(lines.Skip(1), line => Assert.Equal(header.Length, line.Split(',').Length));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void Csv_SingleSpecies_OmitsTheSecondarySpeciesColumns()
        {
            var summary = Run(10, 0, variance: 0, trials: 2);
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");

            try
            {
                ResultsExporter.ExportToCsv(summary, path);
                var header = File.ReadAllLines(path)[0].Split(',');

                Assert.DoesNotContain("Start_B", header);
                Assert.DoesNotContain("Final_B", header);
                Assert.Contains("FinalAgentCount", header);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void SummaryCsv_ContainsStatisticsRows()
        {
            var summary = Run(30, 10, variance: 10, trials: 8);
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");

            try
            {
                ResultsExporter.ExportSummaryToCsv(summary, path);
                var text = File.ReadAllText(path);

                Assert.StartsWith("Metric,Count,Mean,Median,StdDev,SampleStdDev,StdError,CI95Low,CI95High,Min,Max", text);
                Assert.Contains("FinalAgentCount,8,", text);
                Assert.Contains("Final_B,8,", text);
                Assert.Contains("StartFinalCorrelation,", text);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }
    }
}

