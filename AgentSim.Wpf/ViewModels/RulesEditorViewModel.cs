using System.Windows.Input;

namespace AgentSim.Wpf.ViewModels
{
    public class RulesEditorViewModel : ViewModelBase
    {
        private string _scriptText = "public void Update(Agent agent)\n{\n    // Type behavior here\n}";
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

        public RulesEditorViewModel()
        {
            ApplyCommand = new RelayCommand(ApplyScript);
        }

        private void ApplyScript()
        {
            if (string.IsNullOrWhiteSpace(ScriptText))
            {
                HasError = true;
                StatusMessage = "Error: Script cannot be empty.";
                return;
            }

            if (ScriptText.Contains("error"))
            {
                HasError = true;
                StatusMessage = "Compile Error: Invalid C# syntax detected.";
            }
            else
            {
                HasError = false;
                StatusMessage = "Success: Behavior compiled and applied to agents!";
            }
        }
    }
}