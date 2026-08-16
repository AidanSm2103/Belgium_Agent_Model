using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSim.Core.Agents;
using AgentSim.Core.Utilities;
using AgentSim.Core.Worlds;

namespace AgentSim.Core.Scripting
{
    // <summary>
    /// The variables a user's script sees as top-level names (Agent, World, Rng)
    /// when it runs. Roslyn's scripting API calls this a "globals" type.
    /// </summary>
    public class ScriptGlobals
    {
        public Agent Agent { get; set; } = null!;
        public World World { get; set; } = null!;
        public RandomProvider Rng { get; set; } = null!;
    }
}
