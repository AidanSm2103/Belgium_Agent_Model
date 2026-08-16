using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.Scripting
{
    // Outcome of ScriptApplier.ApplyScript(). Covers all three ways a
    // script can fail (safety check, compile error, dry-run failure) with
    // one consistent shape the UI can check.
    public class ScriptApplyResult
    {
        public bool Success { get; init; }
        public string? ErrorMessage { get; init; }

        public static ScriptApplyResult Ok() => new() { Success = true };
        public static ScriptApplyResult Fail(string message) => new() { Success = false, ErrorMessage = message };
    }
}
