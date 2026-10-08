using System;
using System.IO;
using System.Linq;
using AgentSim.Core.Agents;
using AgentSim.Core.Analysis;
using AgentSim.Core.Persistence;
using AgentSim.Core.Scripting;
using AgentSim.Core.Simulation;
using Xunit;

namespace AgentSim.Core.Tests
{
    public class SnapshotAndMonteCarloScriptTests
    {
        private const string KillScript = "Engine.QueueKill(Agent);";

        private static SimulationSettings MakeSettings(int agentCount = 6, int secondary = 0) => new()
        {
            AgentCount = agentCount,
            SecondaryGroupCount = secondary,
            PrimaryGroupSpecies = "A",
            SecondaryGroupSpecies = "B",
            WorldWidth = 100,
            WorldHeight = 100,
            PatchColumns = 4,
            PatchRows = 4,
            Seed = 7
        };

        // Compiles and applies without the dry run, so these tests never
        // depend on how long Roslyn's first execution takes.
        private static void ApplyAndRecord(SimulationEngine engine, string? species, string source)
        {
            var compiled = BehaviorCompiler.Compile(source);
            Assert.True(compiled.Success, compiled.ErrorMessage);

            if (species == null)
                engine.ApplyBehaviorToAllAgents(compiled.Behavior!);
            else
                engine.ApplyBehaviorToAgentsMatching(a => a.Species == species, compiled.Behavior!);

            engine.RecordScriptAssignment(species, source);
        }

        // ---------- Save / load snapshot ----------

        [Fact]
        public void Snapshot_RoundTrip_RestoresAgentsPatchesAndTickCount()
        {
            var engine = new SimulationEngine(MakeSettings(secondary: 2));
            engine.Setup();
            engine.Worlds.Patches![1, 2].Value = 0;
            for (int i = 0; i < 3; i++) engine.Tick();

            var data = new SimulationSaveData();
            SimulationSnapshot.Capture(engine, data);

            var restored = new SimulationEngine(MakeSettings(secondary: 2));
            var warning = SimulationSnapshot.Restore(restored, data);

            Assert.Null(warning);
            Assert.Equal(3, restored.TickCount);
            Assert.Equal(engine.Worlds.Agents.Count, restored.Worlds.Agents.Count);
            Assert.Equal(0.0, restored.Worlds.Patches![1, 2].Value);

            for (int i = 0; i < engine.Worlds.Agents.Count; i++)
            {
                var before = engine.Worlds.Agents[i];
                var after = restored.Worlds.Agents[i];
                Assert.Equal(before.X, after.X, precision: 6);
                Assert.Equal(before.Y, after.Y, precision: 6);
                Assert.Equal(before.Species, after.Species);
            }
        }

        [Fact]
        public void Snapshot_Restore_KeepsScriptedBehaviorsAndAssignments()
        {
            var engine = new SimulationEngine(MakeSettings(agentCount: 6, secondary: 3));
            engine.Setup();
            ApplyAndRecord(engine, "B", KillScript);

            var data = new SimulationSaveData();
            SimulationSnapshot.Capture(engine, data);

            var restored = new SimulationEngine(MakeSettings(agentCount: 6, secondary: 3));
            Assert.Null(SimulationSnapshot.Restore(restored, data));

            foreach (var agent in restored.Worlds.Agents)
            {
                if (agent.Species == "B")
                {
                    var scripted = Assert.IsType<ScriptedBehavior>(agent.Behavior);
                    Assert.Equal(KillScript, scripted.SourceCode);
                }
                else
                {
                    Assert.IsType<RandomWalkBehavior>(agent.Behavior);
                }
            }

            Assert.Single(restored.ScriptAssignments);
            Assert.Equal(new ScriptAssignment("B", KillScript), restored.ScriptAssignments[0]);
        }

        [Fact]
        public void Snapshot_SurvivesWritingToAndReadingFromAFile()
        {
            var engine = new SimulationEngine(MakeSettings(agentCount: 6, secondary: 3));
            engine.Setup();
            ApplyAndRecord(engine, "B", KillScript);
            engine.Worlds.Patches![0, 0].Value = 0;

            var data = new SimulationSaveData();
            SimulationSnapshot.Capture(engine, data);

            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
            try
            {
                SimulationPersistence.Save(data, path);
                var loaded = SimulationPersistence.Load(path);
                Assert.NotNull(loaded);

                var restored = new SimulationEngine(MakeSettings(agentCount: 6, secondary: 3));
                Assert.Null(SimulationSnapshot.Restore(restored, loaded!));

                Assert.Equal(6, restored.Worlds.Agents.Count);
                Assert.Equal(0.0, restored.Worlds.Patches![0, 0].Value);
                Assert.Equal(new ScriptAssignment("B", KillScript), Assert.Single(restored.ScriptAssignments));
                Assert.Contains(restored.Worlds.Agents, a => a.Behavior is ScriptedBehavior);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void Snapshot_OlderFileWithoutAgents_FallsBackToFreshSetup()
        {
            var restored = new SimulationEngine(MakeSettings(agentCount: 5));
            var oldStyleData = new SimulationSaveData { AgentCount = 5 };   // Agents == null

            var warning = SimulationSnapshot.Restore(restored, oldStyleData);

            Assert.Null(warning);
            Assert.Equal(5, restored.Worlds.Agents.Count);
            Assert.Equal(0, restored.TickCount);
        }

        // ---------- Script assignment bookkeeping ----------

        [Fact]
        public void RecordScriptAssignment_AllReplacesEverything_SpeciesReplacesItsOwnEntry()
        {
            var engine = new SimulationEngine(MakeSettings());

            engine.RecordScriptAssignment("A", "one");
            engine.RecordScriptAssignment("B", "two");
            engine.RecordScriptAssignment("A", "three");
            Assert.Equal(2, engine.ScriptAssignments.Count);
            Assert.Contains(new ScriptAssignment("A", "three"), engine.ScriptAssignments);
            Assert.DoesNotContain(new ScriptAssignment("A", "one"), engine.ScriptAssignments);

            engine.RecordScriptAssignment(null, "everyone");
            Assert.Equal(new ScriptAssignment(null, "everyone"), Assert.Single(engine.ScriptAssignments));
        }

        [Fact]
        public void Setup_ClearsRecordedScriptAssignments()
        {
            var engine = new SimulationEngine(MakeSettings());
            engine.Setup();
            engine.RecordScriptAssignment(null, KillScript);

            engine.Setup();

            Assert.Empty(engine.ScriptAssignments);
        }

        // ---------- Monte Carlo uses applied scripts ----------

        [Fact]
        public void MonteCarlo_WithoutScripts_KeepsEveryAgent()
        {
            var summary = MonteCarloRunner.Run(new MonteCarloSettings
            {
                TrialCount = 3,
                TicksPerTrial = 3,
                BaseSeed = 1,
                BaseSettings = MakeSettings(agentCount: 10)
            });

            Assert.Equal(10.0, summary.MeanFinalAgentCount);
            Assert.Equal(0.0, summary.StdDevFinalAgentCount);
        }

        [Fact]
        public void MonteCarlo_WithScriptForEveryone_UsesTheScript()
        {
            var summary = MonteCarloRunner.Run(new MonteCarloSettings
            {
                TrialCount = 3,
                TicksPerTrial = 3,
                BaseSeed = 1,
                BaseSettings = MakeSettings(agentCount: 10),
                ScriptAssignments = new[] { new ScriptAssignment(null, KillScript) }
            });

            // Every agent kills itself on the first tick, so none are left alive.
            Assert.Equal(0.0, summary.MeanFinalAgentCount);
            Assert.Equal(0, summary.MaxFinalAgentCount);
        }

        [Fact]
        public void MonteCarlo_WithScriptForOneSpecies_OnlyAffectsThatSpecies()
        {
            var summary = MonteCarloRunner.Run(new MonteCarloSettings
            {
                TrialCount = 3,
                TicksPerTrial = 3,
                BaseSeed = 1,
                BaseSettings = MakeSettings(agentCount: 10, secondary: 4),
                ScriptAssignments = new[] { new ScriptAssignment("B", KillScript) }
            });

            // 4 of the 10 agents are species "B" and die; the 6 of species "A" survive.
            Assert.Equal(6.0, summary.MeanFinalAgentCount);
            Assert.Equal(6, summary.MinFinalAgentCount);
            Assert.Equal(6, summary.MaxFinalAgentCount);
        }

        [Fact]
        public void MonteCarlo_WithScriptThatDoesNotCompile_ThrowsClearError()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => MonteCarloRunner.Run(new MonteCarloSettings
            {
                TrialCount = 1,
                TicksPerTrial = 1,
                BaseSettings = MakeSettings(),
                ScriptAssignments = new[] { new ScriptAssignment(null, "this is not valid C# code;") }
            }));

            Assert.Contains("could not be compiled", ex.Message);
        }
    }
}