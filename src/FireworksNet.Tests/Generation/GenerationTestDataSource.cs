using System;
using System.Collections.Generic;
using FireworksNet.Distributions;
using FireworksNet.Model;
using NSubstitute;
using FireworksNet.Generation;

namespace FireworksNet.Tests.Generation
{
    public abstract class GenerationTestDataSource
    {
        public const double Amplitude = 1.0D;
        public const double Delta = 0.1D;

        public static IEnumerable<object[]> DataForTestCreationInstanceOfAttractRepulseGenerator
        {
            get
            {
                Solution bestSolution = Substitute.For<Solution>(0);
                IEnumerable<Dimension> dimensions = Substitute.For<IEnumerable<Dimension>>();
                ContinuousUniformDistribution distribution = Substitute.For<ContinuousUniformDistribution>(GenerationTestDataSource.Amplitude - GenerationTestDataSource.Delta, GenerationTestDataSource.Amplitude + GenerationTestDataSource.Delta);
                System.Random randomizer = Substitute.For<System.Random>();

                return new[]
                {
                    new object[] {null,         dimensions, distribution, randomizer, "bestSolution"},
                    new object[] {bestSolution, null,       distribution, randomizer, "dimensions"},
                    new object[] {bestSolution, dimensions, null,         randomizer, "distribution"},
                    new object[] {bestSolution, dimensions, distribution, null,       "randomizer"}
                };
            }
        }      
    }
}