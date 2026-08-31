<div align="center">

# Belgium Agent Model (BAM)

**A C#/WPF agent-based modelling application inspired by NetLogo**

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![WPF](https://img.shields.io/badge/UI-WPF-0078D7?logo=windows&logoColor=white)
![Roslyn Scripting](https://img.shields.io/badge/Scripting-Roslyn-68217A?logo=csharp&logoColor=white)
![Tests](https://img.shields.io/badge/tests-xUnit-25A162)
![Status](https://img.shields.io/badge/status-Milestone%202%20complete-brightgreen)

*A year-long university group project — building a general-purpose agent-based modelling tool where users can define their own agent behavior at runtime, no recompiling required.*

</div>

---

## 📍 Current status: Milestone 2 — Scripting Integration

Milestone 1 proved the core simulation loop. Milestone 2 makes the simulation **user-programmable**: agents can now run C# behavior scripts written and applied at runtime, on top of a real environment grid.

| Area | Status |
|---|:---:|
| Core simulation loop (Setup / Step / Go) | ✅ |
| Torus-wrapped world | ✅ |
| Real-time rendering | ✅ |
| **Runtime C# scripting (Roslyn)** | ✅ |
| **Script safety checks + execution timeout** | ✅ |
| **In-app rules editor** | ✅ |
| **Patch grid — readable/writable environment values** | ✅ |
| **Live plot + monitors (ticks, agent count)** | ✅ |
| **Agent rendering reflects active behavior** | ✅ |
| Safe runtime spawn/kill of agents from scripts | 🔜 next milestone |
| Save/load | 🔜 later |

### What's new since the MVP

- **Write and apply agent behavior without recompiling.** A built-in rules editor compiles a C# snippet via Roslyn and applies it to every agent live, with clear compile-error feedback if it doesn't build.
- **Scripts run safely.** A lightweight denylist blocks obviously unsafe operations (file/network/process access), and any script that hangs is timed out rather than freezing the simulation.
- **A real environment.** The world now has a patch grid agents and scripts can read from and write to — the foundation for environment-based behavior.
- **Live feedback.** A plot tracks agent count over time alongside tick/agent monitors, updating every tick.
- **Visual confirmation.** Agents running a custom script render in a different color than agents on the default behavior, so it's visible at a glance whether scripting actually took effect.

## 🏗️ Architecture

Three projects keep the simulation engine reusable and independent of any specific UI:

```
AgentSim.Core/            Simulation engine — no UI dependencies
├── Agents/                 Agent, IAgentBehavior, RandomWalkBehavior
├── World/                  World, Patch
├── Simulation/             SimulationEngine, SimulationSettings
├── Scripting/               BehaviorCompiler, ScriptedBehavior, ScriptGlobals,
│                            ScriptSafetyChecker, ScriptApplier, ScriptApplyResult,
│                            ScriptTemplates, BehaviorTestRunner
└── Utilities/               RandomProvider (seeded RNG)

AgentSim.Wpf/              WPF front end (MVVM)
├── Views/                   MainWindow, WorldCanvasControl, RulesEditorControl, PlotControl
├── ViewModels/               MainViewModel, SimulationViewModel, RulesEditorViewModel
└── Helpers/                  RelayCommand, ViewModelBase

AgentSim.Core.Tests/        xUnit tests for the engine and scripting subsystem
```

`AgentSim.Core` has no reference to WPF or any UI framework — it can be driven headlessly (e.g. from a console app or test suite), which is what makes it a reusable library rather than logic baked into the UI.

**A note on script safety:** the safety checks (denylist + timeout) are a practical safeguard against accidental slow or unsafe scripts, not a hard security sandbox — genuine isolation from deliberately malicious code would need a separate process, which is out of scope for this project.

## ▶️ Running the project

1. Open `AgentSim.sln` in Visual Studio 2022+ (requires the **.NET desktop development** workload)
2. Set `AgentSim.Wpf` as the Startup Project
3. Press F5

## ✍️ Trying the scripting feature

1. Click **Setup** to spawn agents
2. In the rules editor panel, try one of the built-in example scripts (Random Walk, Flee to Center, Bounce off Walls) or write your own using `Agent`, `World`, and `Rng`
3. Click **Apply** — agents running the new script render in a different color
4. Click **Go** and watch it run live, with ticks/agent count tracked in the plot below the canvas

## 🧪 Running the tests

Open **Test Explorer** (Test menu → Test Explorer) and run all tests, or `Test → Run All Tests`. Coverage includes the simulation engine, world, agents, random number provider, and the full scripting pipeline (compile, safety checks, timeout handling, apply).

## 👥 Team

Built by a 7-person team split across five areas: simulation engine, rendering, UI/control panel, scripting, and QA/DevOps.

## 🌿 Branching

- `master` — stable, always in a working/presentable state
- `dev` — integration branch for the current milestone; merges into `master` once the milestone is complete and demoed
- `feature/*` — individual work branches, merged into `dev` via reviewed pull requests

