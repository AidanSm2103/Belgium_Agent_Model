using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Seedable random number wrapper so simulation runs are reproducible.
// Pass a seed for repeatable test runs, leave it null for a fresh random sequence each time (used by default UI runs).

namespace AgentSim.Core.Utilities
{
    public class RandomProvider
    {
        private readonly Random _random;

        public RandomProvider(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        // Returns a random double in [0.0, 1.0)
        public double NextDouble() => _random.NextDouble();

        // Returns a random int in [minInclusive, maxExclusive).
        public int Next(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);

        // Returns a random double in [minInclusive, maxExclusive)
        public double NextDouble(double minInclusive, double maxExclusive) => minInclusive + (_random.NextDouble() * (maxExclusive - minInclusive));
    }
}
