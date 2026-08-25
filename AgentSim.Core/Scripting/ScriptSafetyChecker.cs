using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.Scripting
{
    /// <summary>
    /// A lightweight denylist check run before compiling a user script.
    ///
    /// IMPORTANT: this is NOT a real security sandbox — a determined user
    /// could bypass it (string tricks, reflection, etc.). It exists to catch
    /// obvious accidental misuse (someone pasting file/process code without
    /// thinking), not to defend against deliberately malicious input. True
    /// isolation would require running scripts in a separate, restricted
    /// process, which is out of scope for this project.
    /// </summary>
    public static class ScriptSafetyChecker
    {
        private static readonly string[] BlockedTerms =
        {
        "System.IO",
        "System.Net",
        "System.Diagnostics",
        "System.Reflection",
        "Process.Start",
        "File.",
        "Directory.",
        "Environment.Exit",
        "AppDomain",
        "Assembly.Load",
        "unsafe"
    };

        private const int MaxScriptLength = 2000;

        /// <summary>
        /// Returns true if the script passes basic checks. On failure,
        /// 'reason' explains why — safe to show directly to the user.
        /// </summary>
        public static bool TryValidate(string userCode, out string? reason)
        {
            if (string.IsNullOrWhiteSpace(userCode))
            {
                reason = "Script is empty.";
                return false;
            }

            if (userCode.Length > MaxScriptLength)
            {
                reason = $"Script is too long (max {MaxScriptLength} characters).";
                return false;
            }

            foreach (var term in BlockedTerms)
            {
                if (userCode.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    reason = $"Script uses a blocked term: '{term}'. " +
                              "File, network, process, and reflection access aren't allowed in agent behaviors.";
                    return false;
                }
            }

            reason = null;
            return true;
        }
    }
}
