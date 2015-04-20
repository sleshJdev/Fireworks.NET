﻿using System;
using System.Collections.Generic;
using FireworksNet.Distributions;
using FireworksNet.Generation;
using FireworksNet.Model;
using NSubstitute;
using Xunit;

namespace FireworksNet.Tests.Generation
{
    public class AttractRepulseSparkGeneratorTests : GenerationTestDataSource
    {
        [Fact]
        public void CreateSpark_MustReturnNotNullFirework()
        {
            const int expectedBirthStepNumber = 1;
            const FireworkType expectedFireworkType = FireworkType.SpecificSpark;    

            var bestSolution = Substitute.For<Solution>(0);
            var dimensions = Substitute.For<IList<Dimension>>();
            var randomizer = Substitute.For<System.Random>();
            var distribution = Substitute.For<ContinuousUniformDistribution>(GenerationTestDataSource.Amplitude - GenerationTestDataSource.Delta, GenerationTestDataSource.Amplitude + GenerationTestDataSource.Delta);
            var epicenter = Substitute.For<Firework>(expectedFireworkType, expectedBirthStepNumber - 1);
            var qualities = Substitute.For<IEnumerable<double>>();
            var sparks = Substitute.For<Dictionary<FireworkType, int>>();
            var explosion = Substitute.For<FireworkExplosion>(epicenter, expectedBirthStepNumber, GenerationTestDataSource.Amplitude, sparks);

            var sparkGenerator = new AttractRepulseSparkGenerator(bestSolution, dimensions, distribution, randomizer);

            var spark = sparkGenerator.CreateSpark(explosion);

            Assert.NotNull(spark);
            Assert.Equal(expectedFireworkType, spark.FireworkType);
            Assert.Equal(expectedBirthStepNumber, spark.BirthStepNumber);
        }

        [Theory]
        [MemberData("DataForTestCreationInstanceOfAttractRepulseGenerator")]
        public void CreateIntaceOfAttractRepulseGenerator_PassEachParameterAsNullAndOtherIsCorrect_ArgumentNullExceptionThrown(
            Solution bestSolution, IEnumerable<Dimension> dimensions, ContinuousUniformDistribution distribution, System.Random randomizer, string expectedParamName)
        {
            ArgumentNullException exeption = Assert.Throws<ArgumentNullException>(
                () => new AttractRepulseSparkGenerator(bestSolution, dimensions, distribution, randomizer));

            Assert.Equal(expectedParamName, exeption.ParamName);
        }
    }
}
