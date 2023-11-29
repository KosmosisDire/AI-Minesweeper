using ProtoEngine;
using ProtoEngine.UI;
using ProtoEngine.Utils;

namespace Minesweeper.Solvers;


public class GeneticSolver : Solver
{
    public float fitnessAverage;
    public float fitnessMax;
    public float fitnessMin;


    public bool IsSolved { get; private set;}
    public Chromosome bestChromosome;

    public class Chromosome
    {
        public float[,] bombProbabilities;
        public float fitness;

        public void Mutate(float chance, float strength)
        {
            for (int x = 0; x < bombProbabilities.GetLength(0); x++)
            {
                for (int y = 0; y < bombProbabilities.GetLength(1); y++)
                {
                    if (Application.random.NextSingle() < chance)
                    {
                        bombProbabilities[x, y] += Application.random.NextSingle() * strength - strength / 2f;
                        bombProbabilities[x, y] = Math.Clamp(bombProbabilities[x, y], 0, 1);
                    }
                }
            }
        }

        public static Chromosome Crossover(Chromosome parent1, Chromosome parent2)
        {
            var newChromosome = new Chromosome
            {
                bombProbabilities = new float[parent1.bombProbabilities.GetLength(0), parent1.bombProbabilities.GetLength(1)],
                fitness = 0
            };

            for (int x = 0; x < parent1.bombProbabilities.GetLength(0); x++)
            {
                for (int y = 0; y < parent1.bombProbabilities.GetLength(1); y++)
                {
                    newChromosome.bombProbabilities[x, y] = (parent1.bombProbabilities[x, y] + parent2.bombProbabilities[x, y]) / 2f;
                }
            }

            return newChromosome;
        }
    }

    public List<Chromosome> chromosomes = new List<Chromosome>();

    public GeneticSolver(MinesweeperGrid grid) : base(grid)
    {
    }

    private void CreateInitialPopulation(int populationSize)
    {
        for (int i = 0; i < populationSize; i++)
        {
            var chromosome = new Chromosome
            {
                bombProbabilities = new float[grid.numColumns, grid.numRows],
                fitness = 0
            };

            for (int x = 0; x < grid.numColumns; x++)
            {
                for (int y = 0; y < grid.numRows; y++)
                {
                    chromosome.bombProbabilities[x, y] = (float)Application.random.NextSingle();
                }
            }

            chromosomes.Add(chromosome);
        }
    }

    private List<(Chromosome, Chromosome)> SelectParents(float percentage01, float tournamentSizePercentage01)
    {
        var parents = new List<(Chromosome, Chromosome)>();
        var parentCount = (int)(chromosomes.Count * percentage01);
        var tournamentSize = (int)(chromosomes.Count * tournamentSizePercentage01);

        for (int i = 0; i < parentCount; i++)
        {
            var parent1 = chromosomes[Application.random.Next(0, chromosomes.Count)];
            var parent2 = chromosomes[Application.random.Next(0, chromosomes.Count)];

            for (int j = 0; j < tournamentSize; j++)
            {
                var chromosome = chromosomes[Application.random.Next(0, chromosomes.Count)];
                if (chromosome.fitness < parent1.fitness)
                {
                    parent1 = chromosome;
                }
            }

            for (int j = 0; j < tournamentSize; j++)
            {
                var chromosome = chromosomes[Application.random.Next(0, chromosomes.Count)];

                if (chromosome == parent1)
                {
                    j--;
                    continue;
                }

                if (chromosome.fitness < parent2.fitness)
                {
                    parent2 = chromosome;
                }
            }

            parents.Add((parent1, parent2));
        }

        return parents;
    }

    private void CalculateFitness()
    {
        foreach (var chromosome in chromosomes)
        {
            var bombProbabilities = chromosome.bombProbabilities;
            var fitness = 0f;

            for (int x = 0; x < grid.numColumns; x++)
            {
                for (int y = 0; y < grid.numRows; y++)
                {
                    var cell = grid.GetCell(x, y);

                    var probability = bombProbabilities[x, y];

                    var realProbability = cell.isMine ? 1 : 0;
                    
                    var error = realProbability - probability;
                    fitness += error * error;
                }
            }

            fitness /= grid.numColumns * grid.numRows;
            fitness = MathF.Sqrt(fitness);

            chromosome.fitness = fitness;
        }

        chromosomes.Sort((a, b) => a.fitness.CompareTo(b.fitness)); // The lowest fitness is sorted to the front

        bestChromosome = chromosomes[0];

        fitnessAverage = chromosomes.Average(chromosome => chromosome.fitness);
        fitnessMax = chromosomes.Max(chromosome => chromosome.fitness);
        fitnessMin = chromosomes.Min(chromosome => chromosome.fitness);
    }

    private void CullPopulation(float percentage01)
    {
        chromosomes.RemoveRange(chromosomes.Count - (int)(chromosomes.Count * percentage01), (int)(chromosomes.Count * percentage01));
    }

    private void RunGeneration()
    {
        Console.WriteLine($"Running generation {chromosomes.Count}");
        
        CalculateFitness();
        var parents = SelectParents(0.3f, 0.1f);

        var newChromosomes = new List<Chromosome>();

        foreach (var (parent1, parent2) in parents)
        {
            var newChromosome = Chromosome.Crossover(parent1, parent2);
            newChromosome.Mutate(0.1f, 0.1f);
            newChromosomes.Add(newChromosome);
        }

        CalculateFitness();
        CullPopulation(0.3f);

        chromosomes.AddRange(newChromosomes);
    }

    public void Solve(CancellationToken token = default)
    {
        CreateInitialPopulation(20);

        for (int i = 0; i < 10000; i++)
        {
            if (token.IsCancellationRequested)
            {
                CalculateFitness();
                return;
            }

            RunGeneration();
        }

        CalculateFitness();

        IsSolved = true;
    }

    public override (bool failed, MinesweeperCell move) GetNextMove(CancellationToken token = default)
    {
        if (!IsSolved)
        {
            Solve(token);
        }

        var bestBombProbabilities = bestChromosome.bombProbabilities;

        //put probability onto each cell
        // grid.unrevealedCells.ForEach((cell) => 
        // {
        //     if (cell.IsFlagged) return;
        //     var (x, y) = (cell.x, cell.y);
        //     cell.countText.Text = bestBombProbabilities[x, y].ToString("N1");
        //     if(cell.isMine) cell.SetIcon(Properties.Resources.mine);
        // });

        var maxProbability = 0f;
        var maxProbabilityCoords = (0, 0);
        var minProbability = 1f;
        var minProbabilityCoords = (0, 0);

        // find the best and worst choices, flag the worst, move on the best
        for (int x = 0; x < grid.numColumns; x++)
        {
            for (int y = 0; y < grid.numRows; y++)
            {
                var cell = grid.GetCell(x, y);
                if (cell.IsRevealed || cell.IsFlagged) continue;

                var probability = bestBombProbabilities[x, y];
                if (probability > maxProbability)
                {
                    maxProbability = probability;
                    maxProbabilityCoords = (x, y);
                }

                if (probability < minProbability)
                {
                    minProbability = probability;
                    minProbabilityCoords = (x, y);
                }
            }
        }

        var move = grid.GetCell(minProbabilityCoords.Item1, minProbabilityCoords.Item2);
        var flag = grid.GetCell(maxProbabilityCoords.Item1, maxProbabilityCoords.Item2);

        if (maxProbability >= 0.7) flag.Flag();

        if (move.isMine) 
        {
            return (true, move); //Stop checking if mine is found
        }

        return (false, move);
    }
}