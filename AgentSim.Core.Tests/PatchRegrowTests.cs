using AgentSim.Core.Simulation;
using AgentSim.Core.Worlds;
using Xunit;

namespace AgentSim.Core.Tests
{
    public class PatchRegrowthTests
    {
        private static SimulationEngine CreateEngine(int columns = 8, int rows = 6, double initialValue = 40, int regrowthTicks = 0)
        {
            var engine = new SimulationEngine(new SimulationSettings
            {
                AgentCount = 0,              // no agents, so only the patch logic is under test
                WorldWidth = 100,
                WorldHeight = 100,
                PatchColumns = columns,
                PatchRows = rows,
                PatchInitialValue = initialValue,
                PatchRegrowthTicks = regrowthTicks
            });
            engine.Setup();
            return engine;
        }

        [Fact]
        public void InitializePatches_UsesGivenSizeAndInitialValue()
        {
            var world = new World(100, 100);

            world.InitializePatches(5, 4, 50);

            Assert.Equal(5, world.Patches!.GetLength(0));
            Assert.Equal(4, world.Patches.GetLength(1));
            Assert.Equal(50.0, world.PatchMaxValue);
            foreach (var patch in world.Patches)
            {
                Assert.Equal(50.0, patch.Value);
            }
        }

        [Fact]
        public void RegrowPatches_ZeroTicks_NeverRegrows()
        {
            var world = new World(100, 100);
            world.InitializePatches(2, 2, 100);
            world.Patches![0, 0].Value = 0;

            for (int i = 0; i < 50; i++)
            {
                world.RegrowPatches(0);
            }

            Assert.Equal(0.0, world.Patches[0, 0].Value);
        }

        [Fact]
        public void RegrowPatches_RestoresPatchAfterConfiguredTicks()
        {
            var world = new World(100, 100);
            world.InitializePatches(2, 2, 100);
            world.Patches![0, 0].Value = 0;

            world.RegrowPatches(3);
            world.RegrowPatches(3);
            Assert.Equal(0.0, world.Patches[0, 0].Value);   // 2 ticks empty: not yet

            world.RegrowPatches(3);
            Assert.Equal(100.0, world.Patches[0, 0].Value); // 3rd tick: regrown
        }

        [Fact]
        public void RegrowPatches_LeavesGrownPatchesAlone()
        {
            var world = new World(100, 100);
            world.InitializePatches(2, 2, 100);
            world.Patches![1, 1].Value = 60;

            for (int i = 0; i < 10; i++)
            {
                world.RegrowPatches(2);
            }

            Assert.Equal(60.0, world.Patches[1, 1].Value);
        }

        [Fact]
        public void Setup_UsesPatchSettings()
        {
            var engine = CreateEngine(columns: 8, rows: 6, initialValue: 40);

            var patches = engine.Worlds.Patches!;
            Assert.Equal(8, patches.GetLength(0));
            Assert.Equal(6, patches.GetLength(1));
            Assert.Equal(40.0, patches[0, 0].Value);
            Assert.Equal(40.0, patches[7, 5].Value);
        }

        [Fact]
        public void Setup_WithInvalidGridSize_FallsBackToOneByOne()
        {
            var engine = CreateEngine(columns: 0, rows: -3);

            Assert.Equal(1, engine.Worlds.Patches!.GetLength(0));
            Assert.Equal(1, engine.Worlds.Patches.GetLength(1));
            Assert.NotNull(engine.Worlds.GetPatchAt(50, 50));
        }

        [Fact]
        public void Tick_RegrowsEmptyPatchAfterConfiguredTicks()
        {
            var engine = CreateEngine(initialValue: 40, regrowthTicks: 2);
            engine.Worlds.Patches![0, 0].Value = 0;

            engine.Tick();
            Assert.Equal(0.0, engine.Worlds.Patches[0, 0].Value);

            engine.Tick();
            Assert.Equal(40.0, engine.Worlds.Patches[0, 0].Value);
        }

        [Fact]
        public void Tick_WithRegrowthDisabled_KeepsPatchEmpty()
        {
            var engine = CreateEngine(initialValue: 40, regrowthTicks: 0);
            engine.Worlds.Patches![0, 0].Value = 0;

            for (int i = 0; i < 20; i++)
            {
                engine.Tick();
            }

            Assert.Equal(0.0, engine.Worlds.Patches[0, 0].Value);
        }
    }
}
