using FireworksNet.Generation;
using FireworksNet.Model;
using FireworksNet.Mutation;
using FireworksNet.Selection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FireworksNet.Tests.Mutation
{
    public class FireworkSearchMutatorTests : MutationTestDataSource
    {
        [Theory]
        [MemberData("DataForTestCreationOfFireworkSearchMutator")]
        public void CreationFireworkSearchMutator_PassEachFromFirstThreeParameterAsNullAndOtherIsCorrect_ArgumentNullExceptionThrown(
            Action<IEnumerable<Firework>> calculator, ISparkGenerator generator, IFireworkSelector selector, int searchExplosionsCount, string expectedParamName)
        {
            ArgumentException exception = Assert.Throws<ArgumentNullException>(() => new FireworkSearchMutator(calculator, generator, selector, searchExplosionsCount));

            Assert.Equal(expectedParamName, exception.ParamName);
        }

        [Fact]
        public void CreationFireworkSearchMutator_Pass4thParameterAsNull_ArgumentOutOfRangeException()
        {
            const string expectedParamName = "searchExplosionsCount";
            Action<IEnumerable<Firework>> calculator = ((e) => { });//simply stub
            var generator = CreateAttractRepulseSparkGenerator();
            var selector = Substitute.For<BestFireworkSelector>(Substitute.For<Func<IEnumerable<Firework>, Firework>>());
            var searchExplosionsCount = 0;

            ArgumentException exception = Assert.Throws<ArgumentOutOfRangeException>(() => new FireworkSearchMutator(calculator, generator, selector, searchExplosionsCount));

            Assert.Equal(expectedParamName, exception.ParamName);
        }

        [Fact]
        public void CreationFireworkSearchMutator_PassValidParameters_MustReturnNotNullFireworkSearchMutator()
        {
            Action<IEnumerable<Firework>> calculator = ((e) => { });//simply stub
            var generator = CreateAttractRepulseSparkGenerator();
            var selector = Substitute.For<BestFireworkSelector>(Substitute.For<Func<IEnumerable<Firework>, Firework>>());
            var searchExplosionsCount = 5;

            IFireworkMutator researcher = new FireworkSearchMutator(calculator, generator, selector, searchExplosionsCount);

            Assert.NotNull(researcher);
        }
       
        [Theory]
        [MemberData("DataForTestMethodMutate")]
        public void MutateFirework_PassEachParameterAsNullAndOtherIsCorrect_ArgumentNullExceptionThrown(
            MutableFirework mutableFirework, FireworkExplosion explosion, String expectedParamName)
        {
            Action<IEnumerable<Firework>> calculator = ((e) => { });//simply stub
            var generator = CreateAttractRepulseSparkGenerator();
            var selector = Substitute.For<BestFireworkSelector>(Substitute.For<Func<IEnumerable<Firework>, Firework>>());
            var searchExplosionsCount = 5;
            IFireworkMutator researcher = new FireworkSearchMutator(calculator, generator, selector, searchExplosionsCount);

            ArgumentException exception = Assert.Throws<ArgumentNullException>(() => researcher.MutateFirework(ref mutableFirework, explosion));

            Assert.Equal(expectedParamName, exception.ParamName);
        }

        [Fact]
        public void MutateFirework_PassValidParameters_ShouldMoveFireworkToBetterLocation()
        {
            const int birthStep = 666;
            const int majorValue = 10;//common value for initialize all necessary components

            Range range = new Range(-majorValue, majorValue);
            IList<MutableFirework> sparks = new List<MutableFirework>(majorValue);             
            IList<Dimension> dimensions = new List<Dimension>();
            dimensions.Add(new Dimension(range));
            dimensions.Add(new Dimension(range));
            dimensions.Add(new Dimension(range));

            const int shift = -majorValue / 2;// to avoid default initialize bug
            for (int i = shift; i < majorValue + shift; ++i)
            {
                IDictionary<Dimension, double> coordinates = new Dictionary<Dimension, double>();
                foreach (Dimension dimension in dimensions)
                {
                    coordinates.Add(dimension, i);
                }
                MutableFirework firework = new MutableFirework(FireworkType.SpecificSpark, birthStep, coordinates);
                sparks.Add(firework);                
            }
            MutableFirework bestFirework = sparks.First();// best firework. mutable firework must move to it
            MutableFirework mutableFirework = sparks.Last();// originally mutable firework is the worst
            FireworkExplosion explosion = CreateFireworkExplosion(mutableFirework);

            Action<IEnumerable<Firework>> calculator = ((e) => 
            { 
                for (int i = 0; i < majorValue; ++i)//0-th firework is best, because his quality(minimum) is optimal
                {
                    e.ElementAt(i).Quality = shift + i;
                }
            });
            
            var generator = CreateAttractRepulseSparkGenerator();
            generator.CreateSparks(explosion).Returns(sparks);

            var selector = Substitute.For<BestFireworkSelector>(Substitute.For<Func<IEnumerable<Firework>, Firework>>());
            selector.SelectFireworks(sparks).Returns(new List<Firework>() { bestFirework });

            var searchExplosionsCount = 5;

            IFireworkMutator researcher = new FireworkSearchMutator(calculator, generator, selector, searchExplosionsCount);
            researcher.MutateFirework(ref mutableFirework, explosion);

            Assert.NotNull(bestFirework);
            Assert.NotNull(mutableFirework);            
            Assert.Equal(mutableFirework.BirthStepNumber, bestFirework.BirthStepNumber);
            Assert.Equal(mutableFirework.Quality, bestFirework.Quality);
            double dimensionValueBefore;
            double dimensionValueAfter;
            foreach (Dimension dimension in dimensions)
            {
                mutableFirework.Coordinates.TryGetValue(dimension, out dimensionValueBefore);
                bestFirework.Coordinates.TryGetValue(dimension, out dimensionValueAfter);
                Assert.Equal(dimensionValueBefore, dimensionValueAfter);
            }
        }
    }
}
