using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using FireworksNet.Algorithm;
using FireworksNet.Problems;
using FireworksNet.StopConditions;
using FireworksNet.Model;
using FireworksNet.Explode;
using FireworksNet.Distributions;
using FireworksNet.Algorithm.Implementation;
using FireworksNet.Selection;
using FireworksNet.Random;
using FireworksNet.Generation;
using FireworksNet.Mutation;
using System;

namespace FireworksNet.Algorithm
{
    /// <summary>
    /// Fireworks algorithm implementation based on GPU, as described in 2013 GPU paper.
    /// </summary>
    public class ParallelFireworksAlgorithm : IFireworksAlgorithm, IStepperFireworksAlgorithm
    {
        private readonly System.Random randomizer;
        private IContinuousDistribution distribution;
        private ParallelExploder exploder;         
        private AlgorithmState state;

        /// <summary>
        /// Execute search.
        /// </summary>
        private IFireworkMutator researcher;

        /// <summary>
        /// Execute mutation
        /// </summary>
        IFireworkMutator mutator;

        /// <summary>
        /// Represent best solution in now.
        /// </summary>
        private Firework bestSolution;

        /// <summary>
        /// Gets the problem to be solved by the algorithm.
        /// </summary>
        public Problem ProblemToSolve { private set; get; }

        /// <summary>
        /// Gets the stop condition for the algorithm.
        /// </summary>
        public IStopCondition StopCondition { private set; get; }

        /// <summary>
        /// Gets the algorithm settings.
        /// </summary>
        public ParallelFireworksAlgorithmSettings Settings { private set; get; }       

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelFireworksAlgorithm"/> class.
        /// </summary>
        /// <param name="problem">The problem to be solved by the algorithm.</param>
        /// <param name="stopCondition">The stop condition for the algorithm.</param>
        /// <param name="settings">The algorithm settings.</param>
        /// <exception cref="System.ArgumentNullException"> if <paramref name="problem"/>
        /// or <paramref name="stopCondition"/> or <paramref name="settings"/> is 
        /// <c>null</c>.</exception>
        public ParallelFireworksAlgorithm(Problem problem, IStopCondition stopCondition, ParallelFireworksAlgorithmSettings settings)
        {
            if (problem == null) 
            {
                throw new System.ArgumentNullException("problem"); 
            }
            
            if (stopCondition == null) 
            {
                throw new System.ArgumentNullException("stopCondition"); 
            }
            
            if (settings == null) 
            {
                throw new System.ArgumentNullException("settings"); 
            }

            this.ProblemToSolve = problem;
            this.StopCondition = stopCondition;
            this.Settings = settings;

            this.randomizer = new DefaultRandom();
            this.distribution = new ContinuousUniformDistribution(1 - settings.Delta, 1 + settings.Delta);

            this.state = CreateInitialState();
            this.bestSolution = state.BestSolution as Firework;//TODO: improve. need try without 'as'

            ISparkGenerator generator = new AttractRepulseSparkGenerator(this.bestSolution, problem.Dimensions, this.distribution, this.randomizer);            
            IFireworkSelector selector = new BestFireworkSelector((fireworks) => fireworks.OrderBy(f => f.Quality).First<Firework>());//select best
            this.mutator = new AttractRepulseSparkMutator(generator);
            this.researcher = new FireworkSearchMutator(this.CalculateQualities, generator, selector, settings.SearchExplosionsCount);

            this.exploder = new ParallelExploder(new ParallelExploderSettings()
            {                
                FixedQuantitySparks = settings.FixedQuantitySparks,
                Amplitude = settings.Amplitude,
                ExplosionSparksMaximumAmplitude = settings.ExplosionSparksMaximumAmplitude,
                Delta = settings.Delta
            });                       
        }

        public Solution Solve()
        {            
            IEnumerable<double> fireworkQualities = state.Fireworks.Select(fw => fw.Quality);
            this.exploder.RecalculateAmplitude(this.bestSolution, fireworkQualities);

            while (!ShouldStop(this.state))
            {
                MakeStep(state);
            }

            return this.state.BestSolution;
        }

        public AlgorithmState MakeStep(AlgorithmState state)
        {
            //TODO: 
            //1. gpu thread for each reseacher
            //2. improve the creation of the initial fireworks
            //3. add overload of MutateFirework, which don't will change state
            
            if(state == null)
            {
                throw new ArgumentNullException("state");
            }

            IEnumerable<double> fireworkQualities = state.Fireworks.Select(fw => fw.Quality);
            FireworkExplosion explosion = this.exploder.Explode(this.bestSolution, fireworkQualities, state.StepNumber) as FireworkExplosion;//TODO: improve. need try without 'as'
            
            //search
            foreach (MutableFirework firework in state.Fireworks)
            {
                MutableFirework mirror = firework;// cannot pass 'firework' as a ref or out argument because it is a 'foreach iteration variable'
                this.researcher.MutateFirework(ref mirror, explosion);   
            }

            //mutation
            foreach (MutableFirework firework in state.Fireworks)
            {
                MutableFirework mirror = firework;// cannot pass 'firework' as a ref or out argument because it is a 'foreach iteration variable'
                this.mutator.MutateFirework(ref mirror, explosion);
            }

            this.CalculateQualities(state.Fireworks);
            this.exploder.RecalculateAmplitude(this.bestSolution, fireworkQualities);

            return state;
        }       

        //TODO: or return state.BestSolution?
        public Solution GetSolution(AlgorithmState state)
        {
            if (this.bestSolution == null) 
            {
                throw new System.ArgumentNullException("state"); 
            }

            return this.bestSolution;
        }

        public bool ShouldStop(AlgorithmState state)
        {
            if (state == null) 
            { 
                throw new System.ArgumentNullException("state"); 
            }

            return StopCondition.ShouldStop(state);
        }         

        public AlgorithmState CreateInitialState()
        {
            Debug.Assert(this.ProblemToSolve != null, "Problem to solve cannot be null");
            Debug.Assert(this.Settings != null, "Settings of algorithm cannot be null");
            Debug.Assert(this.randomizer != null, "Generator cannot be null");

            InitialExplosion explosion = new InitialExplosion(this.Settings.FixedQuantitySparks);
            ParallelInitialSparkGenerator sparkGenerator = new ParallelInitialSparkGenerator(this.ProblemToSolve.Dimensions, this.randomizer);
            IEnumerable<MutableFirework> sparks = sparkGenerator.CreateSparks(explosion);            

            Debug.Assert(sparks != null, "sparks is null");

            AlgorithmState state = new AlgorithmState();
            state.Fireworks = sparks;
            state.BestSolution = ProblemToSolve.GetBest(sparks);
            state.StepNumber = 0;

            CalculateQualities(state.Fireworks);

            return state;
        }

        private void CalculateQualities(IEnumerable<Firework> sparks)
        {
            foreach (Firework spark in sparks)
            {        
               spark.Quality = ProblemToSolve.CalculateQuality(spark.Coordinates);
            }
        }
    }
}
