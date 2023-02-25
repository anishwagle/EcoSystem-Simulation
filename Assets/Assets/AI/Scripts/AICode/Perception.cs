using System;

namespace FeedForwardWithGeneticAlgorithm
{
    public enum PerceptionType
    {
        Input,Output,Hidden
    }
    public class Perception
    {
        private Random rand = new Random();
        // The name of the perception
        public int Id { get; set; }

        // The bias value of the perception
        public double Bias { get; set; }

        // The type of the perception (input, hidden, or output)
        public PerceptionType Type { get; set; }
        public double Order { get; set; }
        public double Output { get; set; }
        public Perception() { }
        // Constructor for creating a new perception
        public Perception(int id, PerceptionType type)
        {
            this.Id = id;
            this.Bias = (rand.NextDouble() * 2) - 1;
            this.Type = type;
            this.Order = 0;
        }
    }
}
