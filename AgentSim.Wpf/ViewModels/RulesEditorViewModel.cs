using System.Collections.ObjectModel;
using System.Windows.Input;
using AgentSim.Core.Agents;
using AgentSim.Core.Scripting;
using AgentSim.Core.Simulation;

namespace AgentSim.Wpf.ViewModels
{
    public class RulesEditorViewModel : ViewModelBase
    {
        private readonly SimulationEngine _engine;
        private string _scriptText = ScriptTemplates.RandomWalk;
        private string _statusMessage = "";
        private bool _hasError;

        public string ScriptText
        {
            get => _scriptText;
            set { _scriptText = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool HasError
        {
            get => _hasError;
            set { _hasError = value; OnPropertyChanged(); }
        }

        // "All" applies to every agent (the original behavior). Any other
        // value applies only to agents whose Species matches it.
        public ObservableCollection<string> AvailableTargets { get; } = new();

        private string _targetSpecies = "All";
        public string TargetSpecies
        {
            get => _targetSpecies;
            set { _targetSpecies = value; OnPropertyChanged(); }
        }

        public ICommand ApplyCommand { get; }
        public ICommand RefreshTargetsCommand { get; }

        public RulesEditorViewModel(SimulationEngine engine)
        {
            _engine = engine;
            ApplyCommand = new RelayCommand(ApplyScript);
            RefreshTargetsCommand = new RelayCommand(RefreshTargets);

            RefreshTargets();
        }

        // Rebuilds the dropdown from the engine's current species settings.
        // Call this after Setup() runs with new group settings, since the
        // list was built from whatever Settings looked like at the time —
        // it doesn't update itself automatically.
        private void RefreshTargets()
        {
            var selected = TargetSpecies;

            AvailableTargets.Clear();
            AvailableTargets.Add("All");
            AvailableTargets.Add(_engine.Settings.PrimaryGroupSpecies);

            if (_engine.Settings.SecondaryGroupCount > 0)
            {
                AvailableTargets.Add(_engine.Settings.SecondaryGroupSpecies);
            }

            // Keep the previous selection if it's still valid, otherwise
            // fall back to "All" rather than an orphaned value.
            TargetSpecies = AvailableTargets.Contains(selected) ? selected : "All";
        }

        private void ApplyScript()
        {
            // "All" -> null (every agent). Otherwise the species name. Going
            // through ApplyScriptToSpecies also records the assignment on the
            // engine, which is what Monte Carlo and save/load read from.
            string? target = TargetSpecies == "All" ? null : TargetSpecies;

            var result = ScriptApplier.ApplyScriptToSpecies(ScriptText, _engine, target);

            if (result.Success)
            {
                HasError = false;
                StatusMessage = TargetSpecies == "All"
                    ? "Success: Behavior compiled and applied to all agents!"
                    : $"Success: Behavior compiled and applied to '{TargetSpecies}' agents!";
            }
            else
            {
                HasError = true;
                StatusMessage = $"Error: {result.ErrorMessage}";
            }
        }
    }
}