using System.Threading.Tasks;
using System.Windows;
using AgentSim.Core.Scripting;

namespace AgentSim.Wpf
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Roslyn is slow the first time it is used (loading, JIT). Do that
            // work in the background while the window opens, so the user's
            // first Apply isn't the one that pays for it.
            Task.Run(() =>
            {
                try
                {
                    var warm = BehaviorCompiler.Compile("Agent.Heading += Rng.NextDouble();");
                    if (warm.Success && warm.Behavior != null) BehaviorTestRunner.TryDryRun(warm.Behavior, out _);
                }
                catch { /* warm-up is best effort */ }
            });
        }
    }
}

