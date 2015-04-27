using FireworksNet.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FireworksNet.Generation
{
    public class ParallelInitialSparkGenerator : InitialSparkGenerator
    {
        public ParallelInitialSparkGenerator(IEnumerable<Dimension> dimensions, System.Random randomizer)
            : base(dimensions, randomizer)
        {
        }

        public override IEnumerable<MutableFirework> CreateSparks(ExplosionBase explosion)
        {
            Firework[] sparks = base.CreateSparks(explosion).ToArray();
            IList<MutableFirework> mutableSparks = new List<MutableFirework>(sparks.Length);
            for (int i = 0; i < sparks.Length; ++i)
            {
                mutableSparks.Add(new MutableFirework(sparks[i]));
            }

            return mutableSparks;
        }
    }
}
