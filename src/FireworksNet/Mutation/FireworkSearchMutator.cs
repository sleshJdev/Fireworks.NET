using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FireworksNet.Algorithm.Implementation;
using FireworksNet.Model;
using FireworksNet.Selection;
using FireworksNet.Algorithm;
using FireworksNet.Generation;
using FireworksNet.Problems;

namespace FireworksNet.Mutation
{
    /// <summary>
    /// Implements search algorithm.
    /// </summary>
    public class FireworkSearchMutator : IFireworkMutator
    {
        private Action<IEnumerable<Firework>> qualityCalculator;
        private Func<IEnumerable<Firework>, Firework> bestFireworkSelector;
        private ISparkGenerator sparkGenerator;
        private int searchExplosionsCount;

        /// <summary>
        /// Create instance of SearchMutator.
        /// </summary>
        /// <param name="calculator">Delegate of function to calculate quality during search.</param>
        /// <param name="mutator">Execute mutation of firework.</param>
        /// <param name="sparkGenerator">Generate fireworks during search.</param>
        /// <param name="bestFireworkSelector">Util class for select best firework, after generate.</param>
        /// <param name="searchExplosionsCount">Quantity of search iterations.</param>
        /// <exception cref="System.ArgumentNullException"> 
        /// if <paramref name="calculator"/> or <paramref name="mutator"/> or <paramref name="sparkGenerator"/> or <paramref name="bestFireworkSelector"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// if <paramref name="searchExplosionsCount"/> is negative.
        /// </exception>
        public FireworkSearchMutator(Action<IEnumerable<Firework>> qualityCalculator, ISparkGenerator sparkGenerator, Func<IEnumerable<Firework>, Firework> bestFireworkSelector, int searchExplosionsCount)
        {
            if (qualityCalculator == null)
            {
                throw new ArgumentNullException("calculator");
            }

            if (sparkGenerator == null)
            {
                throw new ArgumentNullException("generator");
            }

            if (bestFireworkSelector == null)
            {
                throw new ArgumentNullException("selector");
            }

            if (searchExplosionsCount < 1)
            {
                throw new ArgumentOutOfRangeException("searchExplosionsCount");
            }

            this.qualityCalculator = qualityCalculator;
            this.sparkGenerator = sparkGenerator;
            this.bestFireworkSelector = bestFireworkSelector;
            this.searchExplosionsCount = searchExplosionsCount;
        }

        /// <summary>
        /// Executes search. It will be explodes firework 'L' times.
        /// Here, the "L" is set in the <see cref="ParallelFireworksAlgorithmSettings"/>.
        /// 'L' equals ParallelFireworksAlgorithmSettings.searchExplosionsCount;
        /// </summary>
        /// <param name="bestFirework">The Firework for mutate.</param>
        /// <param name="explosion">The explosion that gives birth to the spark.</param>
        public void MutateFirework(ref MutableFirework mutableFirework, FireworkExplosion explosion)
        {
            if (mutableFirework == null)
            {
                throw new ArgumentNullException("mutableFirework");
            }

            if (explosion == null)
            {
                throw new ArgumentNullException("explosion");
            }

            Debug.Assert(explosion.ParentFirework != null, "Explosion parent firework is null");
            Debug.Assert(explosion.ParentFirework.Coordinates != null, "Explosion parent firework coordinate collection is null");             

            for (int i = 0; i < searchExplosionsCount; ++i)
            {
                IEnumerable<Firework> sparks = this.sparkGenerator.CreateSparks(explosion);

                Debug.Assert(sparks != null, "Sparks cannot is null");
                
                this.qualityCalculator(sparks);
                
                Firework newState = this.bestFireworkSelector(sparks);

                Debug.Assert(newState != null, "New state firework is null");
                
                mutableFirework.Update(newState);
            }
        }
    }
}
