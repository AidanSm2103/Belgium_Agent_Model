using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AgentSim.Core.Analysis;
using AgentSim.Core.Scripting;
using AgentSim.Core.Simulation;
using Microsoft.Win32;

namespace AgentSim.Wpf.ViewModels
{
    public class MonteCarloViewModel : ViewModelBase
    {
        private readonly Func<SimulationSettings> _getBaseSettings;
        private readonly Func<IReadOnlyList<ScriptAssignment>> _getScriptAssignments;

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

        // Takes delegates rather than a direct SimulationEngine reference,
        // so each Monte Carlo trial gets its own fresh engine (same settings
        // and the same applied scripts as the simulation on screen) without
        // ever touching the live one.
        public MonteCarloViewModel(
            Func<SimulationSettings> getBaseSettings,
            Func<IReadOnlyList<ScriptAssignment>> getScriptAssignments)
        {
            _getBaseSettings = getBaseSettings;
            _getScriptAssignments = getScriptAssignments;
            RunCommand = new RelayCommand(async () => await RunAsync(), () => !IsRunning);
            ExportCommand = new RelayCommand(Export, () => _lastSummary != null);
        }

        private async Task RunAsync()
        {
            IsRunning = true;

            // Copy the assignments now: the list on the live engine can change
            // while the background run is still going.
            var assignments = _getScriptAssignments().ToList();

            ResultsSummaryText = "Running...";

            var settings = new MonteCarloSettings
            {
                TrialCount = TrialCount,
                TicksPerTrial = TicksPerTrial,
                BaseSettings = _getBaseSettings(),
                ScriptAssignments = assignments
            };

            try
            {
                // Background thread so the UI doesn't freeze while trials run.
                var summary = await Task.Run(() => MonteCarloRunner.Run(settings));

                _lastSummary = summary;

                string scriptsLine = assignments.Count == 0
                    ? "Scripts used: none (default random walk — apply a script first to include it)"
                    : "Scripts used: " + string.Join(", ", assignments.Select(a => a.Species ?? "all agents"));

                ResultsSummaryText =
                    $"Trials: {summary.Trials.Count}\n" +
                    $"{scriptsLine}\n" +
                    $"Mean final agent count: {summary.MeanFinalAgentCount:F2}\n" +
                    $"StdDev: {summary.StdDevFinalAgentCount:F2}\n" +
                    $"Min: {summary.MinFinalAgentCount}   Max: {summary.MaxFinalAgentCount}";
            }
            catch (Exception ex)
            {
                ResultsSummaryText = "Monte Carlo run failed: " + ex.Message;
            }
            finally
            {
                // Always re-enable the Run button, even if the run threw.
                IsRunning = false;
            }
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