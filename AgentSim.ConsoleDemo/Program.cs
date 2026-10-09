using AgentSim.Core.Agents;
using AgentSim.Core.Analysis;
using AgentSim.Core.Scripting;
using AgentSim.Core.Simulation;
using System.Linq;

Console.WriteLine("=== AgentSim Studio — Console Demo (Core library, no UI) ===\n");

// DEMO 1: Headless Setup/Tick loop
Console.WriteLine("--- Demo 1: Headless Setup/Tick loop ---");

var settings = new SimulationSettings
{
    AgentCount = 10,
    WorldWidth = 100,
    WorldHeight = 100,
    Seed = 42
};

var engine = new SimulationEngine(settings);
engine.Setup();

for (int i = 0; i < 5; i++)
{
    engine.Tick();
    Console.WriteLine($"Tick {engine.TickCount}: {engine.Worlds.Agents.Count} agents active");
}

// DEMO 2: Scripting — compile and apply a built-in template with zero UI
Console.WriteLine("\n--- Demo 2: Compiling and applying a scripted behavior ---");

var compileResult = BehaviorCompiler.Compile(ScriptTemplates.FleeToCenter);

if (compileResult.Success)
{
    Console.WriteLine("Script compiled successfully. Applying to all agents...");
    engine.ApplyBehaviorToAllAgents(compileResult.Behavior!);

    for (int i = 0; i < 5; i++)
    {
        engine.Tick();
        var avgDistFromCenter = engine.Worlds.Agents.Average(a =>
        {
            double dx = a.X - settings.WorldWidth / 2;
            double dy = a.Y - settings.WorldHeight / 2;
            return Math.Sqrt(dx * dx + dy * dy);
        });
        Console.WriteLine($"Tick {engine.TickCount}: avg distance from center = {avgDistFromCenter:F2}");
    }
}
else
{
    Console.WriteLine($"Script compilation failed: {compileResult.ErrorMessage}");
}


// DEMO 3: Monte Carlo batch run
Console.WriteLine("\n--- Demo 3: Monte Carlo batch run ---");

var mcSettings = new MonteCarloSettings
{
    TrialCount = 20,
    TicksPerTrial = 100,
    BaseSeed = 12345,
    BaseSettings = new SimulationSettings
    {
        AgentCount = 30,
        WorldWidth = 100,
        WorldHeight = 100
    }
};

var summary = MonteCarloRunner.Run(mcSettings);

Console.WriteLine($"Trials run: {summary.Trials.Count}");
Console.WriteLine($"Mean final agent count: {summary.MeanFinalAgentCount:F2}");
Console.WriteLine($"StdDev: {summary.StdDevFinalAgentCount:F2}");
Console.WriteLine($"Min: {summary.MinFinalAgentCount}  Max: {summary.MaxFinalAgentCount}");

var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "montecarlo_results.csv");
ResultsExporter.ExportToCsv(summary, csvPath);
Console.WriteLine($"Exported results to {csvPath}");

// OPTIONAL DEMO: Multi-species demo — two groups, two different scripts, headless

Console.WriteLine("\n--- Optional: Multi-species behavior demo ---");

var speciesSettings = new SimulationSettings
{
    AgentCount = 10,
    SecondaryGroupCount = 5,          // last 5 spawned = SecondaryGroupSpecies
    PrimaryGroupSpecies = "Wanderer",
    SecondaryGroupSpecies = "Seeker",
    WorldWidth = 100,
    WorldHeight = 100,
    Seed = 7
};

var speciesEngine = new SimulationEngine(speciesSettings);
speciesEngine.Setup();

var seekerResult = BehaviorCompiler.Compile(ScriptTemplates.FleeToCenter);
if (seekerResult.Success)
{
    foreach (var agent in speciesEngine.Worlds.Agents.Where(a => a.Species == "Seeker"))
    {
        agent.Behavior = seekerResult.Behavior!;
    }
}

for (int i = 0; i < 5; i++)
{
    speciesEngine.Tick();
}

var wanderers = speciesEngine.Worlds.Agents.Where(a => a.Species == "Wanderer");
var seekers = speciesEngine.Worlds.Agents.Where(a => a.Species == "Seeker");
Console.WriteLine($"Wanderer group (RandomWalk): {wanderers.Count()} agents");
Console.WriteLine($"Seeker group (FleeToCenter): {seekers.Count()} agents");

Console.WriteLine("\n=== Demo complete ===");