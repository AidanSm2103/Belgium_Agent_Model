using System;
using System.Collections.Generic;
using System.IO;
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

        // How much each trial's starting agent count may differ from the
        // configured count, in percent. 10 with 30 agents = 27 to 33.
        // Whole numbers only, so the box behaves the same in every locale.
        private int _startVariancePercent = 10;
        public int StartVariancePercent
        {
            get => _startVariancePercent;
            set { _startVariancePercent = value; OnPropertyChanged(); }
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

        // The latest completed run. The charts redraw whenever this changes.
        private MonteCarloSummary? _summary;
        public MonteCarloSummary? Summary
        {
            get => _summary;
            private set { _summary = value; OnPropertyChanged(); }
        }

        // Headline numbers shown as tiles above the detailed text.
        public bool HasResults => Summary != null;

        private string _meanFinalText = "-";
        public string MeanFinalText
        {
            get => _meanFinalText;
            private set { _meanFinalText = value; OnPropertyChanged(); }
        }

        private string _ciText = "-";
        public string CiText
        {
            get => _ciText;
            private set { _ciText = value; OnPropertyChanged(); }
        }

        private string _survivalText = "-";
        public string SurvivalText
        {
            get => _survivalText;
            private set { _survivalText = value; OnPropertyChanged(); }
        }

        private string _correlationText = "-";
        public string CorrelationText
        {
            get => _correlationText;
            private set { _correlationText = value; OnPropertyChanged(); }
        }

        private void UpdateTiles(MonteCarloSummary s)
        {
            MeanFinalText = s.FinalTotal.Mean.ToString("F1");
            CiText = $"{s.FinalTotal.CiLow:F1} to {s.FinalTotal.CiHigh:F1}";
            SurvivalText = s.SurvivalRate.Mean.ToString("P0");
            CorrelationText = s.StartFinalCorrelation.ToString("F2");
            OnPropertyChanged(nameof(HasResults));
        }

        // Raised on the UI thread after a run finishes successfully, so the
        // main window can switch to the charts tab.
        public event Action? RunCompleted;

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
            ExportCommand = new RelayCommand(Export, () => Summary != null);
        }

        private async Task RunAsync()
        {
            if (TrialCount < 1)
            {
                ResultsSummaryText = "Trial count must be at least 1.";
                return;
            }
            if (TicksPerTrial < 0)
            {
                ResultsSummaryText = "Ticks per trial can't be negative.";
                return;
            }

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
                ScriptAssignments = assignments,
                StartCountVariancePercent = Math.Clamp(StartVariancePercent, 0, 100)
            };

            bool succeeded = false;

            try
            {
                // Background thread so the UI doesn't freeze while trials run.
                var summary = await Task.Run(() => MonteCarloRunner.Run(settings));

                string scriptsLine = assignments.Count == 0
                    ? "Scripts used: none (default random walk — apply a script first to include it)"
                    : "Scripts used: " + string.Join(", ", assignments.Select(a => a.Species ?? "all agents"));

                ResultsSummaryText = BuildSummaryText(summary, scriptsLine);
                Summary = summary;
                UpdateTiles(summary);
                succeeded = true;
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

            if (succeeded) RunCompleted?.Invoke();
        }

        private static string BuildSummaryText(MonteCarloSummary s, string scriptsLine)
        {
            var lines = new List<string>
            {
                $"Trials: {s.Trials.Count}",
                scriptsLine,
                s.StartCountVariancePercent > 0
                    ? $"Start count: {s.BaseStartAgentCount} ±{s.StartCountVariancePercent:0}% " +
                      $"(actual {s.StartTotal.Min:0}–{s.StartTotal.Max:0}, mean {s.StartTotal.Mean:F1})"
                    : $"Start count: {s.BaseStartAgentCount} (no variance)",
                "",
                "Final agent count",
                $"  Mean {s.FinalTotal.Mean:F2}   Median {s.FinalTotal.Median:F1}",
                $"  StdDev {s.FinalTotal.StdDev:F2}   Std error {s.FinalTotal.StdError:F2}",
                $"  95% CI [{s.FinalTotal.CiLow:F2}, {s.FinalTotal.CiHigh:F2}]",
                $"  Min {s.MinFinalAgentCount}   Max {s.MaxFinalAgentCount}"
            };

            lines.Add("");
            lines.Add($"{s.PrimarySpecies} final mean: {s.FinalPrimary.Mean:F2} (±{s.FinalPrimary.StdDev:F2})");
            if (s.HasSecondarySpecies)
            {
                lines.Add($"{s.SecondarySpecies} final mean: {s.FinalSecondary.Mean:F2} (±{s.FinalSecondary.StdDev:F2})");
            }

            lines.Add("");
            lines.Add($"Mean survival rate: {s.SurvivalRate.Mean:P1}");
            lines.Add($"Start vs final correlation: {s.StartFinalCorrelation:F2}");
            lines.Add($"Mean final patch value: {s.FinalMeanPatchValue.Mean:F1}");

            return string.Join("\n", lines);
        }

        private void Export()
        {
            if (Summary == null) return;

            var dialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                DefaultExt = ".csv",
                FileName = "montecarlo_results.csv"
            };

            if (dialog.ShowDialog() != true) return;

            // Trials go to the chosen file; the statistics go to a second
            // file beside it, named <name>_summary.csv.
            string summaryPath = Path.Combine(
                Path.GetDirectoryName(dialog.FileName) ?? "",
                Path.GetFileNameWithoutExtension(dialog.FileName) + "_summary.csv");

            try
            {
                ResultsExporter.ExportToCsv(Summary, dialog.FileName);
                ResultsExporter.ExportSummaryToCsv(Summary, summaryPath);
            }
            catch (Exception ex)
            {
                ResultsSummaryText = "Export failed: " + ex.Message;
            }
        }
    }
}
