using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FireworksNet.Extensions;
using FireworksNet.Model;

namespace FireworksNet.Explode
{
    /// <summary>
    /// Represent exploder for gpu based implementation of Fireworks algorithm, as described in 2013 GPU paper.
    /// </summary>
    public class ParallelExploder : IExploder
    {
        private readonly ParallelExploderSettings settings;

        /// <summary>
        /// Create instance of ParallelExploder.
        /// </summary>
        /// <param name="settings">settings for ParallelExploder</param>
        public ParallelExploder(ParallelExploderSettings settings)
        {
            if (settings == null) 
            {
                throw new System.ArgumentNullException("settings"); 
            }

            this.settings = settings;
        }

        /// <summary>
        /// Explode firework.
        /// </summary>
        /// <param name="epicenter">Epicenter  - explode the fireworks.</param>
        /// <param name="currentFireworkQualities">Qualities of epicenter.</param>
        /// <param name="currentStepNumber">Step number for now.</param>
        /// <returns></returns>
        public ExplosionBase Explode(Firework epicenter, IEnumerable<double> currentFireworkQualities, int currentStepNumber)
        {
            if (epicenter == null) 
            { 
                throw new System.ArgumentNullException("epicenter"); 
            }
            
            if (currentFireworkQualities == null) 
            { 
                throw new System.ArgumentNullException("currentFireworkQualities"); 
            }
            
            if (currentStepNumber < 0) 
            {
                throw new System.ArgumentOutOfRangeException("currentStepNumber");
            }

            IDictionary<FireworkType, int> sparks = new Dictionary<FireworkType, int>()
            {
                {FireworkType.SpecificSpark, this.settings.FixedQuantitySparks}
            };

            return new FireworkExplosion(epicenter, currentStepNumber, this.settings.Amplitude, sparks);
        }

        /// <summary>
        /// Calculates the explosion amplitude.
        /// </summary>
        /// <param name="focus">The explosion focus.</param>
        /// <param name="currentFireworkQualities">The current firework qualities.</param>
        /// <returns>The explosion amplitude.</returns>
        public void CalculateAmplitude(Solution focus, IEnumerable<double> currentFireworkQualities)
        {
            Debug.Assert(focus != null, "Focus is null");
            Debug.Assert(currentFireworkQualities != null, "Current firework qualities is null");
            Debug.Assert(this.settings != null, "Settings is null");

            // Using Aggregate() here because Min() won't use my double extensions
            double minFireworkQuality = currentFireworkQualities.Aggregate((agg, next) => next.IsLess(agg) ? next : agg);

            Debug.Assert(!double.IsNaN(minFireworkQuality), "Min firework quality is NaN");
            Debug.Assert(!double.IsInfinity(minFireworkQuality), "Min firework quality is Infinity");           

            this.settings.Amplitude = this.settings.ExplosionSparksMaximumAmplitude * (focus.Quality - minFireworkQuality + double.Epsilon) / (currentFireworkQualities.Sum(fq => fq - minFireworkQuality) + double.Epsilon);
        }        
    }
}
