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
                    new TrialResult { TrialIndex = 0, Seed = 101, FinalAgentCount = 15, FinalTickCount = 50 },
                    new TrialResult { TrialIndex = 1, Seed = 102, FinalAgentCount = 12, FinalTickCount = 50 }
                }
            };

            ResultsExporter.ExportToCsv(export, tempFilePath);

            Assert.True(File.Exists(tempFilePath));

            var lines = File.ReadAllLines(tempFilePath);
            Assert.Equal(3, lines.Length);
            Assert.Equal("TrialIndex,Seed,FinalAgentCount,FinalTickCount", lines[0]);
            Assert.Equal("0,101,15,50", lines[1]);
            Assert.Equal("1,102,12,50", lines[2]);
        }

        [Fact]
        public void Export_To_Csv()
        {
            var export = new MonteCarloSummary();

            ResultsExporter.ExportToCsv(export, tempFilePath);

            Assert.True(File.Exists(tempFilePath));

            var execute = File.ReadAllLines(tempFilePath);
            Assert.Single(execute);
            Assert.Equal("TrialIndex,Seed,FinalAgentCount,FinalTickCount", execute[0]);
        }
    }
}