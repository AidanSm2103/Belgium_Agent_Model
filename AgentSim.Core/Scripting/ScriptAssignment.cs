using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.Scripting
{
   
        // Records that a script was applied: to every agent (Species == null) or to one species only
        public record ScriptAssignment(string? Species, string Source);
    
}
