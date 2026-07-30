using AgentSim.Core.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace AgentSim.Wpf.ViewModels
{
    internal class SimulationViewModel : ViewModelBase
    {
        private readonly DispatcherTimer _timer;
        private bool _isRunning;

        public SimulationEngine Engine { get; }

        public int TickCount => Engine.TickCount;

        private int _agentCount = 100;
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
            var settings = new SimulationSettings
            {
                AgentCount = _agentCount,
                WorldWidth = 400,
                WorldHeight = 400,
                StepSize = 3.0,
                MaxTurnDegrees = 25
            };

            Engine = new SimulationEngine(settings);
            Engine.Ticked += (s, e) => OnPropertyChanged(nameof(TickCount));

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _timer.Tick += (s, e) => Engine.Tick();

            SetupCommand = new RelayCommand(Setup, () => !_isRunning);
            StepCommand = new RelayCommand(Step, () => !_isRunning);
            GoCommand = new RelayCommand(ToggleGo);
        }

        private void Setup()
        {
            Engine.Settings.AgentCount = AgentCount;
            Engine.Setup();
            OnPropertyChanged(nameof(TickCount));
        }

        private void Step()
        {
            Engine.Tick();
        }

        private void ToggleGo()
        {
            _isRunning = !_isRunning;

            if (_isRunning)
            {
                Engine.Start();
                _timer.Start();
            }
            else
            {
                Engine.Stop();
                _timer.Stop();
            }
        }
    }
}