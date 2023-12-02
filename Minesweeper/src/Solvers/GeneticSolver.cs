using ProtoEngine;
using ProtoEngine.UI;
using ProtoEngine.Utils;
using SFML.Graphics;

namespace Minesweeper.Solvers;


public class GeneticSolver : Solver
{
    public float fitnessAverage;
    public float fitnessMax;
    public float fitnessMin;

    public bool useWoC = false;

    public event Action OnGenerationComplete;

    public bool IsSolved { get; private set;}
    public Chromosome bestChromosome;

    public class Chromosome
    {
        public float[,] bombProbabilities;
        public float[,] probabilityErrors;
        public float fitness;

        public int MineCount => bombProbabilities.Cast<float>().Count(probability => probability > 0.5f);

        public Chromosome(int sizeX, int sizeY)
        {
            bombProbabilities = new float[sizeX, sizeY];
            probabilityErrors = new float[sizeX, sizeY];
            fitness = 0;
        }
        
        public void Mutate(float chance, float strength)
        {
            for (int x = 0; x < bombProbabilities.GetLength(0); x++)
            {
                for (int y = 0; y < bombProbabilities.GetLength(1); y++)
                {
                    if (Application.random.NextSingle() < chance)
                    {
                        var strengthMod = strength * probabilityErrors[x, y];
                        bombProbabilities[x, y] += (Application.random.NextSingle() - 0.5f) * strengthMod;
                        bombProbabilities[x, y] = Math.Clamp(bombProbabilities[x, y], 0, 1);
                    }
                }
            }
        }

        public static Chromosome Crossover(Chromosome parent1, Chromosome parent2)
        {
            var newChromosome = new Chromosome(parent1.bombProbabilities.GetLength(0), parent1.bombProbabilities.GetLength(1));

            for (int x = 0; x < parent1.bombProbabilities.GetLength(0); x++)
            {
                for (int y = 0; y < parent1.bombProbabilities.GetLength(1); y++)
                {
                    newChromosome.bombProbabilities[x, y] = (parent1.bombProbabilities[x, y] + parent2.bombProbabilities[x, y]) / 2f;
                    newChromosome.probabilityErrors[x, y] = (parent1.probabilityErrors[x, y] + parent2.probabilityErrors[x, y]) / 2f;
                }
            }

            return newChromosome;
        }

        public bool AllFlagsCorrect(MinesweeperGrid grid)
        {
            foreach (var cell in grid.cells)
            {
                var (x, y) = (cell.x, cell.y);
                if (bombProbabilities[x, y] > 0.5f) // will be flagged
                {
                    if (!cell.isMine) return false;
                }
                else // will not be flagged
                {
                    if (cell.isMine) return false;
                }
            }

            return true;
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
            var chromosome = new Chromosome(grid.numColumns, grid.numRows);

            for (int x = 0; x < grid.numColumns; x++)
            {
                for (int y = 0; y < grid.numRows; y++)
                {
                    chromosome.bombProbabilities[x, y] = Application.random.NextSingle() * 0.1f;
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

    public void DebugSolution()
    {
        foreach (var cell in grid.cells)
        {
            var (x, y) = (cell.x, cell.y);
            var probability = bestChromosome.bombProbabilities[x, y];

            cell.card.Style.fillColor = new Color(35, 68, 83).Lerp(new Color(0, 122, 204), probability);

            cell.card.Box.FillColor = cell.card.Style.fillColor;
        }

        // grid.unrevealedCells.ForEach((cell) => 
        // {
        //     if (cell.IsFlagged) return;
        //     var (x, y) = (cell.x, cell.y);
        //     cell.countText.Text = bestChromosome.bombProbabilities[x, y].ToString("N1");
        //     // if(cell.isMine) cell.SetIcon(Properties.Resources.mine);
        // });
    }

    private void CalculateFitness()
    {
        foreach (var chromosome in chromosomes)
        {
            var fitness = 0f;

            // we only allow the fitness function to check as many cells as there are bombs otherwise it is given more information than possible in one game
            var possibleCells = new List<MinesweeperCell>();
            possibleCells.AddRange(grid.cells);


            // choose a random set of cells to check
            var len = possibleCells.Count;
            for (int i = 0; i < 99; i++)
            {
                var cell = possibleCells[Application.random.Next(0, possibleCells.Count)];
                possibleCells.Remove(cell);

                var (x, y) = (cell.x, cell.y);
                var probability = chromosome.bombProbabilities[x, y];
                var realProbability = cell.isMine ? 1 : 0;
                var error = realProbability - probability;

                chromosome.probabilityErrors[x, y] = error;
                fitness += error * error;
            }

            fitness /= 99;
            fitness = MathF.Sqrt(fitness);

            chromosome.fitness = fitness;
        }

        chromosomes.Sort((a, b) => a.fitness.CompareTo(b.fitness)); // The lowest fitness is sorted to the front

        bestChromosome = chromosomes[0];

        fitnessAverage = chromosomes.Average(chromosome => chromosome.fitness);
        fitnessMax = chromosomes.Max(chromosome => chromosome.fitness);
        fitnessMin = chromosomes.Min(chromosome => chromosome.fitness);
    }

    public Chromosome GetWoC(int topNum)
    {
        var chromosome = new Chromosome(grid.numColumns, grid.numRows);

        var top = chromosomes.Take(topNum).ToList();
        var weights = new List<float>();

        top.ForEach((c) => weights.Add(1 / (c.fitness + 1)));
        var weightSum = weights.Sum();
        weights = weights.Select((w) => w / weightSum).ToList();

        for (int i = 0; i < topNum; i++)
        {
            var c = top[i];
            for (int x = 0; x < grid.numColumns; x++)
            {
                for (int y = 0; y < grid.numRows; y++)
                {
                    chromosome.bombProbabilities[x, y] += c.bombProbabilities[x, y] * weights[i];
                    chromosome.probabilityErrors[x, y] += c.probabilityErrors[x, y] * weights[i];
                }
            }
        };

        return chromosome;
    }

    private void CullPopulation(int maxPopulation)
    {
        chromosomes.RemoveRange(maxPopulation, chromosomes.Count - maxPopulation);
    }

    private void RunGeneration()
    {
        if (useWoC) 
        {
            var woc = GetWoC(5);
            chromosomes[^1] = woc;
        }

        var parents = SelectParents(0.5f, 0.2f);

        var newChromosomes = new List<Chromosome>();

        foreach (var (parent1, parent2) in parents)
        {
            var newChromosome = Chromosome.Crossover(parent1, parent2);
            newChromosome.Mutate(fitnessMin, 2 * fitnessMin);
            newChromosomes.Add(newChromosome);
        }

        chromosomes.AddRange(newChromosomes);

        CalculateFitness();
        CullPopulation(20); 

        DebugSolution();
    }

    public void Solve(CancellationToken token = default)
    {
        CreateInitialPopulation(20);

        CalculateFitness();

        while (true)
        {
            

            RunGeneration();
            OnGenerationComplete?.Invoke();

            if (fitnessMin < 0.05f || bestChromosome.AllFlagsCorrect(grid)) break;

            if (token.IsCancellationRequested)
            {
                return;
            }
        }

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

        if (maxProbability >= 0.5) flag.Flag();

        if (move.isMine) 
        {
            return (true, move); //Stop checking if mine is found
        }

        return (false, move);
    }
}