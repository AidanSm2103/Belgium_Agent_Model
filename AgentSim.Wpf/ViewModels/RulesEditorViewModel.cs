using System.Windows.Input;
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

        public ICommand ApplyCommand { get; }

        public RulesEditorViewModel(SimulationEngine engine)
        {
            _engine = engine;
            ApplyCommand = new RelayCommand(ApplyScript);
        }

        private void ApplyScript()
        {
            var result = ScriptApplier.ApplyScript(ScriptText, _engine);
            if (result.Success)
            {
                HasError = false;
                StatusMessage = "Success: Behavior compiled and applied to agents!";
            }
            else
            {
                HasError = true;
                StatusMessage = $"Error: {result.ErrorMessage}";
            }
        }
    }
}
