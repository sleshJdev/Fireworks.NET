using FireworksNet.Algorithm.Implementation;
using FireworksNet.Model;
using FireworksNet.Problems;
using FireworksNet.StopConditions;
using NSubstitute;
using System;
using System.Collections.Generic;
namespace FireworksNet.Tests.Algorithm
{
    public abstract class AlgorithmTestDataSource
    {
        public static IEnumerable<object[]> DataForTestingCreationOfParallelFireworkAlgorithm
        {
            get
            {
                var range = new Range(-1, 1);
                var dimension = new Dimension(range);

                var dimensions = new List<Dimension>()
                {
                    dimension
                };
                var initialRanges = new Dictionary<Dimension, Range>
                {
                    {dimension, range}
                };

                var func = Substitute.For<Func<IDictionary<Dimension, double>, double>>();
                var problemTarget = ProblemTarget.Minimum;

                var problem = Substitute.For<Problem>(dimensions, initialRanges, func, problemTarget);
                var stopCondition = Substitute.For<IStopCondition>();
                var settings = Substitute.For<ParallelFireworksAlgorithmSettings>();

                return new[]
                {
                    new object[] {null,     stopCondition,  settings, "problem"},
                    new object[] {problem,  null,           settings, "stopCondition"},
                    new object[] {problem,  stopCondition,  null,     "settings"}
                };
            }
        }
    }
}
