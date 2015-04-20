using FireworksNet.Explode;
using FireworksNet.Model;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;


namespace FireworksNet.Tests.Explode
{
    public class ParallelExploderTests : ExplodeTestDataSource
    {
        [Fact]
        public void CreateInstanceOfParallelExploder_PassNullAsParameter_ArgumentNullExceptionThrown()
        {
            const string expectedParamName = "settings";

            ArgumentNullException exeption = Assert.Throws<ArgumentNullException>(() => new ParallelExploder(null));

            Assert.Equal(expectedParamName, exeption.ParamName);
        }

        [Fact]
        public void CreateInstanceOfParallelExploder_PassValidSettings_ShouldReturnNotNullExploder()
        {
            var exploderSettings = Substitute.For<ParallelExploderSettings>();

            var exploder = new ParallelExploder(exploderSettings);

            Assert.NotNull(exploder);
        }

        [Fact]
        public void Explode_PassValidParameters_ShouldReturnExplosionBase()
        {
            const int expectedBirthStepNumber = 1;
            const FireworkType expectedFireworkType = FireworkType.SpecificSpark;
            var exploderSettings = Substitute.For<ParallelExploderSettings>();
            var epicenter = Substitute.For<Firework>(expectedFireworkType, expectedBirthStepNumber - 1);
            var qualities = Substitute.For<IEnumerable<double>>();
            var exploder = new ParallelExploder(exploderSettings);

            var explosion = exploder.Explode(epicenter, qualities, expectedBirthStepNumber);

            Assert.NotNull(explosion);
            Assert.True(explosion is FireworkExplosion);
            Assert.Equal(exploderSettings.Amplitude, (explosion as FireworkExplosion).Amplitude);
            Assert.Equal(epicenter, (explosion as FireworkExplosion).ParentFirework);
            Assert.Equal(expectedBirthStepNumber, explosion.StepNumber);
        }

        [Theory, MemberData("DataForTestMethodExplodeOfParallelExploder")]
        public void Explode_PassEachParameterAsNullAndOtherIsCorrect_ArgumentExceptionThrown(
            Firework epicenter, IEnumerable<double> qualities, int currentStepNumber, Type exceptionType,  string expectedParamName)
        {
            var exploderSettings = Substitute.For<ParallelExploderSettings>();           
            var exploder = new ParallelExploder(exploderSettings);
                       
            string actualParamName = null;

            if (typeof(ArgumentNullException) == exceptionType)
            {
                ArgumentNullException exeption = Assert.Throws<ArgumentNullException>(
                    () => exploder.Explode(epicenter, qualities, currentStepNumber));
                actualParamName = exeption.ParamName;
            }
            else if(typeof(ArgumentOutOfRangeException) == exceptionType)
            {
                ArgumentOutOfRangeException exeption = Assert.Throws<ArgumentOutOfRangeException>(
                    () => exploder.Explode(epicenter, qualities, currentStepNumber));
                actualParamName = exeption.ParamName;
            }            

            Assert.Equal(expectedParamName, actualParamName);
        }        
    }
}
