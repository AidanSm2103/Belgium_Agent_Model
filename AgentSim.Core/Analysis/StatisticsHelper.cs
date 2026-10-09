using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgentSim.Core.Analysis
{
    // Small, independently-testable statistics helpers used to summarize Monte Carlo trial results. 
    public static class StatisticsHelper
    {
        public static double Mean(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count == 0) return 0;
            return list.Average();
        }

        public static double StdDev(IEnumerable<double> values)
        {
            var list = values.ToList();
            if (list.Count == 0) return 0;

            double mean = Mean(list);
            double sumOfSquares = list.Sum(v => (v - mean) * (v - mean));
            return Math.Sqrt(sumOfSquares / list.Count);
        }

        public static double Min(IEnumerable<double> values) => values.DefaultIfEmpty(0).Min();
        public static double Max(IEnumerable<double> values) => values.DefaultIfEmpty(0).Max();
    }
}
