using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgentSim.Core.Utilities;
using Xunit;

namespace AgentSim.Core.Tests
{
    public class RandomProviderTests
    {
        [Fact]
        public void NextDouble_WithSameSeed_ProducesSameSequence()
        {
            var rng1 = new RandomProvider(seed: 42);
            var rng2 = new RandomProvider(seed: 42);

            var sequence1 = new[] { rng1.NextDouble(), rng1.NextDouble(), rng1.NextDouble() };
            var sequence2 = new[] { rng2.NextDouble(), rng2.NextDouble(), rng2.NextDouble() };

            Assert.Equal(sequence1, sequence2);
        }

        [Fact]
        public void NextDouble_ReturnsValueBetweenZeroAndOne()
        {
            var rng = new RandomProvider();

            for (int i = 0; i < 100; i++)
            {
                var value = rng.NextDouble();
                Assert.InRange(value, 0.0, 1.0);
            }
        }

        [Fact]
        public void NextDoubleRange_ReturnsValueWithinBounds()
        {
            var rng = new RandomProvider();

            for (int i = 0; i < 100; i++)
            {
                var value = rng.NextDouble(-25, 25);
                Assert.InRange(value, -25.0, 25.0);
            }
        }
    }
}
