using FireworksNet.Distributions;
using FireworksNet.Generation;
using FireworksNet.Model;
using FireworksNet.Selection;
using NSubstitute;
using System;
using System.Collections.Generic;

namespace FireworksNet.Tests.Mutation
{
    public abstract class MutationTestDataSource
    {
        public const double Amplitude = 1.0D;
        public const double Delta = 0.1D;

        public static IEnumerable<object[]> DataForTestCreationOfFireworkSearchMutator
        {
            get
            {
                var calculator = Substitute.For<Action<IEnumerable<Firework>>>();
                var generator = CreateAttractRepulseSparkGenerator();
                var selector = Substitute.For<BestFireworkSelector>(Substitute.For<Func<IEnumerable<Firework>, Firework>>());
                return new[]
                {
                    new object[]{null,       generator, selector, 10, "calculator"},
                    new object[]{calculator, null,      selector, 10, "generator"},
                    new object[]{calculator, generator, null,     10, "selector"}
                };
            }
        }

        public static IEnumerable<object[]> DataForTestMethodMutate
        {
            get
            {
                var coordinates = Substitute.For<IDictionary<Dimension, double>>();
                var mutableFirework = Substitute.For<MutableFirework>(FireworkType.SpecificSpark, 0, coordinates);

                var epicenter = mutableFirework;
                var sparks = Substitute.For<Dictionary<FireworkType, int>>();
                var explosion = Substitute.For<FireworkExplosion>(epicenter, 1, MutationTestDataSource.Amplitude, sparks);

                return new[]
                {
                    new object[]{mutableFirework, null, "explosion"},
                    new object[]{null, explosion, "mutableFirework"}
                };
            }
        }

        public static ISparkGenerator CreateAttractRepulseSparkGenerator()
        {
            var bestSolution = Substitute.For<Solution>(0);
            var dimensions = Substitute.For<IList<Dimension>>();
            var distribution = Substitute.For<ContinuousUniformDistribution>(MutationTestDataSource.Amplitude - MutationTestDataSource.Delta, MutationTestDataSource.Amplitude + MutationTestDataSource.Delta);
            var randomizer = Substitute.For<System.Random>();
            var generator = Substitute.For<AttractRepulseSparkGenerator>(bestSolution, dimensions, distribution, randomizer);

            return generator;
        }

        public static FireworkExplosion CreateFireworkExplosion(Firework epicenter)
        {
            var sparks = Substitute.For<Dictionary<FireworkType, int>>();
            var explosion = Substitute.For<FireworkExplosion>(epicenter, 1, MutationTestDataSource.Amplitude, sparks);

            return explosion;
        }  
    }
}