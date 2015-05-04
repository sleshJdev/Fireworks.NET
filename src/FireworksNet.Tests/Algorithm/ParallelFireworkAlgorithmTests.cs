using FireworksNet.Algorithm;
using FireworksNet.Algorithm.Implementation;
using FireworksNet.Problems;
using FireworksNet.StopConditions;
using System;
using Xunit;
namespace FireworksNet.Tests.Algorithm
{
    public class ParallelFireworkAlgorithmTests : AlgorithmTestDataSource
    {
        [Theory]
        [MemberData("DataForTestingCreationOfParallelFireworkAlgorithm")]
        public void CreationParallelFireworkAlgorithm_PassEachParameterAsNullAndOtherIsCorrect_ArgumentNullExceptionThrown(
            Problem problem, IStopCondition stopCondition, ParallelFireworksAlgorithmSettings settings, string expectedParamName)
        {
            ArgumentException exception = Assert.Throws<ArgumentNullException>(() => new ParallelFireworksAlgorithm(problem, stopCondition, settings));

            Assert.Equal(expectedParamName, exception.ParamName);
        }

        
    }
}
