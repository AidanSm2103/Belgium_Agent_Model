using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSim.Core.Agents;
using AgentSim.Core.Utilities;
using AgentSim.Core.Worlds;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace AgentSim.Core.Scripting
{
    /// <summary>
    /// Compiles a user-written C# snippet (the body of a behavior — see
    /// ScriptGlobals for what names it can use: Agent, World, Rng) into a
    /// live IAgentBehavior. This is the main entry point the UI's rules
    /// editor calls when the user clicks "Apply".
    ///
    /// Compile() runs ScriptSafetyChecker first, then attempts a real Roslyn
    /// compile (which catches syntax/type errors) before ever executing
    /// anything.
    /// </summary>
    public static class BehaviorCompiler
    {
        private static readonly ScriptOptions Options = ScriptOptions.Default
            .WithReferences(typeof(Agent).Assembly)
            .WithImports("System", "AgentSim.Core.Agents", "AgentSim.Core.Worlds", "AgentSim.Core.Utilities");

        public static BehaviorCompileResult Compile(string userCode)
        {
            if (!ScriptSafetyChecker.TryValidate(userCode, out var safetyReason))
            {
                return BehaviorCompileResult.Fail(safetyReason!);
            }

            try
            {
                var script = CSharpScript.Create(userCode, Options, globalsType: typeof(ScriptGlobals));

                // Compile() surfaces syntax/type errors without running anything —
                // this is what lets us report a compile error instead of throwing.
                var diagnostics = script.Compile();
                var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

                if (errors.Count > 0)
                {
                    var message = string.Join(Environment.NewLine, errors.Select(e => e.GetMessage()));
                    return BehaviorCompileResult.Fail(message);
                }

                return BehaviorCompileResult.Ok(new ScriptedBehavior(script));
            }
            catch (Exception ex)
            {
                return BehaviorCompileResult.Fail(ex.Message);
            }
        }
    }
}