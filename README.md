<div align="center">

<img src="https://img.shields.io/badge/BAM-Belgium%20Agent%20Model-FDDA24?style=for-the-badge&labelColor=0C0F14" alt="BAM"/>

# Belgium Agent Model (BAM)

**A C# / WPF agent-based modelling studio inspired by NetLogo**
*Write agent behaviour at runtime, watch species interact, and test it statistically.*

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![WPF](https://img.shields.io/badge/UI-WPF-0078D7?logo=windows&logoColor=white)
![Roslyn](https://img.shields.io/badge/Scripting-Roslyn-68217A?logo=csharp&logoColor=white)
![Tests](https://img.shields.io/badge/tests-xUnit-25A162)
![Status](https://img.shields.io/badge/status-Final%20Release-brightgreen)

</div>

---

> Year-long university group project (Belgium Campus ITversity, Module: PRJ381). A general-purpose agent-based modelling tool where users define their own agent behaviour at runtime, run statistical batch analysis and observe multiple interacting agent types. Built as a **reusable C# library** with a **WPF front end**.

## 📍 Status

Development is **feature-complete**. 

| Area | Status |
|---|:---:|
| Core simulation loop (Setup / Step / Go) | ✅ |
| Torus-wrapped world (agents leaving one edge appear on the opposite one) | ✅ |
| Real-time rendering | ✅ |
| Runtime C# scripting (Roslyn) with safety checks and a per-agent time limit | ✅ |
| In-app rules editor with an expandable full-size view | ✅ |
| Configurable patch grid (size, starting value, regrowth), readable and writable from scripts | ✅ |
| Live plot and monitors (ticks, agent count, species breakdown, patch health) | ✅ |
| Safe runtime spawn / kill of agents from scripts | ✅ |
| Multi-species agents with per-species script targeting | ✅ |
| Monte Carlo batch runs: statistics, start-count variance, per-species results, charts, CSV export | ✅ |
| Themed interface (Belgian tricolour dark theme), including themed pop-up messages | ✅ |
| Save / load of the full simulation | ✅ |
| Performance pass (fast script execution, parallel Monte Carlo, single-layer world drawing) | ✅ |
| Reusable library packaging and standalone console demo | ✅ |

## ✨ Features

### 🧠 Scripting
- **Write behaviour without recompiling.** The rules editor compiles a C# snippet with Roslyn and applies it live, with clear compile-error feedback. 
- **Safe by design.** A denylist blocks file, network and process access, a first-run test catches broken or hanging scripts before they touch real agents and an agent whose script runs far too long is switched off rather than freezing the simulation.
- **Scripts can change the world.** Read and write patch values, look at other agents and spawn or remove agents through `Engine.QueueSpawn` and `Engine.QueueKill`.

### 🐑🐺 Multiple species
Split agents into two named species groups at setup, then apply a script to *All* agents or to a single species. The **Apply to** list shows the species names you currently have set and the refresh button next to it updates the list.

### 🌱 A real environment
A patch grid agents and scripts can read and write, drawn live behind the agents. Grid size, starting value and regrowth time are configurable. An emptied patch can regrow after a set number of ticks or never.

### 📊 Statistics
- A **Monte Carlo** panel runs many independent trials with different seeds, using the same settings and scripts as the simulation on screen. Trials run **in parallel** across your CPU cores, and each trial's seed is still derived from the base seed, so results are reproducible.
- **Start-count variance** makes each trial begin with a slightly different population (for example 10% of 30 gives 27 to 33), varied per species and reproducible from the seed.
- Reports mean, median, standard deviation, standard error, 95% confidence interval and min/max of the final population, plus per-species totals, survival rate, mean final patch value and the start-versus-final correlation.
- A **Monte Carlo Charts** tab: final population per trial, histogram of final populations, start versus final per trial and mean start versus mean final per species with 95% confidence intervals.
- **CSV export** writes one row per trial plus a `_summary.csv` of the statistics beside it. 

### 💾 Save and resume
A save file captures the whole simulation: settings, species groups, every agent's position and behaviour, patch values, the tick count and the scripts you applied. Loading puts you back where you left off with the scripts still active. Older settings-only files still load and start a fresh Setup.

### 🎨 Finished look
A dark theme built on the Belgian tricolour, with a status header, card-based side panel and shared species colours across the world, the live plot and the Monte Carlo charts. Save and load messages, errors and warnings use themed pop-ups instead of the standard Windows message box.

### ⚡ Performance
Built to stay smooth with large populations:
- Scripts are compiled once into a delegate and run directly on every tick. The thread-plus-timeout guard is used only for a script's first test run.
- The world is drawn into a single image per tick rather than thousands of separate shapes.
- Roslyn is warmed up in the background at startup, so the first **Apply** is not the slow one.
- Monte Carlo trials run in parallel.

### 📦 Genuinely reusable
`AgentSim.Core` has zero UI dependencies and ships with a standalone console demo that runs the simulation, scripting and Monte Carlo end to end with no UI.

## 🚀 Quick start

1. Open `AgentSim.Core.sln` in **Visual Studio 2022 or later** (the *.NET desktop development* workload is required).
2. Set `AgentSim.Wpf` as the **Startup Project**.
3. Press **F5**.

## 🕹️ Using the app

1. Click **Setup** to spawn agents. Optionally configure a second species group first (species names and secondary count under **Population**) and the patch grid (size, starting value, regrowth under **Environment**). Both are applied on Setup.
2. In the rules editor, write a script using `Agent`, `World`, `Rng` and `Engine`. Use the expand button for a larger view.
3. Choose who the script applies to under **Apply to** (*All* or a species), then click **Apply behavior**.
4. Click **Go** to run continuously or **Step** to advance one tick. Ticks, agent count, species breakdown, patch health and the live plot update as it runs.
5. Use **Save** / **Load** to store or restore the whole simulation.
6. Open the **Monte Carlo** panel, set the trial count, ticks per trial and start-count variance, then run. Statistics appear in the panel, charts open in the **Monte Carlo Charts** tab, and **CSV** exports the data.

**Script notes**
- A script is limited to 2000 characters.
- Always move agents with `World.Wrap(...)` so they stay in the world.
- Distance checks in a script do not account for the world wrapping at the edges, so two agents on opposite edges count as far apart.
- Neighbour scans over `World.Agents` cost more as the population grows. Around a few hundred agents per species stays smooth.

## 🏗️ Architecture

Four projects keep the simulation engine reusable and independent of any UI:

```
AgentSim.Core/              Simulation engine, no UI dependencies
├── Agents/                   Agent, IAgentBehavior, RandomWalkBehavior
├── World/                    World, Patch
├── Simulation/               SimulationEngine, SimulationSettings
├── Scripting/                BehaviourCompiler, ScriptedBehaviour, ScriptGlobals,
│                             ScriptSafetyChecker, ScriptApplier, ScriptApplyResult,
│                             ScriptTemplates, BehaviourTestRunner
├── Analysis/                 MonteCarloSettings, MonteCarloResult, MonteCarloRunner,
│                             StatisticsHelper, ResultsExporter
├── Persistence/              SimulationSaveData, SimulationSnapshot, SimulationPersistence
└── Utilities/                RandomProvider (seeded RNG)

AgentSim.Wpf/               WPF front end (MVVM)
├── Themes/                   Theme.xaml (colours, control styles)
├── Views/                    MainWindow, WorldCanvasControl, RulesEditorControl,
│                             PlotControl, MonteCarloControl, MonteCarloChartsControl,
│                             ChartDrawing, SpeciesColors, NonTopmostPopup,
│                             Dialogs, ThemedMessageWindow
├── ViewModels/               MainViewModel, SimulationViewModel,
│                             RulesEditorViewModel, MonteCarloViewModel
└── Helpers/                  RelayCommand, ViewModelBase

AgentSim.Core.Tests/        xUnit tests: engine, scripting, Monte Carlo,
                            multi-species support and persistence

AgentSim.ConsoleDemo/       Standalone console app proving AgentSim.Core runs
                            with zero UI involvement
```

`AgentSim.Core` has no reference to WPF or any UI framework. It can be driven headlessly, which is what makes it a reusable library rather than logic baked into the UI.

> **A note on script safety:** the safety checks (denylist, first-run test and time limit) are a practical safeguard against accidental slow or unsafe scripts, not a hard security sandbox. Real isolation from deliberately malicious code would need a separate process, which is out of scope for this project.

## 🧪 Running the tests

Open **Test Explorer** (Test menu → Test Explorer) and choose **Run All Tests**. Coverage includes the engine, world, agents, random number provider, the scripting pipeline (compile, safety checks, timeout handling, apply), Monte Carlo statistics, start-count variance, CSV export and determinism, multi-species targeting, patch configuration and regrowth, and save/load persistence.

## 📦 Using AgentSim.Core as a library

`AgentSim.Core` has no WPF dependency, so it can be driven from a console app, a test suite or any other .NET front end.

> See `AgentSim.ConsoleDemo/Program.cs` for a full example covering a headless simulation, Roslyn scripting and Monte Carlo batch runs with statistical summaries.

## 👥 Team

Built by a 6-person team covering the simulation engine, rendering, UI and control panel, scripting, Monte Carlo analysis and QA/DevOps.
