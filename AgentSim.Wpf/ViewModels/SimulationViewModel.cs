using AgentSim.Core.Persistence;
using AgentSim.Core.Simulation;
using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace AgentSim.Wpf.ViewModels
{
    public class SimulationViewModel : ViewModelBase
    {
        private readonly DispatcherTimer _timer;
        private bool _isRunning;
        private RulesEditorViewModel? _rulesEditor;

        public SimulationEngine Engine { get; }

        public int TickCount => Engine.TickCount;

        private int _agentCount = 100;
        public int AgentCount
        {
            get => _agentCount;
            set
            {
                _agentCount = value;
                OnPropertyChanged();
            }
        }

        // Multi-species group configuration — applied to Engine.Settings
        // on the next Setup(). SecondaryGroupCount = 0 means every agent
        // belongs to PrimaryGroupSpecies (single-species, unchanged from before).
        private int _secondaryGroupCount = 0;
        public int SecondaryGroupCount
        {
            get => _secondaryGroupCount;
            set { _secondaryGroupCount = value; OnPropertyChanged(); }
        }

        private string _primaryGroupSpecies = "GroupA";
        public string PrimaryGroupSpecies
        {
            get => _primaryGroupSpecies;
            set { _primaryGroupSpecies = value; OnPropertyChanged(); }
        }

        private string _secondaryGroupSpecies = "GroupB";
        public string SecondaryGroupSpecies
        {
            get => _secondaryGroupSpecies;
            set { _secondaryGroupSpecies = value; OnPropertyChanged(); }
        }

        public ICommand SetupCommand { get; }
        public ICommand StepCommand { get; }
        public ICommand GoCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }

        public void SetRulesEditor(RulesEditorViewModel rulesEditor)
        {
            _rulesEditor = rulesEditor;
        }

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

            SaveCommand = new RelayCommand(Save);
            LoadCommand = new RelayCommand(Load);
        }

        private void Setup()
        {
            Engine.Settings.AgentCount = AgentCount;
            Engine.Settings.SecondaryGroupCount = SecondaryGroupCount;
            Engine.Settings.PrimaryGroupSpecies = PrimaryGroupSpecies;
            Engine.Settings.SecondaryGroupSpecies = SecondaryGroupSpecies;

            Engine.Setup();

            OnPropertyChanged(nameof(TickCount));

            // The rules editor's species dropdown was built from whatever
            // group settings existed when it was constructed — refresh it
            // now that Setup() may have changed them.
            _rulesEditor?.RefreshTargetsCommand.Execute(null);
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

        private void Save()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                DefaultExt = ".json",
                FileName = "simulation.json"
            };

            if (dialog.ShowDialog() != true)
                return;

            var data = new SimulationSaveData
            {
                AgentCount = Engine.Settings.AgentCount,
                WorldWidth = Engine.Settings.WorldWidth,
                WorldHeight = Engine.Settings.WorldHeight,
                StepSize = Engine.Settings.StepSize,
                MaxTurnDegrees = Engine.Settings.MaxTurnDegrees,
                Seed = Engine.Settings.Seed,
                SecondaryGroupCount = Engine.Settings.SecondaryGroupCount,
                PrimaryGroupSpecies = Engine.Settings.PrimaryGroupSpecies,
                SecondaryGroupSpecies = Engine.Settings.SecondaryGroupSpecies,
                LastAppliedScript = _rulesEditor?.ScriptText
            };

            try
            {
                SimulationPersistence.Save(data, dialog.FileName);

                MessageBox.Show(
                    "Simulation saved successfully.",
                    "Save Simulation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not save the simulation.\n\n{ex.Message}",
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void Load()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                DefaultExt = ".json"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                var data = SimulationPersistence.Load(dialog.FileName);

                if (data == null)
                {
                    MessageBox.Show(
                        "The selected save file is missing or corrupted.",
                        "Load Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    return;
                }

                Engine.Settings.AgentCount = data.AgentCount;
                Engine.Settings.WorldWidth = data.WorldWidth;
                Engine.Settings.WorldHeight = data.WorldHeight;
                Engine.Settings.StepSize = data.StepSize;
                Engine.Settings.MaxTurnDegrees = data.MaxTurnDegrees;
                Engine.Settings.Seed = data.Seed;
                Engine.Settings.SecondaryGroupCount = data.SecondaryGroupCount;
                Engine.Settings.PrimaryGroupSpecies = data.PrimaryGroupSpecies;
                Engine.Settings.SecondaryGroupSpecies = data.SecondaryGroupSpecies;

                AgentCount = data.AgentCount;
                SecondaryGroupCount = data.SecondaryGroupCount;
                PrimaryGroupSpecies = data.PrimaryGroupSpecies;
                SecondaryGroupSpecies = data.SecondaryGroupSpecies;

                if (_rulesEditor != null && data.LastAppliedScript != null)
                {
                    _rulesEditor.ScriptText = data.LastAppliedScript;
                }

                Engine.Setup();
                _rulesEditor?.RefreshTargetsCommand.Execute(null);

                MessageBox.Show(
                    "Simulation loaded successfully.",
                    "Load Simulation",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Could not load the simulation.\n\n{ex.Message}",
                    "Load Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}