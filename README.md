# Belgium Agent Model (BAM)

A C#/WPF agent-based modelling application inspired by NetLogo, built as a year-long university group project. The long-term goal is a general-purpose ABM tool where users can define custom agent behaviors; the current milestone is a working MVP demonstrating the core simulation loop end-to-end.

## Current status: MVP

- Agents spawn at random positions and wander the world using a hardcoded random-walk behavior
- Simulation space wraps at the edges (torus topology, same default as NetLogo)
- Full control loop working: **Setup** (spawn fresh agents), **Step** (advance one tick), **Go/Stop** (run continuously)
- Agent count adjustable via slider, applied on next Setup
- Live tick counter
- Real-time rendering of agent positions on a WPF canvas
- Unit test coverage for the simulation engine, world, agents, and random number provider

**Not yet implemented** (planned for later milestones): user-editable agent behaviors/scripting, patch-based environment features, plots and additional monitors, save/load.

## Architecture

The solution is split into three projects to keep the simulation engine reusable and independent of any specific UI:

```
AgentSim.Core/          Simulation engine — no UI dependencies
├── Agents/              Agent, IAgentBehavior, RandomWalkBehavior
├── World/                World, Patch
├── Simulation/          SimulationEngine, SimulationSettings
└── Utilities/            RandomProvider (seeded RNG)

AgentSim.Wpf/            WPF front end (MVVM)
├── Views/                MainWindow, WorldCanvasControl
├── ViewModels/           MainViewModel, SimulationViewModel
└── Helpers/               RelayCommand, ViewModelBase

AgentSim.Core.Tests/      xUnit tests for the engine
```

`AgentSim.Core` has no reference to WPF or any UI framework — it can be driven headlessly (e.g. from a console app or test suite), which is what makes it a reusable library rather than logic baked into the UI.

## Running the project

1. Open `AgentSim.sln` in Visual Studio 2022+ (requires the **.NET desktop development** workload)
2. Set `AgentSim.Wpf` as the Startup Project
3. Press F5

## Running the tests

Open **Test Explorer** (Test menu → Test Explorer) and run all tests, or `Test → Run All Tests`.

## Team

Built by a 7-person team split across five areas: simulation engine, rendering, UI/control panel, scripting (upcoming), and QA/DevOps.

## Branching

- `master` — stable, always in a working/presentable state
- `feature/*` — individual work branches, merged via reviewed pull requests
