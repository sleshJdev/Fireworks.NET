using FireworksNet.Distributions;
using FireworksNet.Model;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireworksNet.Tests.Explode
{
    public abstract class ExplodeTestDataSource
    {
        public const double Amplitude = 1.0D;
        public const double Delta = 0.1D;       

        public static IEnumerable<object[]> DataForTestMethodExplodeOfParallelExploder
        {
            get
            {
                Firework epicenter = Substitute.For<Firework>(FireworkType.SpecificSpark, 0);
                IEnumerable<double> qualities = Substitute.For<IEnumerable<double>>();

                return new[]
                {
                    new object[] { null,      qualities,  0, typeof(ArgumentNullException),       "epicenter"},
                    new object[] { epicenter, null,       0, typeof(ArgumentNullException),       "currentFireworkQualities" },
                    new object[] { epicenter, qualities, -1, typeof(ArgumentOutOfRangeException), "currentStepNumber" }
                };
            }
        }       
    }
}
