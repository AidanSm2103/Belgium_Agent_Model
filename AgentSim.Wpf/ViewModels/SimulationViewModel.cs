using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AgentSim.Wpf.ViewModels
{
    internal class SimulationViewModel : ViewModelBase
    {
        private int _tickCount;
        private int _agentCount = 100; 
        private bool _isRunning;

        public int TickCount
        {
            get => _tickCount;
            set { _tickCount = value; OnPropertyChanged(); }
        }

        public int AgentCount
        {
            get => _agentCount;
            set { _agentCount = value; OnPropertyChanged(); }
        }

        public ICommand SetupCommand { get; }
        public ICommand StepCommand { get; }
        public ICommand GoCommand { get; }

        public SimulationViewModel()
        {
            SetupCommand = new RelayCommand(Setup, () => !_isRunning);
            StepCommand = new RelayCommand(Step, () => !_isRunning);
            GoCommand = new RelayCommand(ToggleGo);
        }

        private void Setup()
        {
            TickCount = 0;
        }

        private void Step()
        {
            TickCount++;
        }

        private void ToggleGo()
        {
            _isRunning = !_isRunning;
        }
    }
}