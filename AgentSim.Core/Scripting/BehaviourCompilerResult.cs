
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSim.Core.Agents;

namespace AgentSim.Core.Scripting
{
    // <summary>
    /// Outcome of compiling a user script. Check Success before using Behavior.
    /// The UI's rules editor should display ErrorMessage when Success is false.
    /// </summary>
    public class BehaviorCompileResult
    {
        public bool Success { get; init; }
        public IAgentBehavior? Behavior { get; init; }
        public string? ErrorMessage { get; init; }

        public static BehaviorCompileResult Ok(IAgentBehavior behavior) =>
            new() { Success = true, Behavior = behavior };

        public static BehaviorCompileResult Fail(string message) =>
            new() { Success = false, ErrorMessage = message };
    }
}
