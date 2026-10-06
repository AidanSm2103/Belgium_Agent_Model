using System;
using System.Threading.Tasks;
using System.Windows.Input;
using AgentSim.Core.Analysis;
using Microsoft.Win32;

namespace AgentSim.Wpf.ViewModels
{
    public class MonteCarloViewModel : ViewModelBase
    {
        private readonly Func<AgentSim.Core.Simulation.SimulationSettings> _getBaseSettings;

        private int _trialCount = 30;
        public int TrialCount
        {
            get => _trialCount;
            set { _trialCount = value; OnPropertyChanged(); }
        }

        private int _ticksPerTrial = 200;
        public int TicksPerTrial
        {
            get => _ticksPerTrial;
            set { _ticksPerTrial = value; OnPropertyChanged(); }
        }

        private string _resultsSummaryText = "No run yet.";
        public string ResultsSummaryText
        {
            get => _resultsSummaryText;
            set { _resultsSummaryText = value; OnPropertyChanged(); }
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set { _isRunning = value; OnPropertyChanged(); }
        }

        private MonteCarloSummary? _lastSummary;

        public ICommand RunCommand { get; }
        public ICommand ExportCommand { get; }

        // Takes a delegate rather than a direct SimulationEngine reference,
        // so each Monte Carlo trial gets its own fresh engine/settings
        // (same AgentCount/WorldWidth/etc. as the main simulation, but
        // never touches the live one on screen).
        public MonteCarloViewModel(Func<AgentSim.Core.Simulation.SimulationSettings> getBaseSettings)
        {
            _getBaseSettings = getBaseSettings;
            RunCommand = new RelayCommand(async () => await RunAsync(), () => !IsRunning);
            ExportCommand = new RelayCommand(Export, () => _lastSummary != null);
        }

        private async Task RunAsync()
        {
            IsRunning = true;
            ResultsSummaryText = "Running...";

            var settings = new MonteCarloSettings
            {
                TrialCount = TrialCount,
                TicksPerTrial = TicksPerTrial,
                BaseSettings = _getBaseSettings()
            };

            // Run on a background thread so the UI doesn't freeze while
            // many trials execute.
            var summary = await Task.Run(() => MonteCarloRunner.Run(settings));

            _lastSummary = summary;
            ResultsSummaryText =
                $"Trials: {summary.Trials.Count}\n" +
                $"Mean final agent count: {summary.MeanFinalAgentCount:F2}\n" +
                $"StdDev: {summary.StdDevFinalAgentCount:F2}\n" +
                $"Min: {summary.MinFinalAgentCount}   Max: {summary.MaxFinalAgentCount}";

            IsRunning = false;
        }

        private void Export()
        {
            if (_lastSummary == null) return;

            var dialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                DefaultExt = ".csv",
                FileName = "montecarlo_results.csv"
            };

            if (dialog.ShowDialog() != true) return;

            ResultsExporter.ExportToCsv(_lastSummary, dialog.FileName);
        }
    }
}
