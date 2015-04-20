using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FireworksNet.Algorithm.Implementation;
using FireworksNet.Model;
using FireworksNet.Selection;
using FireworksNet.Algorithm;
using FireworksNet.Generation;

namespace FireworksNet.Mutation
{
    /// <summary>
    /// Implements search algorithm.
    /// </summary>
    public class FireworkSearchMutator : IFireworkMutator
    {
        private Action<IEnumerable<Firework>> calculator;
        private ISparkGenerator generator;
        private IFireworkSelector selector;
        private int searchExplosionsCount;

        /// <summary>
        /// Create instance of SearchMutator.
        /// </summary>
        /// <param name="calculator">Delegate of function to calculate quality during search.</param>
        /// <param name="mutator">Execute mutation of firework.</param>
        /// <param name="generator">Generate fireworks during search.</param>
        /// <param name="selector">Util class for select best firework, after generate.</param>
        /// <param name="searchExplosionsCount">Quantity of search iterations.</param>
        /// <exception cref="System.ArgumentNullException"> 
        /// if <paramref name="calculator"/> or <paramref name="mutator"/> or <paramref name="generator"/> or <paramref name="selector"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// if <paramref name="searchExplosionsCount"/> is negative.
        /// </exception>
        public FireworkSearchMutator(Action<IEnumerable<Firework>> calculator,ISparkGenerator generator, IFireworkSelector selector, int searchExplosionsCount)
        {
            if (calculator == null)
            {
                throw new ArgumentNullException("calculator");
            }

            if (generator == null)
            {
                throw new ArgumentNullException("generator");
            }

            if (selector == null)
            {
                throw new ArgumentNullException("selector");
            }

            if (searchExplosionsCount < 1)
            {
                throw new ArgumentOutOfRangeException("searchExplosionsCount");
            }

            this.calculator = calculator;
            this.generator = generator;
            this.selector = selector;
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
                IEnumerable<Firework> sparks = this.generator.CreateSparks(explosion);
                Debug.Assert(sparks != null, "Sparks cannot is null");

                this.calculator(sparks);

                Firework newState = this.selector.SelectFireworks(sparks).First();//select best firework
                Debug.Assert(newState != null, "New state firework is null");

                mutableFirework.Update(newState);
            }
        }
    }
}
