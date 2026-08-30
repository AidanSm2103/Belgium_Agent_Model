using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSim.Core; 

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
        }
    }
}