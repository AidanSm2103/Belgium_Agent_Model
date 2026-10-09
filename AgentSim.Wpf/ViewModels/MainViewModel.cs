using AgentSim.Core;
using System;
using System.Linq;

namespace AgentSim.Wpf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public SimulationViewModel Simulation { get; }
        public RulesEditorViewModel RulesEditor { get; }
        public MonteCarloViewModel MonteCarlo { get; }

        public MainViewModel()
        {
            Simulation = new SimulationViewModel();
            RulesEditor = new RulesEditorViewModel(Simulation.Engine);

            Simulation.SetRulesEditor(RulesEditor);

            // Monte Carlo runs its own separate engines internally. It is
            // handed the current settings AND the scripts applied so far, so
            // the trials behave like the simulation on screen instead of
            // always running the default random walk.
            MonteCarlo = new MonteCarloViewModel(
                () => Simulation.Engine.Settings,
                () => Simulation.Engine.ScriptAssignments.ToList());
        }
    }
}