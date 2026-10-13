using System;
using System.IO;
using System.Collections.Generic;
using Xunit;
using AgentSim.Core.Analysis;

namespace AgentSim.Core.Tests
{
    public class PersistenceTests : IDisposable
    {
        private readonly string tempFilePath;

        public PersistenceTests()
        {
            tempFilePath = Path.Combine(Path.GetTempPath(), $"test_export_{Guid.NewGuid()}.csv");
        }

        public void Dispose()
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }

        [Fact]
        public void Export_File_To_Create_CSV()
        {
            var export = new MonteCarloSummary
            {
                Trials = new List<TrialResult>
                {
                    new TrialResult { TrialIndex = 0, Seed = 101, StartAgentCount = 20, FinalAgentCount = 15, FinalTickCount = 50 },
                    new TrialResult { TrialIndex = 1, Seed = 102, StartAgentCount = 15, FinalAgentCount = 12, FinalTickCount = 50 }
                }
            };

            ResultsExporter.ExportToCsv(export, tempFilePath);

            Assert.True(File.Exists(tempFilePath));

            var lines = File.ReadAllLines(tempFilePath);
            Assert.Equal(3, lines.Length);
            Assert.Equal("TrialIndex,Seed,StartAgentCount,FinalAgentCount,NetChange,SurvivalRate,FinalMeanPatchValue,FinalTickCount", lines[0]);
            Assert.Equal("0,101,20,15,-5,0.75,0,50", lines[1]);
            Assert.Equal("1,102,15,12,-3,0.8,0,50", lines[2]);
        }

        [Fact]
        public void Export_To_Csv()
        {
            var export = new MonteCarloSummary();

            ResultsExporter.ExportToCsv(export, tempFilePath);

            Assert.True(File.Exists(tempFilePath));

            var execute = File.ReadAllLines(tempFilePath);
            Assert.Single(execute);
            Assert.Equal("TrialIndex,Seed,StartAgentCount,FinalAgentCount,NetChange,SurvivalRate,FinalMeanPatchValue,FinalTickCount", execute[0]);
        }
    }
}