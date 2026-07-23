using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.Utilities
{
    public class RandomProvider
    {
        private readonly Random _random;

        public RandomProvider(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public double NextDouble() => _random.NextDouble();

        public int Next(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);

        public double NextDouble(double minInclusive, double maxExclusive) => minInclusive + (_random.NextDouble() * (maxExclusive - minInclusive));
    }
}
