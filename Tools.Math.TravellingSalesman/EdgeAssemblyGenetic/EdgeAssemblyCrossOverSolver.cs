﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools.Math.TSP.Problems;
using Tools.Math.VRP.Core.Routes;
using Tools.Math.AI.Genetic;
using Tools.Math.TSP.Genetic.Solver;
using Tools.Math.AI.Genetic.Operations.Generation;
using Tools.Math.AI.Genetic.Solvers;
using Tools.Math.AI.Genetic.Operations.CrossOver;
using Tools.Math.VRP.Core.Routes.ASymmetric;

namespace Tools.Math.TSP.EdgeAssemblyGenetic
{
    /// <summary>
    /// Implements a best-placement solver.
    /// </summary>
    public class EdgeAssemblyCrossOverSolver : ISolver
    {
        /// <summary>
        /// Keeps the stopped flag.
        /// </summary>
        private bool _stopped = false;

        /// <summary>
        /// Keeps an orginal list of customers.
        /// </summary>
        private IList<int> _customers;

        /// <summary>
        /// Holds the population size.
        /// </summary>
        private int _population_size;

        /// <summary>
        /// Holds the stagnation count.
        /// </summary>
        private int _stagnation;

        /// <summary>
        /// Holds a generation operation.
        /// </summary>
        private IGenerationOperation<List<int>, GeneticProblem, Fitness> _generation_operation;

        /// <summary>
        /// Holds a generation operation.
        /// </summary>
        private ICrossOverOperation<List<int>, GeneticProblem, Fitness> _cross_over_operation;

        /// <summary>
        /// Creates a new solver.
        /// </summary>
        /// <param name="problem"></param>
        public EdgeAssemblyCrossOverSolver(int population_size, int stagnation,
            IGenerationOperation<List<int>, GeneticProblem, Fitness> generation_operation,
            ICrossOverOperation<List<int>, GeneticProblem, Fitness> cross_over_operation)
        {
            _stopped = false;
            _stagnation = stagnation;
            _population_size = population_size;

            _generation_operation = generation_operation;
            _cross_over_operation = cross_over_operation;
        }

        /// <summary>
        /// Creates a new solver.
        /// </summary>
        /// <param name="problem"></param>
        public EdgeAssemblyCrossOverSolver(Tools.Math.TSP.Problems.IProblem problem, IList<int> customers)
        {
            _stopped = false;
            _customers = customers;
        }

        /// <summary>
        /// Retuns the name of this solver.
        /// </summary>
        public string Name
        {
            get
            {
                return string.Format("EAX{0}_{1}_{2}", _population_size,
                    _generation_operation.Name,
                    _cross_over_operation.Name);
            }
        }

        /// <summary>
        /// Returns a solution found using best-placement.
        /// </summary>
        /// <returns></returns>
        public IRoute Solve(Tools.Math.TSP.Problems.IProblem problem)
        {
            // create the settings.
            SolverSettings settings = new SolverSettings(
                -1,
                -1,
                1000000000,
                -1,
                -1,
                -1);

            Solver<List<int>, GeneticProblem, Fitness> solver =
                new Solver<List<int>, GeneticProblem, Fitness>(
                new GeneticProblem(problem),
                settings,
                null,
                null,
                null,
                _generation_operation,
                new FitnessCalculator(),
                true, false);

            Population<List<int>, GeneticProblem, Fitness> population =
                new Population<List<int>, GeneticProblem, Fitness>(true);
            while (population.Count < _population_size)
            {
                // generate new.
                Individual<List<int>, GeneticProblem, Fitness> new_individual =
                    _generation_operation.Generate(solver);

                // add to population.
                population.Add(new_individual);
            }

            // select each individual once.
            Population<List<int>, GeneticProblem, Fitness> new_population =
                new Population<List<int>, GeneticProblem, Fitness>(true);
            Individual<List<int>, GeneticProblem, Fitness> best = null;
            int stagnation = 0;
            while (stagnation < _stagnation)
            {
                while (new_population.Count < _population_size)
                {
                    // select an individual and the next one.
                    int idx = Tools.Math.Random.StaticRandomGenerator.Get().Generate(population.Count);
                    Individual<List<int>, GeneticProblem, Fitness> individual1 = population[idx];
                    Individual<List<int>, GeneticProblem, Fitness> individual2 = null;
                    if (idx == population.Count - 1)
                    {
                        individual2 = population[0];
                    }
                    else
                    {
                        individual2 = population[idx + 1];
                    }
                    population.RemoveAt(idx);

                    Individual<List<int>, GeneticProblem, Fitness> new_individual = _cross_over_operation.CrossOver(solver,
                        individual1, individual2);

                    new_individual.CalculateFitness(solver.Problem, solver.FitnessCalculator);
                    if (new_individual.Fitness.CompareTo(
                        individual1.Fitness) < 0)
                    {
                        new_population.Add(new_individual);
                    }
                    else
                    {
                        new_population.Add(individual1);
                    }
                }

                population = new_population;
                population.Sort(solver, solver.FitnessCalculator);

                new_population = new Population<List<int>, GeneticProblem, Fitness>(true);

                if (best == null ||
                    best.Fitness.CompareTo(population[0].Fitness) > 0)
                {
                    stagnation = 0;
                    best = population[0];
                }
                else
                {
                    stagnation++;
                }
            }

            List<int> result = new List<int>(best.Genomes);
            result.Insert(0, 0);
            return new SimpleAsymmetricRoute(result, true);
        }

        /// <summary>
        /// Stops executiong.
        /// </summary>
        public void Stop()
        {
            _stopped = true;
        }

        #region Intermidiate Results

        /// <summary>
        /// Raised when an intermidiate result is available.
        /// </summary>
        public event SolverDelegates.IntermidiateDelegate IntermidiateResult;

        /// <summary>
        /// Returns true when the event has to be raised.
        /// </summary>
        /// <returns></returns>
        protected bool CanRaiseIntermidiateResult()
        {
            return this.IntermidiateResult != null;
        }

        /// <summary>
        /// Raises the intermidiate results event.
        /// </summary>
        /// <param name="result"></param>
        protected void RaiseIntermidiateResult(int[] result, float weight)
        {
            if (IntermidiateResult != null)
            {
                this.IntermidiateResult(result, weight);
            }
        }

        #endregion
    }
}
