﻿using FireworksNet.Algorithm.Implementation;
using FireworksNet.Distributions;
using FireworksNet.Explode;
using FireworksNet.Generation;
using FireworksNet.Model;
using FireworksNet.Mutation;
using FireworksNet.Problems;
using FireworksNet.Random;
using FireworksNet.Selection;
using FireworksNet.StopConditions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
            this.bestSolution = state.BestSolution as Firework;

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

        /// <summary>
        /// Solve problem.
        /// </summary>
        /// <returns>Returns the best solution.</returns>
        public Solution Solve()
        {
            IEnumerable<double> fireworkQualities = state.Fireworks.Select(fw => fw.Quality);

            Debug.Assert(fireworkQualities != null, "Firework qualities is null");
            
            this.exploder.CalculateAmplitude(this.bestSolution, fireworkQualities);
            
            while (!ShouldStop(this.state))
            {
                Debug.Assert(this.state != null, "State is null");

                this.MakeStep(ref state);

                Debug.Assert(this.state != null, "State is null");
            }

            Debug.Assert(this.bestSolution != null, "Best solution is null");

            return this.bestSolution;
        }

        /// <summary>
        /// Makes one step of solve.
        /// </summary>
        /// <param name="state">The state after previous step.</param>
        /// <exception cref="ArgumentNullException">if state is <c>null</c></exception>
        private void MakeStep(ref AlgorithmState state)
        {
            if(state == null)
            {
                throw new ArgumentNullException("state");
            }

            IEnumerable<double> fireworkQualities = state.Fireworks.Select(fw => fw.Quality);

            Debug.Assert(fireworkQualities != null, "Firework qualities is null");

            FireworkExplosion explosion = this.exploder.Explode(this.bestSolution, fireworkQualities, state.StepNumber) as FireworkExplosion;

            Debug.Assert(explosion != null, "Explosion is null");

            //search
            foreach (MutableFirework firework in state.Fireworks)
            {
                MutableFirework mirror = firework;// cannot pass 'firework' as a ref or out argument because it is a 'foreach iteration variable'

                Debug.Assert(mirror != null, "Firework is null");

                this.researcher.MutateFirework(ref mirror, explosion);

                Debug.Assert(mirror != null, "Firework is null");
            }

            //mutation
            foreach (MutableFirework firework in state.Fireworks)
            {
                MutableFirework mirror = firework;// cannot pass 'firework' as a ref or out argument because it is a 'foreach iteration variable'

                Debug.Assert(mirror != null, "Firework is null");

                this.mutator.MutateFirework(ref mirror, explosion);

                Debug.Assert(mirror != null, "Firework is null");
            }

            this.CalculateQualities(state.Fireworks);
            this.exploder.CalculateAmplitude(this.bestSolution, fireworkQualities);
        }

        /// <summary>
        /// Make one step of algorithm.
        /// </summary>
        /// <param name="state">The state after previous step.</param>
        /// <returns>The new state of algorithm.</returns>
        /// <exception cref="ArgumentNullException">if state is <c>null</c></exception>
        public AlgorithmState MakeStep(AlgorithmState state)
        {
            //TODO: 
            //1. gpu thread for each researcher

            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            AlgorithmState newState = new AlgorithmState()
            {
                BestSolution = state.BestSolution,
                Fireworks = state.Fireworks,
                StepNumber = state.StepNumber            
            };

            this.MakeStep(ref newState);

            Debug.Assert(newState != null, "New state is null");

            return newState;
        }

        /// <summary>
        /// Returns best solution after algorithm work.
        /// </summary>
        /// <param name="state">The state after calculating.</param>
        /// <returns>The best solution.</returns>
        public Solution GetSolution(AlgorithmState state)
        {
            if (this.bestSolution == null)
            {
                throw new ArgumentNullException("state");
            }

            return this.bestSolution;
        }

        /// <summary>
        /// Checks is if algorithm should be stop.
        /// </summary>
        /// <param name="state">The current state of algorithm.</param>
        /// <returns><c>true</c> if should be stopped, otherwise - <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">if state is <c>null</c>.</exception>
        public bool ShouldStop(AlgorithmState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }

            return this.StopCondition.ShouldStop(state);
        }

        public AlgorithmState CreateInitialState()
        {
            Debug.Assert(this.ProblemToSolve != null, "Problem to solve cannot be null");
            Debug.Assert(this.Settings != null, "Settings of algorithm cannot be null");
            Debug.Assert(this.randomizer != null, "Generator cannot be null");

            InitialExplosion explosion = new InitialExplosion(this.Settings.FixedQuantitySparks);
            ISparkGenerator sparkGenerator = new InitialSparkGenerator(this.ProblemToSolve.Dimensions, this.randomizer);
            IEnumerable<Firework> originSparks = sparkGenerator.CreateSparks(explosion);

            Debug.Assert(originSparks != null, "Origin sparks collection is null");

            IEnumerable<MutableFirework> sparks = this.MakeMutant(originSparks);

            Debug.Assert(sparks != null, "sparks is null");

            AlgorithmState state = new AlgorithmState();
            state.Fireworks = sparks;
            state.BestSolution = ProblemToSolve.GetBest(sparks);
            state.StepNumber = 0;

            this.CalculateQualities(state.Fireworks);

            return state;
        }
        
        /// <summary>
        /// Turns firework to mutable firework.
        /// </summary>
        /// <param name="collection">The collection of firework for processing.</param>
        /// <returns>Collection of mutable fireworks.</returns>
        private IEnumerable<MutableFirework> MakeMutant(IEnumerable<Firework> collection)
        {
            Debug.Assert(collection != null, "Collection is null");

            Firework[] sparks = collection.ToArray();
            IList<MutableFirework> mutableSparks = new List<MutableFirework>(sparks.Length);
            for (int i = 0; i < sparks.Length; ++i)
            {
                mutableSparks.Add(new MutableFirework(sparks[i]));
            }

            return mutableSparks;
        }

        /// <summary>
        /// Calculate qualities for fireworks.
        /// </summary>
        /// <param name="sparks">The collection of sparks for which need calculate qualities.</param>
        private void CalculateQualities(IEnumerable<Firework> sparks)
        {
            Debug.Assert(sparks != null, "Sparks collection is null");
            Debug.Assert(this.ProblemToSolve != null, "Problem to solve is null");

            foreach (Firework spark in sparks)
            {
                Debug.Assert(spark != null, "Firework is null");
                Debug.Assert(double.IsNaN(spark.Quality), "Excessive quality calculation"); // If quality is not NaN, it most likely has been already calculated
                Debug.Assert(spark.Coordinates != null, "Firework coordinates collection is null");

                spark.Quality = this.ProblemToSolve.CalculateQuality(spark.Coordinates);
            }
        }
    }
}
