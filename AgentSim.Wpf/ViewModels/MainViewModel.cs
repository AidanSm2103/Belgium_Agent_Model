using AgentSim.Core;
using System;

namespace AgentSim.Wpf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public SimulationViewModel Simulation { get; }
        public RulesEditorViewModel RulesEditor { get; }

        public MainViewModel()
        {
            Simulation = new SimulationViewModel();
            RulesEditor = new RulesEditorViewModel(Simulation.Engine);

            Simulation.SetRulesEditor(RulesEditor);
        }
    }
}