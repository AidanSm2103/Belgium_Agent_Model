<div align="center">

# Belgium Agent Model (BAM)

**C#/WPF agent-based modelling application inspired by NetLogo**

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![WPF](https://img.shields.io/badge/UI-WPF-0078D7?logo=windows&logoColor=white)
![Roslyn Scripting](https://img.shields.io/badge/Scripting-Roslyn-68217A?logo=csharp&logoColor=white)
![Tests](https://img.shields.io/badge/tests-xUnit-25A162)
![Status](https://img.shields.io/badge/status-Final%20Release-brightgreen)

*Year-long university group project. A general-purpose agent-based modelling tool where users can define their own agent behavior at runtime, run statistical batch analysis, and observe multiple interacting agent types, built as a reusable C# library with a WPF front end.*

</div>

---

## 📍 Current status: 

Development is feature-complete. Remaining work going forward is limited to bug fixes and polish rather than new functionality.

| Area | Status |
|---|:---:|
| Core simulation loop (Setup / Step / Go) | ✅ |
| Torus-wrapped world | ✅ |
| Real-time rendering | ✅ |
| Runtime C# scripting (Roslyn) | ✅ |
| Script safety checks + execution timeout | ✅ |
| In-app rules editor, with an expandable full-size view | ✅ |
| Configurable patch grid (size, starting value, regrowth) — readable/writable, rendered live on the canvas | ✅ |
| Live plot + monitors (ticks, agent count) | ✅ |
| Agent rendering reflects species and active behavior | ✅ |
| Safe runtime spawn/kill of agents from scripts | ✅ |
| Multi-species agent support, with per-species script targeting | ✅ |
| Monte Carlo batch simulation with statistical summaries + CSV export | ✅ |
| Save / load simulation configuration and scripts | ✅ |
| Reusable library packaging + standalone console demo | ✅ |

### Feature overview

- **Write and apply agent behavior without recompiling.** The rules editor compiles a C# snippet via Roslyn and applies it live, with clear compile-error feedback if it doesn't build. An expand button opens the editor in a larger floating overlay for more comfortable editing, without disturbing the rest of the layout.
- **Scripts run safely.** A lightweight denylist blocks obviously unsafe operations (file/network/process access), and any script that hangs is timed out rather than freezing the simulation. Agents can safely spawn or remove other agents from within a script.
- **Multiple interacting agent types.** Agents can be split into two named species groups at setup, and a script can be applied to a single species rather than always affecting everyone — enabling predator/prey-style and other multi-agent interaction scenarios.
- **A real, visible environment.** The world has a patch grid agents and scripts can read from and write to, rendered live as a colored grid behind the agents. Grid size, starting value, and regrowth time are all configurable: an emptied patch (value 0) can regrow to its full value after a set number of ticks, or never if regrowth is turned off.
- **Statistical analysis.** A Monte Carlo panel runs the simulation many times over with varying random seeds, using the same settings and the same scripts you have applied to the simulation on screen. It reports mean/standard deviation/min/max of the final living agent count across trials, with CSV export for external analysis.
- **Save and resume.** A save file captures the whole simulation: settings, species groups, every agent's position and behavior, patch values, the tick count, and the scripts you applied. Loading puts you back where you left off with the scripts still active. Files saved by earlier versions (settings only) still load, and start a fresh Setup.
- **Live feedback.** A plot tracks agent count over time alongside tick/agent monitors, updating every tick.
- **Genuinely reusable.** `AgentSim.Core` has zero UI dependencies and ships with a standalone console demo proving it runs headlessly end to end — simulation, scripting, and Monte Carlo included.

## 🏗️ Architecture

Four projects keep the simulation engine reusable and independent of any specific UI:

```
AgentSim.Core/              Simulation engine — no UI dependencies
├── Agents/                   Agent, IAgentBehavior, RandomWalkBehavior
├── World/                    World, Patch
├── Simulation/                SimulationEngine, SimulationSettings
├── Scripting/                 BehaviorCompiler, ScriptedBehavior, ScriptGlobals,
│                              ScriptSafetyChecker, ScriptApplier, ScriptApplyResult,
│                              ScriptTemplates, BehaviorTestRunner
├── Analysis/                  MonteCarloSettings, MonteCarloResult, MonteCarloRunner,
│                              StatisticsHelper, ResultsExporter
├── Persistence/                SimulationSaveData, SimulationSnapshot, SimulationPersistence
└── Utilities/                  RandomProvider (seeded RNG)

AgentSim.Wpf/                WPF front end (MVVM)
├── Views/                     MainWindow, WorldCanvasControl, RulesEditorControl,
│                              PlotControl, MonteCarloControl
├── ViewModels/                 MainViewModel, SimulationViewModel,
│                              RulesEditorViewModel, MonteCarloViewModel
└── Helpers/                    RelayCommand, ViewModelBase

AgentSim.Core.Tests/          xUnit tests covering the engine, scripting, Monte Carlo,
                              multi-species support, and persistence

AgentSim.ConsoleDemo/         Standalone console app — proves AgentSim.Core runs with
                              zero UI involvement
```

`AgentSim.Core` has no reference to WPF or any UI framework — it can be driven headlessly, which is what makes it a reusable library rather than logic baked into the UI. `AgentSim.ConsoleDemo` exists specifically to prove that end to end.

**A note on script safety:** the safety checks (denylist + timeout) are a practical safeguard against accidental slow or unsafe scripts, not a hard security sandbox — genuine isolation from deliberately malicious code would need a separate process, which is out of scope for this project.

## ▶️ Running the project

1. Open `AgentSim.Core.sln` in Visual Studio 2022+ (requires the **.NET desktop development** workload)
2. Set `AgentSim.Wpf` as the Startup Project
3. Press F5

## ✍️ Using the app

1. Click **Setup** to spawn agents. Optionally configure a second species group first (primary/secondary species names and secondary group count) under **Species Groups**, and the patch grid (size, starting value, regrowth time) under **Patches**. Both are applied on Setup.
2. In the rules editor, write a script using `Agent`, `World`, `Rng`, and `Engine` (for safe spawn/kill). Use the expand button to open a larger editing view if needed.
3. Choose who the script applies to under **Apply to** — `All`, or a specific species group — then click **Apply Behavior**.
4. Click **Go** to run continuously, or **Step** to advance one tick at a time. Ticks, agent count, and a live plot update as it runs.
5. Use **Save**/**Load** to store or restore the current settings, species groups, and script.
6. Open the **Monte Carlo** panel to run many independent trials and view aggregated statistics, with an option to export results to CSV.

## 🧪 Running the tests

Open **Test Explorer** (Test menu → Test Explorer) and run all tests, or `Test → Run All Tests`. Coverage includes the simulation engine, world, agents, random number provider, the full scripting pipeline (compile, safety checks, timeout handling, apply), Monte Carlo statistics and determinism, multi-species behavior targeting, patch configuration and regrowth, and save/load persistence.

## 📦 Using AgentSim.Core as a library

`AgentSim.Core` has zero dependencies on WPF or any UI framework, so it can be driven entirely headlessly — from a console app, a test suite, or any other .NET front end.

```csharp
using AgentSim.Core.Simulation;

var settings = new SimulationSettings { AgentCount = 30, WorldWidth = 100, WorldHeight = 100, Seed = 42 };
var engine = new SimulationEngine(settings);
engine.Setup();

for (int i = 0; i < 100; i++)
    engine.Tick();

Console.WriteLine($"Final agent count: {engine.Worlds.Agents.Count}");
```

See `AgentSim.ConsoleDemo/Program.cs` for a full working example covering headless simulation, Roslyn scripting, and Monte Carlo batch runs with statistical summaries — all with zero UI involvement.

### Running the console demo

```powershell
dotnet run --project AgentSim.ConsoleDemo
```

## 👥 Team

Built by a 7-person team split across simulation engine, rendering, UI/control panel, scripting, and QA/DevOps.

## 🌿 Branching

- `master` — stable, the current release
- `dev` — integration branch for ongoing bug fixes; merges into `master` once changes are verified
- `feature/*` / `bugfix/*` — individual work branches, merged via reviewed pull requests
